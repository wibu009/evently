using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

namespace Evently.Common.Infrastructure.Authorization;

/// <summary>
/// OAuth 2.0 step-up authorization. The `acr` claim carries the Level of
/// Assurance (LoA) the IdP gave the session (0 = primary factor only, 1 =
/// authenticated, 2 = second factor verified, e.g. TOTP / passkey).
/// </summary>
internal sealed class StepUpRequirement(int minimumLoa) : IAuthorizationRequirement
{
    public int MinimumLoa { get; } = minimumLoa;
}

/// <summary>
/// Enforces <see cref="StepUpRequirement"/>. When the IdP does not signal LoA
/// (no acr claim, or a lower one) the requirement fails and
/// <see cref="StepUpAuthorizationResultHandler"/> turns it into
/// 401 + `WWW-Authenticate: Bearer error="insufficient_user_authentication"`
/// so the client can re-authenticate with a higher `acr_values` request.
///
/// Enforcement can be disabled for deployments whose IdP does not yet emit
/// `acr` (tests use that); set `Authentication:StepUp:Enforced=false`.
/// </summary>
internal sealed class StepUpAuthorizationHandler(IConfiguration configuration) : AuthorizationHandler<StepUpRequirement>
{
    private readonly bool _isEnforced = configuration.GetValue("Authentication:StepUp:Enforced", defaultValue: true);

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, StepUpRequirement requirement)
    {
        if (!_isEnforced || HasRequiredLoa(context.User, requirement))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }

    public static bool HasRequiredLoa(ClaimsPrincipal user, StepUpRequirement requirement)
    {
        return int.TryParse(user.FindFirstValue("acr"), out int loa) && loa >= requirement.MinimumLoa;
    }
}

/// <summary>
/// Emits RFC 10005 challenge responses for failed step-up requirements:
/// 401 with `WWW-Authenticate: Bearer error="insufficient_user_authentication"`,
/// prompting web clients to re-run the authorize request with `acr_values=2`.
/// </summary>
internal sealed class StepUpAuthorizationResultHandler : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();

    public async Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        if (policy.Requirements.OfType<StepUpRequirement>().Any()
            && context.User.Identity?.IsAuthenticated == true
            && !authorizeResult.Succeeded)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.Headers[HeaderNames.WWWAuthenticate] =
                "Bearer error=\"insufficient_user_authentication\", error_description=\"A higher level of authentication is required for this operation.\"";

            return;
        }

        await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
    }}


