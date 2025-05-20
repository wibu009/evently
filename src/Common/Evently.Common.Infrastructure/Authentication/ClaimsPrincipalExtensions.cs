using System.Security.Claims;
using Evently.Common.Application.Exceptions;

namespace Evently.Common.Infrastructure.Authentication;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal? principal)
    {
        string? userId = principal?.FindFirstValue(CustomClaims.Sub);
        return Guid.TryParse(userId, out Guid parsedUserId)
            ? parsedUserId
            : throw new EventlyException("User identifier is unavailable");
    }
    
    public static string GetIdentityId(this ClaimsPrincipal? principal) =>
        principal?.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new EventlyException("User identifier is unavailable");

    public static HashSet<string> GetPermissions(this ClaimsPrincipal? principal)
    {
        IEnumerable<Claim> permissionClaims = principal?.FindAll(CustomClaims.Permission) ?? throw new EventlyException("User permissions are unavailable");
        return permissionClaims.Select(c => c.Value).ToHashSet();
    }
}
