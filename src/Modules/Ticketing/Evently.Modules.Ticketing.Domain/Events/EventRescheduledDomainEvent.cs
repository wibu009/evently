using Evently.Common.Domain;

namespace Evently.Modules.Ticketing.Domain.Events;

public sealed class EventRescheduledDomainEvent(Guid eventId, DateTime startAtUtc, DateTime? endAtUtc) : DomainEvent
{
    public Guid EventId { get; init; } = eventId;
    public DateTime StartAtUtc { get; init; } = startAtUtc;
    public DateTime? EndAtUtc { get; init; } = endAtUtc;
}
