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

    internal async Task<List<UserSummaryRepresentation>> ListUsersAsync(int page, int pageSize,
        CancellationToken cancellationToken = default)
    {
        string resource = $"users?briefRepresentation=true&first={page * pageSize}&max={pageSize}";
        HttpResponseMessage response = await httpClient.GetAsync(resource, cancellationToken);

        response.EnsureSuccessStatusCode();

        List<UserSummaryRepresentation>? users = await response.Content.ReadFromJsonAsync<List<UserSummaryRepresentation>>(cancellationToken);

        return users ?? [];
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