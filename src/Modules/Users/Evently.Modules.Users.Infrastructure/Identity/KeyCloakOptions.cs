namespace Evently.Modules.Users.Infrastructure.Identity;

internal sealed class KeyCloakOptions
{
    public string AdminUrl { get; init; }
    public string TokenUrl { get; init; }
    public string ConfidentialClientId { get; init; }
    public string ConfidentialClientSecret { get; init; }
    public string PublicClientId { get; init; }
}