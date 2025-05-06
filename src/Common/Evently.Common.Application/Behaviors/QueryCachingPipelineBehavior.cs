using Evently.Common.Application.Caching;
using Evently.Common.Domain;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Evently.Common.Application.Behaviors;

internal sealed class QueryCachingPipelineBehavior<TRequest, TResponse>(
    ICacheService cacheService,
    ILogger<QueryCachingPipelineBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICacheQuery
    where TResponse : Result
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        string requestName = request.GetType().Name;
        
        TResponse? cachedResult = await cacheService.GetAsync<TResponse>(request.CacheKey, cancellationToken);
        if (cachedResult is not null)
        {
            logger.LogInformation("Cache hit for {Query}", requestName);
            return cachedResult;
        }
        
        logger.LogInformation("Cache miss for {Query}", requestName);
        TResponse result = await next(cancellationToken);
        if (result.IsSuccess)
        {
            await cacheService.SetAsync(request.CacheKey, result, request.Expiration, request.LocalCacheExpiration, cancellationToken: cancellationToken);
        }

        return result;
    }
}
