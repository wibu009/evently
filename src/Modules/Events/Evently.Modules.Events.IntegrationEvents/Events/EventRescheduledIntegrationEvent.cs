using Evently.Common.Application.EventBus;

namespace Evently.Modules.Events.IntegrationEvents.Events;

public sealed class EventRescheduledIntegrationEvent(
    Guid id,
    DateTime occurredOnUtc,
    Guid eventId,
    DateTime startAtUtc,
    DateTime? endAtUtc)
    : IntegrationEvent(id, occurredOnUtc)
{
    public Guid EventId { get; init; } = eventId;

    public DateTime StartAtUtc { get; init; } = startAtUtc;

    public DateTime? EndAtUtc { get; init; } = endAtUtc;
}
