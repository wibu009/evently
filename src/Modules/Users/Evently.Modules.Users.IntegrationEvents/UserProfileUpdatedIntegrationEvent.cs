using Evently.Common.Application.EventBus;

namespace Evently.Modules.Users.IntegrationEvents;

public sealed class UserProfileUpdatedIntegrationEvent (
    Guid id,
    DateTime occurredOnUtc,
    Guid userId,
    string firstName,
    string lastName)
    : IntegrationEvent(id, occurredOnUtc)
{
    public Guid UserId { get; init; } = userId;

    public string FirstName { get; init; } = firstName;

    public string LastName { get; init; } = lastName;
}
