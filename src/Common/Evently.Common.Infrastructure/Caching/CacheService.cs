using Evently.Common.Application.Caching;
using Microsoft.Extensions.Caching.Hybrid;

namespace Evently.Common.Infrastructure.Caching;

internal sealed class CacheService(HybridCache hybridCache) : ICacheService
{
    public async ValueTask<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        return await hybridCache.GetOrCreateAsync(
            key,
            _ => ValueTask.FromResult<T?>(default),
            new HybridCacheEntryOptions
            {
                Flags = HybridCacheEntryFlags.DisableLocalCacheWrite | HybridCacheEntryFlags.DisableDistributedCacheWrite
            },
            cancellationToken: cancellationToken);
    }

    public async ValueTask SetAsync<T>(string key, T value, TimeSpan? expiration = null, 
        TimeSpan? localCacheExpiration = null, IEnumerable<string>? tags = null, 
        CancellationToken cancellationToken = default)
    {
        HybridCacheEntryOptions options = CacheOptions.Create(expiration, localCacheExpiration);
        await hybridCache.SetAsync(key, value, options, tags, cancellationToken);
    }
    
    public async ValueTask<T> GetOrCreateAsync<T>(string key, Func<CancellationToken, ValueTask<T>> factory,
        TimeSpan? expiration, TimeSpan? localCacheExpiration, IEnumerable<string>? tags = null, CancellationToken cancellationToken = default)
    {
        HybridCacheEntryOptions options = CacheOptions.Create(expiration, localCacheExpiration);
        return await hybridCache.GetOrCreateAsync(key, factory, options, tags, cancellationToken);
    }

    public async ValueTask RemoveAsync(string key, CancellationToken cancellationToken = default) =>
        await hybridCache.RemoveAsync(key, cancellationToken);

    public async ValueTask RemoveByTagAsync(string tag, CancellationToken cancellationToken = default) =>
        await hybridCache.RemoveByTagAsync(tag, cancellationToken);
}
