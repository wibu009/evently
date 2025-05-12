namespace Evently.Common.Application.Caching;

public interface ICacheService
{
    ValueTask<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    ValueTask SetAsync<T>(string key, T value, TimeSpan? expiration = null,
        TimeSpan? localCacheExpiration = null, IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default);
    ValueTask<T> GetOrCreateAsync<T>(string key, Func<CancellationToken, ValueTask<T>> factory,
        TimeSpan? expiration, TimeSpan? localCacheExpiration, IEnumerable<string>? tags = null, CancellationToken cancellationToken = default);
    ValueTask RemoveAsync(string key, CancellationToken cancellationToken = default);
    ValueTask RemoveByTagAsync(string tag, CancellationToken cancellationToken = default);
}
