using System.Net.Http.Json;

namespace Evently.Modules.Users.Infrastructure.Identity;

internal sealed class KeyCloakClient(HttpClient httpClient)
{
    internal async Task<string> RegisterUserAsync(UserRepresentation user,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("users", user, cancellationToken);
        
        response.EnsureSuccessStatusCode();

        return ExtractIdentityIdFromLocationHeader(response);
    }

    private static string ExtractIdentityIdFromLocationHeader(HttpResponseMessage httpResponseMessage)
    {
        const string usersSegmentName = "users/";
        
        string? locationHeader = httpResponseMessage.Headers.Location?.PathAndQuery;
        if (locationHeader is null)
        {
            throw new InvalidOperationException("Location header is null");
        }

        int userSegmentValueIndex =
            locationHeader.IndexOf(usersSegmentName, StringComparison.InvariantCultureIgnoreCase);
        
        string identityId = locationHeader[(userSegmentValueIndex + usersSegmentName.Length)..];

        return identityId;
    }
}