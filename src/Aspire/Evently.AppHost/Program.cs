// Evently distributed application definition.
//
// Run the whole system (APIs, gateway, web UI, and all infrastructure) with:
//     dotnet run --project src/Aspire/Evently.AppHost
// and open the Aspire dashboard it prints for unified logs, traces, and health.
//
// Addressing strategy: only Keycloak keeps a fixed host port (18080) because
// browsers, JWT issuers, and the web .env all address it by URL. Everything
// else uses dynamic ports resolved through endpoint references below, so the
// AppHost never fights over ports and never depends on container hostnames.

using Aspire.Hosting.ApplicationModel;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

// Keycloak ships its realm under .files at the repository root.
string realmImportDir = Path.GetFullPath(Path.Combine(builder.AppHostDirectory, "..", "..", "..", ".files"));

const string KeycloakRealm = "http://localhost:18080/realms/evently";

// ---------------------------------------------------------------------------
// Infrastructure
// ---------------------------------------------------------------------------

IResourceBuilder<ParameterResource> postgresPassword =
    builder.AddParameter("postgres-password", "postgres", secret: true);

IResourceBuilder<PostgresServerResource> postgres =
    builder
        .AddPostgres("postgres", password: postgresPassword)
        .WithImage("postgres", "17.5")
        .WithDataVolume();

// Single shared database (per-module schemas), like docker-compose.
IResourceBuilder<PostgresDatabaseResource> writeDb = postgres.AddDatabase("writedb", "evently");

IResourceBuilder<MongoDBServerResource> mongo = builder
    .AddMongoDB(
        "readdatabase",
        userName: builder.AddParameter("mongo-username", "admin"),
        password: builder.AddParameter("mongo-password", "admin", secret: true))
    .WithImage("mongo", "8.2")
    .WithDataVolume();

IResourceBuilder<RedisResource> cache = builder
    .AddRedis("cache")
    .WithImage("redis", "8.0.2")
    .WithDataVolume();

IResourceBuilder<RabbitMQServerResource> queue = builder
    .AddRabbitMQ(
        "queue",
        userName: builder.AddParameter("rabbit-username", "guest"),
        password: builder.AddParameter("rabbit-password", "guest", secret: true))
    .WithImage("rabbitmq", "4.1.3-management-alpine")
    .WithDataVolume()
    // The -management-alpine image already ships the plugin; WithManagementPlugin
    // rejects non-default tags, so the UI port is published explicitly instead.
    .WithHttpEndpoint(targetPort: 15672, name: "management");

IResourceBuilder<ContainerResource> keycloak = builder
    .AddContainer("keycloak", "quay.io/keycloak/keycloak", "26.4.0")
    .WithHttpEndpoint(port: 18080, targetPort: 8080, name: "http")
    .WithHttpEndpoint(targetPort: 9000, name: "management")
    .WithEnvironment("KEYCLOAK_ADMIN", "admin")
    .WithEnvironment("KEYCLOAK_ADMIN_PASSWORD", "admin")
    .WithEnvironment("KC_HEALTH_ENABLED", "true")
    .WithEnvironment("KC_METRICS_ENABLED", "true")
    .WithBindMount(realmImportDir, "/opt/keycloak/data/import")
    .WithVolume("evently-keycloak-data", "/opt/keycloak/data")
    .WithArgs("start-dev", "--import-realm")
    // Health-check the management port so WaitFor(keycloak) blocks until the IdP
    // actually serves HTTP — user seeding at API startup needs it fully ready.
    .WithHttpHealthCheck("/health/", endpointName: "management");

IResourceBuilder<ContainerResource> seq = builder
    .AddContainer("seq", "datalust/seq", "2025.1")
    .WithHttpEndpoint(name: "http", targetPort: 80)
    .WithHttpEndpoint(targetPort: 5341, name: "ingestion")
    .WithEnvironment("ACCEPT_EULA", "Y");

// Jaeger is intentionally not hosted here: OpenTelemetry exporters in the APIs
// automatically target the Aspire dashboard, which replaces Jaeger for traces.
// (docker-compose still runs Jaeger for the container workflow.)

// ---------------------------------------------------------------------------
// Applications
// ---------------------------------------------------------------------------

EndpointReference keycloakHttp = keycloak.GetEndpoint("http");
EndpointReference keycloakManagement = keycloak.GetEndpoint("management");
EndpointReference seqIngestion = seq.GetEndpoint("ingestion");

