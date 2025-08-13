using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Testcontainers.Keycloak;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using Testcontainers.Redis;

namespace Evently.IntegrationTests.Abstractions;

#pragma warning disable CA1515
public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
#pragma warning restore CA1515
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:17.5")
        .WithDatabase("evently")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();
    private readonly RedisContainer _redisContainer = new RedisBuilder()
        .WithImage("redis:8.0.2")
        .Build();
    private readonly KeycloakContainer _keycloakContainer = new KeycloakBuilder()
        .WithImage("quay.io/keycloak/keycloak:26.2.4")
        .WithResourceMapping(
            new FileInfo(Path.Combine(
                Directory.GetCurrentDirectory(),
                "..",
                "..",
                "..", 
                "..",
                "..", 
                ".files", 
                "evently-realm-export.json"
            )),
            new FileInfo("/opt/keycloak/data/import/realm.json"))
        .WithCommand("--import-realm")
        .Build();
    private readonly RabbitMqContainer _rabbitMqContainer = new RabbitMqBuilder()
        .WithImage("rabbitmq:4.1.3-management-alpine")
        .WithUsername("guest")
        .WithPassword("guest")
        .Build();
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            EnvironmentVariablesConfigurationSource? environmentSource = configBuilder.Sources.OfType<EnvironmentVariablesConfigurationSource>().FirstOrDefault();
            if (environmentSource != null)
            {
                configBuilder.Sources.Remove(environmentSource);
            }
            configBuilder.AddEnvironmentVariables();
        });
        
        string keyCloakAddress = _keycloakContainer.GetBaseAddress();
        string keyCloakRealmUrl = $"{keyCloakAddress}realms/evently";
        Environment.SetEnvironmentVariable("Authentication:MetadataAddress", $"{keyCloakRealmUrl}/.well-known/openid-configuration");
        Environment.SetEnvironmentVariable("Authentication:TokenValidationParameters:ValidIssuers", keyCloakRealmUrl);
        Environment.SetEnvironmentVariable("Users:KeyCloak:AdminUrl", $"{keyCloakAddress}admin/realms/evently/");
        Environment.SetEnvironmentVariable("Users:KeyCloak:TokenUrl", $"{keyCloakRealmUrl}/protocol/openid-connect/token");
        Environment.SetEnvironmentVariable("ConnectionStrings:Database", _dbContainer.GetConnectionString());
        Environment.SetEnvironmentVariable("ConnectionStrings:Cache", _redisContainer.GetConnectionString());
        Environment.SetEnvironmentVariable("ConnectionStrings:Queue", _rabbitMqContainer.GetConnectionString());
        Environment.SetEnvironmentVariable("Users:Outbox:IntervalInSeconds", "5");
        Environment.SetEnvironmentVariable("Users:InBox:IntervalInSeconds", "5");
        Environment.SetEnvironmentVariable("Events:Outbox:IntervalInSeconds", "5");
        Environment.SetEnvironmentVariable("Events:InBox:IntervalInSeconds", "5");
        Environment.SetEnvironmentVariable("Ticketing:Outbox:IntervalInSeconds", "5");
        Environment.SetEnvironmentVariable("Ticketing:InBox:IntervalInSeconds", "5");
        Environment.SetEnvironmentVariable("Attendance:Outbox:IntervalInSeconds", "5");
        Environment.SetEnvironmentVariable("Attendance:InBox:IntervalInSeconds", "5");
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        await _redisContainer.StartAsync();
        await _keycloakContainer.StartAsync();
        await _rabbitMqContainer.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
        await _redisContainer.StopAsync();
        await _keycloakContainer.StopAsync();
        await _rabbitMqContainer.StopAsync();
    }
}
