using Evently.Common.Application.Caching;
using Microsoft.Extensions.Caching.Hybrid;

namespace Evently.Common.Infrastructure.Caching;

internal sealed class CacheService(HybridCache hybridCache) : ICacheService
{
    public async ValueTask<T> GetOrCreateAsync<T>(string key, Func<CancellationToken, ValueTask<T>> factory,
        TimeSpan? expiration, TimeSpan? localCacheExpiration, IEnumerable<string>? tags, CancellationToken cancellationToken = default)
    {
        HybridCacheEntryOptions options = CacheOptions.Create(expiration, localCacheExpiration);
        return await hybridCache.GetOrCreateAsync(key, factory, options, tags, cancellationToken);
    }

    public async ValueTask RemoveAsync(string key, CancellationToken cancellationToken = default) =>
        await hybridCache.RemoveAsync(key, cancellationToken);

    public async ValueTask RemoveByTagAsync(string tag, CancellationToken cancellationToken = default) =>
        await hybridCache.RemoveByTagAsync(tag, cancellationToken);
}
