namespace Evently.Common.Application.Authorization;

/// <summary>
/// Named authorization policies resolved by the API's policy provider
/// (<c>PermissionAuthorizationPolicyProvider</c>) into step-up
/// (second-factor) requirements at runtime. Sensitive endpoints require a
/// stronger Level of Assurance than ordinary sessions: failing them returns
/// 401 with a
/// `WWW-Authenticate: Bearer error="insufficient_user_authentication"`
/// challenge so clients re-authenticate with `acr_values`.
/// </summary>
public static class StepUpPolicies
{
    /// <summary>Policy marking an endpoint as a sensitive action refund/transfer.</summary>
    public const string SensitiveAction = "step-up:loa-2";

    /// <summary>Level of Assurance sensitive actions must prove (second factor).</summary>
    public const int SensitiveActionLoa = 2;
}
