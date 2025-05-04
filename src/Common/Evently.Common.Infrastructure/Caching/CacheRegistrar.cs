using Evently.Common.Application.Caching;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StackExchange.Redis;

namespace Evently.Common.Infrastructure.Caching;

internal static class CacheRegistrar
{
    public static void AddCaching(this IServiceCollection services, string redisConnectionString)
    {
        IConnectionMultiplexer connectionMultiplexer = ConnectionMultiplexer.Connect(redisConnectionString);
        services.TryAddSingleton(connectionMultiplexer);
        
        services.AddStackExchangeRedisCache(options => 
            options.ConnectionMultiplexerFactory = () => Task.FromResult(connectionMultiplexer));
        services.AddHybridCache();
        
        services.TryAddSingleton<ICacheService, CacheService>();
    }
}
