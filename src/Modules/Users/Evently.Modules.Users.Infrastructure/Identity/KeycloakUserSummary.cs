namespace Evently.Modules.Users.Infrastructure.Identity;

/// <summary>
/// Subset of a Keycloak UserRepresentation returned by GET /admin/realms/{realm}/users
/// (id + profile fields are all the Users module needs for reconciliation).
/// </summary>
internal sealed record UserSummaryRepresentation(
    string Id,
    string Username,
    string Email,
    string FirstName,
    string LastName,
    bool Enabled);

internal sealed record UserSummaryRepresentationCollection(List<UserSummaryRepresentation> Users);
