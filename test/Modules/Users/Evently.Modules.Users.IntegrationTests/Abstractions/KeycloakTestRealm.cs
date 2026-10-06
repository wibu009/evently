using System.Net.Http.Json;
using System.Text.Json;

namespace Evently.Modules.Users.IntegrationTests.Abstractions;

/// <summary>
/// Adjusts the imported test realm for headless testing. Production keeps direct
/// access grants disabled (OAuth 2.1), but the integration tests obtain tokens
/// through the password grant because there is no browser to run the
/// authorization-code flow — so the fixture re-enables it on the test Keycloak only.
/// </summary>
internal static class KeycloakTestRealm
{
    public static async Task AllowDirectGrantsAsync(string realmBaseUrl, CancellationToken cancellationToken = default)
    {
        using HttpClient client = new();

        using FormUrlEncodedContent tokenRequest = new(
        [
            new KeyValuePair<string, string>("client_id", "admin-cli"),
            new KeyValuePair<string, string>("grant_type", "password"),
            new KeyValuePair<string, string>("username", "admin"),
            new KeyValuePair<string, string>("password", "admin")
        ]);

        HttpResponseMessage tokenResponse = await client.PostAsync(
            $"{realmBaseUrl}realms/master/protocol/openid-connect/token",
            tokenRequest,
            cancellationToken);
        tokenResponse.EnsureSuccessStatusCode();

        Dictionary<string, JsonElement>? token =
            await tokenResponse.Content.ReadFromJsonAsync<Dictionary<string, JsonElement>>(cancellationToken);
        string adminToken = token!["access_token"].GetString()!;

        client.DefaultRequestHeaders.Authorization = new("Bearer", adminToken);

        HttpResponseMessage clientsResponse = await client.GetAsync(
            $"{realmBaseUrl}admin/realms/evently/clients?clientId=evently-public-client",
            cancellationToken);
        clientsResponse.EnsureSuccessStatusCode();

        List<Dictionary<string, JsonElement>>? representations =
            await clientsResponse.Content.ReadFromJsonAsync<List<Dictionary<string, JsonElement>>>(cancellationToken);
        string clientId = representations![0]["id"].GetString()!;

        HttpResponseMessage clientResponse = await client.GetAsync(
            $"{realmBaseUrl}admin/realms/evently/clients/{clientId}",
            cancellationToken);
        clientResponse.EnsureSuccessStatusCode();

        Dictionary<string, object?>? representation =
            await clientResponse.Content.ReadFromJsonAsync<Dictionary<string, object?>>(cancellationToken);
        representation!["directAccessGrantsEnabled"] = true;

        HttpResponseMessage updateResponse = await client.PutAsJsonAsync(
            $"{realmBaseUrl}admin/realms/evently/clients/{clientId}",
            representation,
            cancellationToken);
        updateResponse.EnsureSuccessStatusCode();
    }
}
