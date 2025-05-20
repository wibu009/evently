using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;

namespace Evently.Modules.Users.Infrastructure.Identity;

internal sealed class KeyCloakAuthDelegatingHandler(IOptions<KeyCloakOptions> options) : DelegatingHandler
{
    private readonly KeyCloakOptions _options = options.Value;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        AuthToken authorizationToken = await GetAuthorizationTokenAsync(cancellationToken);
        
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authorizationToken.AccessToken);
        
        HttpResponseMessage response = await base.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        
        return response;
    }

    private async Task<AuthToken> GetAuthorizationTokenAsync(CancellationToken cancellationToken)
    {
        var authRequestParameters = new KeyValuePair<string, string>[]
        {
            new("client_id", _options.ConfidentialClientId),
            new("client_secret", _options.ConfidentialClientSecret),
            new("scope", "openid"),
            new("grant_type", "client_credentials")
        };

        using var authRequestContent = new FormUrlEncodedContent(authRequestParameters);
        
        using var authRequest = new HttpRequestMessage(HttpMethod.Post, new Uri(_options.TokenUrl));
        authRequest.Content = authRequestContent;
        
        using HttpResponseMessage authorizationResponse = await base.SendAsync(authRequest, cancellationToken);
        authorizationResponse.EnsureSuccessStatusCode();
        
        return await authorizationResponse.Content.ReadFromJsonAsync<AuthToken>(cancellationToken);
    }
}