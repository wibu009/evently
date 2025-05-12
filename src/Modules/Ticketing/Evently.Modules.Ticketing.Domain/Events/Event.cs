using Evently.Common.Domain;

namespace Evently.Modules.Ticketing.Domain.Events;

public sealed class Event : Entity
{
    private Event() { }
    
    public Guid Id { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public string Location { get; private set; }
    public DateTime StartAtUtc { get; private set; }
    public DateTime? EndAtUtc { get; private set; }
    public bool Canceled { get; private set; }
    
    public static Event Create(
        Guid id,
        string title,
        string description,
        string location,
        DateTime startAtUtc,
        DateTime? endAtUtc)
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

        return @event;
    }
    
    public void Reschedule(DateTime startAtUtc, DateTime? endAtUtc)
    {
        StartAtUtc = startAtUtc;
        EndAtUtc = endAtUtc;
        
        RaiseDomainEvent(new EventRescheduledDomainEvent(Id, startAtUtc, endAtUtc));
    }

    public void Cancel()
    {
        if (Canceled)
        {
            return;
        }
        
        Canceled = true;
        
        RaiseDomainEvent(new EventCanceledDomainEvent(Id));
    }
    
    public void PaymentsRefunded()
    {
        RaiseDomainEvent(new EventPaymentsRefundedDomainEvent(Id));
    }
    
    public void TicketsArchived()
    {
        RaiseDomainEvent(new EventTicketsArchivedDomainEvent(Id));
    }
}
