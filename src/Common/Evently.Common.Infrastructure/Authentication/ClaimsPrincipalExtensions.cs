using System.Security.Claims;
using Evently.Common.Application.Exceptions;

namespace Evently.Common.Infrastructure.Authentication;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal? principal)
    {
        if (principal is null)
        {
            throw new ArgumentNullException(nameof(principal), "ClaimsPrincipal is null");
        }

        string? userId = principal.FindFirstValue(CustomClaims.Sub);
        if (userId is null)
        {
            throw new InvalidOperationException("User ID claim is missing");
        }

        return Guid.TryParse(userId, out Guid parsedUserId)
            ? parsedUserId
            : throw new FormatException("User ID claim is not a valid GUID");
    }
    
    public static string GetIdentityId(this ClaimsPrincipal? principal)
    {
        if (principal is null)
        {
            throw new ArgumentNullException(nameof(principal), "ClaimsPrincipal is null");
        }

        string? identityId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        return identityId ?? throw new InvalidOperationException("Identity ID claim is missing");
    }

    public static HashSet<string> GetPermissions(this ClaimsPrincipal? principal)
    {
        if (principal is null)
        {
            throw new ArgumentNullException(nameof(principal), "ClaimsPrincipal is null");
        }

        IEnumerable<Claim>? permissionClaims = principal.FindAll(CustomClaims.Permission);
        return permissionClaims != null
            ? permissionClaims.Select(c => c.Value).ToHashSet()
            : throw new InvalidOperationException("User permission claims are missing");
    }
}
