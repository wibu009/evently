using Evently.Common.Application.EventBus;

namespace Evently.Modules.Events.IntegrationEvents;

public sealed class EventPublishedIntegrationEvent(
    Guid id,
    DateTime occurredOnUtc,
    Guid eventId,
    string title,
    string description,
    string location,
    DateTime startAtUtc,
    DateTime? endAtUtc,
    List<TicketTypeModel> ticketTypes)
    : IntegrationEvent(id, occurredOnUtc)
{
    public Guid EventId { get; init; } = eventId;
    public string Title { get; init; } = title;
    public string Description { get; init; } = description;
    public string Location { get; init; } = location;
    public DateTime StartAtUtc { get; init; } = startAtUtc;
    public DateTime? EndAtUtc { get; init; } = endAtUtc;
    public List<TicketTypeModel> TicketTypes { get; init; } = ticketTypes;
}
