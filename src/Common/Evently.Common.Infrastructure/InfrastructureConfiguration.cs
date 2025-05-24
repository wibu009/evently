using System.Reflection;
using System.Runtime.Loader;
using Evently.Common.Application.Caching;
using Evently.Common.Application.Clock;
using Evently.Common.Application.Data;
using Evently.Common.Application.EventBus;
using Evently.Common.Infrastructure.Authentication;
using Evently.Common.Infrastructure.Authorization;
using Evently.Common.Infrastructure.Caching;
using Evently.Common.Infrastructure.Clock;
using Evently.Common.Infrastructure.Data;
using Evently.Common.Infrastructure.Interceptors;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using StackExchange.Redis;

namespace Evently.Common.Infrastructure;

public static class InfrastructureConfiguration
{
    public static void AddCoreServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        #region Clock

        services.TryAddSingleton<IDateTimeProvider, DateTimeProvider>();

        #endregion

        #region Data

        string databaseConnectionString = configuration.GetConnectionString("Database")!;
        
        NpgsqlDataSource npgsqlDataSource = new NpgsqlDataSourceBuilder(databaseConnectionString).Build();
        services.TryAddSingleton(npgsqlDataSource);

        services.AddScoped<IDbConnectionFactory, DbConnectionFactory>();

        #endregion

        #region Caching

        string redisConnectionString = configuration.GetConnectionString("Redis")!;

        try
        {
            IConnectionMultiplexer connectionMultiplexer = ConnectionMultiplexer.Connect(redisConnectionString);
            services.TryAddSingleton(connectionMultiplexer);
        
            services.AddStackExchangeRedisCache(options => 
                options.ConnectionMultiplexerFactory = () => Task.FromResult(connectionMultiplexer));
        }
        catch
        {
            services.AddDistributedMemoryCache();
        }
        
        services.AddHybridCache();
        
        services.TryAddSingleton<ICacheService, CacheService>();

        #endregion
        
        #region Interceptors
        
        services.TryAddSingleton<PublishDomainEventsInterceptor>();
        
        #endregion
        
        #region EventBus

        services.AddMassTransit(configure =>
        {
            configure.UsingInMemory((context, cfg) =>
            {
                cfg.ConfigureEndpoints(context);
            });
        });
        
        services.TryAddSingleton<IEventBus, EventBus.EventBus>();
        
        #endregion

        #region Authentication

        services.AddAuthenticationInternal();

        #endregion

        #region Authorization

        services.AddAuthorizationInternal();

        #endregion
    }
}
