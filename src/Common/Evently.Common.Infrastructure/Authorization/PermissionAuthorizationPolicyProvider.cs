using Evently.Common.Application.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Evently.Common.Infrastructure.Authorization;

internal sealed class PermissionAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options) : DefaultAuthorizationPolicyProvider(options)
{
    private readonly AuthorizationOptions _authorizationOptions = options.Value;

    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        AuthorizationPolicy? policy = await base.GetPolicyAsync(policyName);
        if (policy is not null)
        {
            return policy;
        }

        if (policyName == StepUpPolicies.SensitiveAction)
        {
            AuthorizationPolicy stepUpPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(new StepUpRequirement(StepUpPolicies.SensitiveActionLoa))
                .Build();

            _authorizationOptions.AddPolicy(policyName, stepUpPolicy);

            return stepUpPolicy;
        }

        AuthorizationPolicy permissionPolicy = new AuthorizationPolicyBuilder()
            .AddRequirements(new PermissionRequirement(policyName))
            .Build();

        _authorizationOptions.AddPolicy(policyName, permissionPolicy);
        
        return permissionPolicy;
    }
}