using Dapper;
using Evently.Common.Application.Authentication;
using Evently.Common.Application.Caching;
using Evently.Common.Application.Clock;
using Evently.Common.Application.Data;
using Evently.Common.Application.EventBus;
using Evently.Common.Infrastructure.Authentication;
using Evently.Common.Infrastructure.Authorization;
using Evently.Common.Infrastructure.Caching;
using Evently.Common.Infrastructure.Clock;
using Evently.Common.Infrastructure.Configuration;
using Evently.Common.Infrastructure.Data;
using Evently.Common.Infrastructure.Outbox;
using MassTransit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using Quartz;
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

        string databaseConnectionString = configuration.GetConnectionStringOrThrow("Database");
        
        NpgsqlDataSource npgsqlDataSource = new NpgsqlDataSourceBuilder(databaseConnectionString).Build();
        services.TryAddSingleton(npgsqlDataSource);

        services.AddScoped<IDbConnectionFactory, DbConnectionFactory>();
        
        SqlMapper.AddTypeHandler(new GenericArrayHandler<string>());

        #endregion

        #region Caching

        string redisConnectionString = configuration.GetConnectionStringOrThrow("Cache");

        try
        {
            IConnectionMultiplexer connectionMultiplexer = ConnectionMultiplexer.Connect(redisConnectionString);
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
        
        #region Outbox
        
        services.TryAddSingleton<InsertOutboxMessagesInterceptor>();
        
        #endregion
        
        #region EventBus

        services.AddMassTransit(configure =>
        {
            configure.SetKebabCaseEndpointNameFormatter();
            configure.UsingInMemory((context, cfg) =>
            {
                cfg.ConfigureEndpoints(context);
            });
        });
        
        services.TryAddSingleton<IEventBus, EventBus.EventBus>();
        
        #endregion

        #region Authentication

        services.AddAuthorization();
        services.AddAuthentication().AddJwtBearer();
        services.AddHttpContextAccessor();

        services.ConfigureOptions<JwtBearerConfigureOptions>();

        services.AddScoped<ICurrentActor, CurrentActor>();

        #endregion

        #region Authorization

        services.AddTransient<IClaimsTransformation, CustomClaimsTransformation>();
        services.AddTransient<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddTransient<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();

        #endregion

        #region Background Jobs

        services.AddQuartz();
        services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

        #endregion
    }
}
