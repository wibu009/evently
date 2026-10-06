using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Testcontainers.MongoDb;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using Testcontainers.Redis;

namespace Evently.Modules.Attendance.IntegrationTests.Abstractions;

#pragma warning disable CA1515
public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
#pragma warning restore CA1515
{
    static IntegrationTestWebAppFactory()
    {
        // Ryuk (the Testcontainers resource reaper) is a convenience, not a requirement:
        // xUnit disposes the containers itself, and skipping it lets test runs work even
        // when the container registry is unreachable (offline / firewalled machines).
        Environment.SetEnvironmentVariable("TESTCONTAINERS_RYUK_DISABLED", "true");
    }

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

            // Keep the development sample data seeder out of the test databases.
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["SampleData:Enabled"] = "false",
                // No `acr` claims in the test auth flow — disable step-up (unit-tested).
                ["Authentication:StepUp:Enforced"] = "false"
            });
        });
        
        Environment.SetEnvironmentVariable("ConnectionStrings:writedb", _postgreSqlContainer.GetConnectionString());
        Environment.SetEnvironmentVariable("ConnectionStrings:readdatabase", _mongoDbContainer.GetConnectionString());
        Environment.SetEnvironmentVariable("ConnectionStrings:cache", _redisContainer.GetConnectionString());
        Environment.SetEnvironmentVariable("ConnectionStrings:queue", _rabbitMqContainer.GetConnectionString());
    }
    
    public async Task InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();
        await _mongoDbContainer.StartAsync();
        await _redisContainer.StartAsync();
        await _rabbitMqContainer.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await _postgreSqlContainer.StopAsync();
        await _mongoDbContainer.StopAsync();
        await _redisContainer.StopAsync();
        await _rabbitMqContainer.StopAsync();
    }
}
