using Evently.Common.Application.EventBus;

namespace Evently.Modules.Events.IntegrationEvents.Events;

public sealed class EventUpdatedIntegrationEvent(
    Guid id,
    DateTime occuredOnUtc,
    Guid eventId,
    string title,
    string description,
    string location)
    : IntegrationEvent(id, occuredOnUtc)
{
    public Guid EventId { get; init; } = eventId;

    public string Title { get; init; } = title;

    public string Description { get; init; } = description;

    public string Location { get; init; } = location;
}
