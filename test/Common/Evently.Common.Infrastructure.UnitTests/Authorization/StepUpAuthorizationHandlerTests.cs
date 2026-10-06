using System.Security.Claims;
using Evently.Common.Application.Authorization;
using Evently.Common.Infrastructure.Authorization;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;

namespace Evently.Common.Infrastructure.UnitTests.Authorization;

public sealed class StepUpAuthorizationHandlerTests
{
    private const string ClientId = "evently-public-client";

    private static ClaimsPrincipal WithAcr(string? acr) =>
        acr is null
            ? new ClaimsPrincipal(new ClaimsIdentity([new Claim("azp", ClientId)], authenticationType: "oidc"))
            : new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim("azp", ClientId), new Claim("acr", acr)],
                authenticationType: "oidc"));

    private static IConfiguration EnforcedConfiguration(bool enforced) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Authentication:StepUp:Enforced"] = enforced.ToString()
            })
            .Build();

    [Theory]
    [InlineData("2", true)]
    [InlineData("3", true)]
    [InlineData("1", false)]
    [InlineData("0", false)]
    [InlineData("not-a-number", false)]
    [InlineData("", false)]
    public void HasRequiredLoa_MapsAcrLevelsOntoTheRequirement(string acr, bool expected)
    {
        // Arrange
        StepUpRequirement requirement = new(StepUpPolicies.SensitiveActionLoa);

        // Act
        bool result = StepUpAuthorizationHandler.HasRequiredLoa(WithAcr(acr), requirement);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void HasRequiredLoa_FailsWhenTheAcrClaimIsMissing()
    {
        ClaimsPrincipal principal = WithAcr(null);
        StepUpRequirement requirement = new(StepUpPolicies.SensitiveActionLoa);

        StepUpAuthorizationHandler.HasRequiredLoa(principal, requirement)
            .Should().BeFalse();
    }

    [Fact]
    public async Task Handler_SucceedsTheRequirement_WhenTheAcrLevelIsSufficient()
    {
        // Arrange
        StepUpAuthorizationHandler handler = new(EnforcedConfiguration(enforced: true));
        AuthorizationHandlerContext context = new(
            [new StepUpRequirement(StepUpPolicies.SensitiveActionLoa)],
            WithAcr("2"),
            resource: null);

        // Act
        await handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_DoesNotSucceedTheRequirement_WhenTheAcrLevelIsTooLow()
    {
        // Arrange
        StepUpAuthorizationHandler handler = new(EnforcedConfiguration(enforced: true));
        AuthorizationHandlerContext context = new(
            [new StepUpRequirement(StepUpPolicies.SensitiveActionLoa)],
            WithAcr("1"),
            resource: null);

        // Act
        await handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task Handler_WhenEnforcementIsDisabled_SucceedsForAnyToken()
    {
        // Arrangement — deployments (and test fixtures) whose IdP does not yet emit `acr`.
        StepUpAuthorizationHandler handler = new(EnforcedConfiguration(enforced: false));
        AuthorizationHandlerContext context = new(
            [new StepUpRequirement(StepUpPolicies.SensitiveActionLoa)],
            WithAcr(null),
            resource: null);

        // Act
        await handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }
}
