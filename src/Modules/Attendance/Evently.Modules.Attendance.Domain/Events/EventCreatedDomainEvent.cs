using Evently.Common.Domain;

namespace Evently.Modules.Attendance.Domain.Events;

public sealed class EventCreatedDomainEvent(
    Guid eventId,
    string title,
    string description,
    string location,
    DateTime startAtUtc,
    DateTime? endAtUtc) : DomainEvent
{
    public Guid EventId { get; init; } = eventId;

    public string Title { get; init; } = title;

    public string Description { get; init; } = description;

    public string Location { get; init; } = location;

    public DateTime StartAtUtc { get; init; } = startAtUtc;

    public DateTime? EndAtUtc { get; init; } = endAtUtc;
}