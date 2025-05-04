using Microsoft.Extensions.Caching.Hybrid;

namespace Evently.Common.Infrastructure.Caching;

public static class CacheOptions
{
    public static HybridCacheEntryOptions DefaultExpiration => new()
    {
        LocalCacheExpiration = TimeSpan.FromMinutes(5),
        Expiration = TimeSpan.FromMinutes(30)
    };
    
    public static HybridCacheEntryOptions Create(TimeSpan? expiration = null, TimeSpan? localExpiration = null)
    {
        var options = new HybridCacheEntryOptions
        {
            LocalCacheExpiration = localExpiration ?? DefaultExpiration.LocalCacheExpiration,
            Expiration = expiration ?? DefaultExpiration.Expiration
        };
        return options;
    }
}
