using Evently.Common.Domain;
using Evently.Modules.Events.Domain.Categories;

namespace Evently.Modules.Events.Domain.Events;

public sealed class Event : Entity
{
    private Event()
    {
    }
    
    public Guid Id { get; private init; }
    public Guid CategoryId { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public string Location { get; private set; }
    public DateTime StartAtUtc { get; private set; }
    public DateTime? EndAtUtc { get; private set; }
    public EventStatus Status { get; private set; }

    public static Result<Event> Create(
        Category category,
        string title,
        string description,
        string location,
        DateTime startAtUtc,
        DateTime? endAtUtc)
    {
        if (endAtUtc.HasValue && endAtUtc < startAtUtc)
        {
            return Result.Failure<Event>(EventErrors.EndDatePrecedesStartDate);
        }

        var @event = new Event
        {
            Id = Guid.CreateVersion7(),
            CategoryId = category.Id,
            Title = title,
            Description = description,
            Location = location,
            StartAtUtc = startAtUtc,
            EndAtUtc = endAtUtc,
            Status = EventStatus.Draft
        };

        @event.RaiseDomainEvent(new EventCreatedDomainEvent(@event.Id));

        return @event;
    }

    public Result Publish()
    {
        if (Status != EventStatus.Draft)
        {
            return Result.Failure(EventErrors.NotDraft);
        }
        
        Status = EventStatus.Published;
        
        RaiseDomainEvent(new EventPublishedDomainEvent(Id));
        
        return Result.Success();
    }
    
    public void Reschedule(DateTime startAtUtc, DateTime? endAtUtc)
    {
        if (StartAtUtc == startAtUtc && EndAtUtc == endAtUtc)
        {
            return;
        }
        
        StartAtUtc = startAtUtc;
        EndAtUtc = endAtUtc;
        
        RaiseDomainEvent(new EventRescheduledDomainEvent(Id, startAtUtc, endAtUtc));
    }

    public Result Cancel(DateTime utcNow)
    {
        if (Status == EventStatus.Canceled)
        {
            return Result.Failure(EventErrors.AlreadyCanceled);
        }

        if (StartAtUtc < utcNow)
        {
            return Result.Failure(EventErrors.AlreadyStarted);
        }
        
        Status = EventStatus.Canceled;
        
        RaiseDomainEvent(new EventCanceledDomainEvent(Id));
        
        return Result.Success();
    }
}