IResourceBuilder<ProjectResource> api = builder
    .AddProject<Projects.Evently_Api>("evently-api")
    .WithReference(writeDb)
    .WithReference(mongo)
    .WithReference(cache)
    .WithReference(queue)
    .WaitFor(writeDb)
    .WaitFor(keycloak)
    .WithEnvironment("Users__KeyCloak__AdminUrl", ReferenceExpression.Create($"{keycloakHttp}/admin/realms/evently/"))
    .WithEnvironment("Users__KeyCloak__TokenUrl", ReferenceExpression.Create($"{keycloakHttp}/realms/evently/protocol/openid-connect/token"))
    .WithEnvironment("Authentication__MetadataAddress", ReferenceExpression.Create($"{keycloakHttp}/realms/evently/.well-known/openid-configuration"))
    .WithEnvironment("Authentication__TokenValidationParameters__ValidIssuers__0", ReferenceExpression.Create($"{keycloakHttp}/realms/evently"))
    .WithEnvironment("Authentication__TokenValidationParameters__ValidIssuers__1", "http://localhost:18080/realms/evently")
    .WithEnvironment("KeyCloak__HealthUrl", ReferenceExpression.Create($"{keycloakManagement}/health/"))
    .WithEnvironment("Serilog__WriteTo__1__Args__serverUrl", seqIngestion);

IResourceBuilder<ProjectResource> ticketingApi = builder
    .AddProject<Projects.Evently_Ticketing_Api>("evently-ticketing-api")
    .WithReference(writeDb)
    .WithReference(mongo)
    .WithReference(cache)
    .WithReference(queue)
    .WaitFor(writeDb)
    .WaitFor(keycloak)
    .WithEnvironment("Authentication__MetadataAddress", ReferenceExpression.Create($"{keycloakHttp}/realms/evently/.well-known/openid-configuration"))
    .WithEnvironment("Authentication__TokenValidationParameters__ValidIssuers__0", ReferenceExpression.Create($"{keycloakHttp}/realms/evently"))
    .WithEnvironment("Authentication__TokenValidationParameters__ValidIssuers__1", "http://localhost:18080/realms/evently")
    .WithEnvironment("KeyCloak__HealthUrl", ReferenceExpression.Create($"{keycloakManagement}/health/"))
    .WithEnvironment("Serilog__WriteTo__1__Args__serverUrl", seqIngestion);

// React storefront: Vite dev server with hot reload, env vars baked into the bundle.
// Proxyless endpoint so Vite itself binds the fixed port from vite.config.ts (5173) —
// browsers and the Keycloak client's redirect URIs address the UI by URL. (The default
// proxied mode makes DCP pre-bind the port and push Vite onto a random one.)
builder
    .AddExecutable("web", "npm", "../../Web", "run", "dev")
    .WithHttpEndpoint(port: 5173, name: "http")
    .WithEnvironment("VITE_API_BASE_URL", "http://localhost:5000")
    .WithEnvironment("VITE_TICKETING_BASE_URL", "http://localhost:5004")
    .WithEnvironment("VITE_OIDC_AUTHORITY", KeycloakRealm)
    .WithEnvironment("VITE_OIDC_CLIENT_ID", "evently-public-client")
    .WithEndpointProxySupport(false);

builder
    .AddProject<Projects.Evently_Gateway>("evently-gateway")
    .WithEnvironment("Authentication__MetadataAddress", ReferenceExpression.Create($"{keycloakHttp}/realms/evently/.well-known/openid-configuration"))
    .WithEnvironment("Authentication__TokenValidationParameters__ValidIssuers__0", ReferenceExpression.Create($"{keycloakHttp}/realms/evently"))
    .WithEnvironment("Authentication__TokenValidationParameters__ValidIssuers__1", "http://localhost:18080/realms/evently")
    .WithEnvironment("Serilog__WriteTo__1__Args__serverUrl", seqIngestion)
    .WithEnvironment("ReverseProxy__Clusters__evently-cluster__Destinations__default__Address", api.GetEndpoint("http"))
    .WithEnvironment("ReverseProxy__Clusters__evently-ticketing-cluster__Destinations__default__Address", ticketingApi.GetEndpoint("http"));

// Note: the React storefront is orchestrated above as a plain executable
// (`npm run dev`), keeping its port fixed at 5173 for browser + Keycloak redirects.

await builder.Build().RunAsync();
