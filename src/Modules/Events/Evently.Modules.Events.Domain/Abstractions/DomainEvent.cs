namespace Evently.Modules.Events.Domain.Abstractions;

public abstract class DomainEvent(Guid id, DateTime occurredAtUtc) : IDomainEvent
{
    protected DomainEvent() : this(Guid.CreateVersion7(), DateTime.UtcNow)
    {
    }

    public Guid Id { get; init; } = id;
    public DateTime OccurredAtUtc { get; init; } = occurredAtUtc;
}
