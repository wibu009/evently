using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Testcontainers.MongoDb;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using Testcontainers.Redis;

namespace Evently.Modules.Ticketing.IntegrationTests.Abstractions;

#pragma warning disable CA1515
public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
#pragma warning restore CA1515
{
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder()
        .WithImage("postgres:17.5")
        .WithDatabase("evently_ticketing")
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

            // The saga state-machine tests drive each step explicitly (ProcessPayment, ExpireOrder, Restock...).
            // The background outbox/inbox/expiration jobs are slowed down so they cannot race the assertions.
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Ticketing:Outbox:IntervalInSeconds"] = "3600",
                ["Ticketing:Inbox:IntervalInSeconds"] = "3600",
                ["Ticketing:Orders:ExpirationIntervalInSeconds"] = "3600",
                ["SampleData:Enabled"] = "false"
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
