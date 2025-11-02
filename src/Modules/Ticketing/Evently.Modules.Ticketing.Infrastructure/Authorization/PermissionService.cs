using Evently.Common.Application.Authorization;
using Evently.Common.Application.Caching;
using Evently.Common.Domain;
using Evently.Modules.Users.IntegrationEvents.Users;
using MassTransit;

namespace Evently.Modules.Ticketing.Infrastructure.Authorization;

internal sealed class PermissionService(
    IRequestClient<GetUserPermissionsRequest> requestClient,
    ICacheService cacheService) : IPermissionService
{
    private static readonly Error NotFound = Error.NotFound(nameof(PermissionService), "The user was not found");
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan LocalCacheExpiration = TimeSpan.FromMinutes(5);
    
    public async Task<Result<PermissionsResponse>> GetUserPermissionsAsync(string identityId)
    {
        PermissionsResponse? permissionsResponse = await cacheService.GetAsync<PermissionsResponse>(CreateCacheKey(identityId));
        
        if (permissionsResponse is not null)
        {
            return permissionsResponse;
        }
        
        var request = new GetUserPermissionsRequest(identityId);
        
        Response<PermissionsResponse, Error> response = await requestClient.GetResponse<PermissionsResponse, Error>(request);
        
        if (response.Is(out Response<Error> errorResponse))
        {
            return Result.Failure<PermissionsResponse>(errorResponse.Message);
        }

        if (response.Is(out Response<PermissionsResponse> permissionResponse))
        {
            await cacheService.SetAsync(
                CreateCacheKey(identityId),
                permissionResponse.Message,
                DefaultExpiration,
                LocalCacheExpiration);

            return permissionResponse.Message;
        }
        
        return Result.Failure<PermissionsResponse>(NotFound);
    }
    
    private static string CreateCacheKey(string identityId) => $"user-permissions:{identityId}";
}
