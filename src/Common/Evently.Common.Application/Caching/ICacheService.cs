namespace Evently.Common.Application.Caching;

public interface ICacheService
{
    ValueTask<T> GetOrCreateAsync<T>(string key, Func<CancellationToken, ValueTask<T>> factory,
        TimeSpan? expiration, TimeSpan? localCacheExpiration, IEnumerable<string>? tags, CancellationToken cancellationToken = default);
    ValueTask RemoveAsync(string key, CancellationToken cancellationToken = default);
    ValueTask RemoveByTagAsync(string tag, CancellationToken cancellationToken = default);
}
