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

        logger.LogInformation("Processing cached request: {RequestName}", requestName);
        
        TResponse result = await cacheService.GetOrCreateAsync(
            key: request.CacheKey,
            factory: async ct => await next(ct),
            expiration: request.Expiration,
            localCacheExpiration: request.LocalCacheExpiration,
            tags: request.Tags,
            cancellationToken: cancellationToken
        );

        logger.LogInformation("Completed cached request: {RequestName}", requestName);

        return result;
    }
}
