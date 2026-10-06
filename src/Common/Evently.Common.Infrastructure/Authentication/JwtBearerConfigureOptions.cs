using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Evently.Common.Infrastructure.Authentication;

internal sealed class JwtBearerConfigureOptions(IConfiguration configuration) : IConfigureNamedOptions<JwtBearerOptions>
{
    private const string ConfigurationSectionName = "Authentication";
    private const string AuthorizedPartyKey = "AuthorizedParty";
    private const string AzpClaimType = "azp";

    public void Configure(JwtBearerOptions options)
    {
        configuration.GetSection(ConfigurationSectionName).Bind(options);

        // RFC 9068 / OAuth 2.0: `azp` is the client the access token was minted for.
        // Requiring it to match the front-end's client id stops tokens issued to other
        // clients of the realm from being replayed against this API.
        string? authorizedParty = configuration.GetSection(ConfigurationSectionName)[AuthorizedPartyKey];
        if (string.IsNullOrWhiteSpace(authorizedParty) || options.Events is null)
        {
            return;
        }

        Func<TokenValidatedContext, Task>? originalOnTokenValidated = options.Events.OnTokenValidated;

        options.Events.OnTokenValidated = async context =>
        {
            if (originalOnTokenValidated is not null)
            {
                await originalOnTokenValidated(context);
            }

            EnforceAuthorizedParty(context.Principal!, authorizedParty);
        };
    }

    public void Configure(string? name, JwtBearerOptions options)
    {
        Configure(options);
    }

    private static void EnforceAuthorizedParty(ClaimsPrincipal principal, string authorizedParty)
    {
        Claim? azpClaim = principal.FindFirst(AzpClaimType);
        if (azpClaim is null)
        {
            throw new AuthenticationFailureException("The access token is missing the authorized party (azp) claim.");
        }

        string[] azpValues = azpClaim.Value.Split(
            ' ',
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (!azpValues.Contains(authorizedParty, StringComparer.Ordinal))
        {
            throw new AuthenticationFailureException($"The access token was not issued for the client '{authorizedParty}'.");
        }
    }
}
