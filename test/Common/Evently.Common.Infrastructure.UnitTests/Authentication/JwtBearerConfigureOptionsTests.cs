using System.Security.Claims;
using Evently.Common.Infrastructure.Authentication;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Evently.Common.Infrastructure.UnitTests.Authentication;

public sealed class JwtBearerConfigureOptionsTests
{
    private const string ClientId = "evently-public-client";

    private static ClaimsPrincipal Principal(ICollection<Claim>? claims = null) =>
        new(new ClaimsIdentity(claims ?? [], authenticationType: "oidc"));

    private static AuthenticationScheme BearerScheme =>
        new("Bearer", displayName: null, handlerType: typeof(JwtBearerHandler));

    /// <summary>Runs the options' OnTokenValidated handler like the middleware would.</summary>
    private static async Task<JwtBearerOptions> ConfigureAndValidate(
        Dictionary<string, string?> authenticationSettings,
        ClaimsPrincipal principal)
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(authenticationSettings)
            .Build();

        JwtBearerConfigureOptions configureOptions = new(configuration);
        JwtBearerOptions options = new()
        {
            Events = new JwtBearerEvents() // the middleware always defaults this before handlers run
        };
        configureOptions.Configure(options);

        TokenValidatedContext context = new(new DefaultHttpContext(), BearerScheme, options)
        {
            Principal = principal
        };

        await options.Events.OnTokenValidated(context);

        return options;
    }

    [Fact]
    public async Task Configure_WhenAuthorizedPartyIsConfigured_AcceptsTokensMintedForTheClient()
    {
        // Arrange
        ClaimsPrincipal principal = Principal([new Claim("azp", ClientId)]);
        Func<Task> act = async () => await ConfigureAndValidate(
            new Dictionary<string, string?> { ["Authentication:AuthorizedParty"] = ClientId },
            principal);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Configure_AcceptsTokensWhoseAzpCarriesMultipleClients()
    {
        // Arrange — some stacks emit azp as a space-separated multi-value claim.
        ClaimsPrincipal principal = Principal([new Claim("azp", $"other-client {ClientId}")]);
        Func<Task> act = async () => await ConfigureAndValidate(
            new Dictionary<string, string?> { ["Authentication:AuthorizedParty"] = ClientId },
            principal);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Configure_RejectsTokensMintedForADifferentClient()
    {
        // Arrange
        ClaimsPrincipal principal = Principal([new Claim("azp", "somewhere-else")]);
        Func<Task> act = async () => await ConfigureAndValidate(
            new Dictionary<string, string?> { ["Authentication:AuthorizedParty"] = ClientId },
            principal);

        // Assert
        (await act.Should().ThrowAsync<AuthenticationFailureException>())
            .WithMessage($"*not issued for the client '{ClientId}'*");
    }

    [Fact]
    public async Task Configure_RejectsTokensWithoutAnAzpClaim()
    {
        // Arrange
        ClaimsPrincipal principal = Principal([new Claim("aud", "account")]);
        Func<Task> act = async () => await ConfigureAndValidate(
            new Dictionary<string, string?> { ["Authentication:AuthorizedParty"] = ClientId },
            principal);

        // Assert
        (await act.Should().ThrowAsync<AuthenticationFailureException>())
            .WithMessage("*missing the authorized party (azp) claim*");
    }

    [Fact]
    public async Task Configure_WhenAuthorizedPartyIsNotConfigured_DoesNotEnforceAzp()
    {
        // Arrange — deployments that have not adopted the direct-azp check remain functional.
        ClaimsPrincipal principal = Principal([new Claim("aud", "account")]);
        Func<Task> act = async () => await ConfigureAndValidate(
            new Dictionary<string, string?>(), // no AuthorizedParty
            principal);

        // Assert
        await act.Should().NotThrowAsync();
    }
}
