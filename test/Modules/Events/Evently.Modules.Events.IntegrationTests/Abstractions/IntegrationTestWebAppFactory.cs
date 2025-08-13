using Evently.Common.Application.Clock;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using Testcontainers.Redis;

namespace Evently.Modules.Events.IntegrationTests.Abstractions;

#pragma warning disable CA1515
public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
#pragma warning restore CA1515
{
    public readonly IDateTimeProvider DateTimeProviderMock = Substitute.For<IDateTimeProvider>();
    
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:17.5")
        .WithDatabase("evently")
        .WithUsername("postgres")
        .WithPassword("postgres")
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
        });
        
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IDateTimeProvider>();
            
            DateTimeProviderMock.UtcNow.Returns(_ => DateTime.UtcNow);
            services.AddSingleton(DateTimeProviderMock);
        });
        
        Environment.SetEnvironmentVariable("ConnectionStrings:Database", _dbContainer.GetConnectionString());
        Environment.SetEnvironmentVariable("ConnectionStrings:Cache", _redisContainer.GetConnectionString());
        Environment.SetEnvironmentVariable("ConnectionStrings:Queue", _rabbitMqContainer.GetConnectionString());
    }
    
    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        await _redisContainer.StartAsync();
        await _rabbitMqContainer.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
        await _redisContainer.StopAsync();
        await _rabbitMqContainer.StopAsync();
    }
}
