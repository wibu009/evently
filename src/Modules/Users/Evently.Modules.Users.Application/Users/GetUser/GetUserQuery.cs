using Evently.Common.Application.Caching;

namespace Evently.Modules.Users.Application.Users.GetUser;

public sealed record GetUserQuery(Guid UserId) : ICacheQuery<UserResponse>
{
    public string CacheKey => $"user:{UserId}";
}
