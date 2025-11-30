using Testcontainers.Keycloak;
using Testcontainers.MongoDb;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using Testcontainers.Redis;

namespace Evently.IntegrationTests.Abstractions;

#pragma warning disable CA1515
public sealed class TestEnvironment : IAsyncLifetime
#pragma warning restore CA1515
{
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder()
        .WithImage("postgres:17.5")
        .WithDatabase("evently")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();
    private readonly MongoDbContainer _mongoDbContainer = new MongoDbBuilder()
        .WithImage("mongo:8.2")
        .WithUsername("admin")
        .WithPassword("admin")
        .Build();
    private readonly RedisContainer _redisContainer = new RedisBuilder()
        .WithImage("redis:8.0.2")
        .Build();
    private readonly KeycloakContainer _keycloakContainer = new KeycloakBuilder()
        .WithImage("quay.io/keycloak/keycloak:26.2.4")
        .WithResourceMapping(
            new FileInfo(Path.Combine(
                Directory.GetCurrentDirectory(),
                "..","..","..","..","..",".files","evently-realm-export.json")),
            new FileInfo("/opt/keycloak/data/import/realm.json"))
        .WithCommand("--import-realm")
        .Build();
    private readonly RabbitMqContainer _rabbitMqContainer = new RabbitMqBuilder()
        .WithImage("rabbitmq:4.1.3-management-alpine")
        .WithUsername("guest")
        .WithPassword("guest")
        .Build();

    public async Task InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();
        await _mongoDbContainer.StartAsync();
        await _redisContainer.StartAsync();
        await _keycloakContainer.StartAsync();
        await _rabbitMqContainer.StartAsync();
        
        // Common environment variables used by both services.
        string keycloakAddress = _keycloakContainer.GetBaseAddress();
        string realmUrl = $"{keycloakAddress}realms/evently";
        Environment.SetEnvironmentVariable("Authentication:MetadataAddress", $"{realmUrl}/.well-known/openid-configuration");
        Environment.SetEnvironmentVariable("Authentication:TokenValidationParameters:ValidIssuers", realmUrl);
        Environment.SetEnvironmentVariable("Users:KeyCloak:AdminUrl", $"{keycloakAddress}admin/realms/evently/");
        Environment.SetEnvironmentVariable("Users:KeyCloak:TokenUrl", $"{realmUrl}/protocol/openid-connect/token");
        Environment.SetEnvironmentVariable("ConnectionStrings:WriteDatabase", _postgreSqlContainer.GetConnectionString());
        Environment.SetEnvironmentVariable("ConnectionStrings:ReadDatabase", _mongoDbContainer.GetConnectionString());
        Environment.SetEnvironmentVariable("ConnectionStrings:Cache", _redisContainer.GetConnectionString());
        Environment.SetEnvironmentVariable("ConnectionStrings:Queue", _rabbitMqContainer.GetConnectionString());
        
        // Outbox/Inbox poll intervals
        foreach (string module in new[]{ "Users","Events","Ticketing","Attendance" })
        {
            Environment.SetEnvironmentVariable($"{module}:Outbox:IntervalInSeconds", "5");
            Environment.SetEnvironmentVariable($"{module}:InBox:IntervalInSeconds", "5");
        }
    }

    public async Task DisposeAsync()
    {
        await _rabbitMqContainer.StopAsync();
        await _keycloakContainer.StopAsync();
        await _redisContainer.StopAsync();
        await _mongoDbContainer.StopAsync();
        await _postgreSqlContainer.StopAsync();
    }
}
