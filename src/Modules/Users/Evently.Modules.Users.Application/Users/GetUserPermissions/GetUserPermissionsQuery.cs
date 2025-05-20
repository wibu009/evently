using Evently.Common.Application.Authorization;
using Evently.Common.Application.Caching;

namespace Evently.Modules.Users.Application.Users.GetUserPermissions;

public sealed record GetUserPermissionsQuery(string IdentityId) : ICacheQuery<PermissionsResponse>
{
    public string CacheKey => $"user:{IdentityId}:permissions";
}
