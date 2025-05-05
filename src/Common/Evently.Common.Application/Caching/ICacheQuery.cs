using Evently.Common.Application.Messaging;

namespace Evently.Common.Application.Caching;

public interface ICacheQuery<TResponse> : IQuery<TResponse>, ICacheQuery;

public interface ICacheQuery
{
    string CacheKey { get; }
    
    string[]? Tags => null;
    TimeSpan? Expiration => null;
    TimeSpan? LocalCacheExpiration => null;
}
