using Evently.Common.Domain;

namespace Evently.Modules.Attendance.Domain.Events;

public sealed class Event : Entity
{
    private Event() { }
    
    public Guid Id { get; private init; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public string Location { get; private set; }
    public DateTime StartAtUtc { get; private set; }
    public DateTime? EndAtUtc { get; private set; }

    public static Event Create(Guid id, string title, string description, string location,
        DateTime startAtUtc, DateTime? endAtUtc)
    {
        var @event = new Event
        {
            Id = id,
            Title = title,
            Description = description,
            Location = location,
            StartAtUtc = startAtUtc,
            EndAtUtc = endAtUtc
        };
        
        @event.RaiseDomainEvent(new EventCreatedDomainEvent(
            @event.Id,
            @event.Title,
            @event.Description,
            @event.Location,
            @event.StartAtUtc,
            @event.EndAtUtc));
        
        return @event;
    }
}
