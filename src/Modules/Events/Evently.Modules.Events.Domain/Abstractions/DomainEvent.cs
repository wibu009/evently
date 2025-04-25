namespace Evently.Modules.Events.Domain.Abstractions;

public abstract class DomainEvent(Guid id, DateTime occurredAtUtc) : IDomainEvent
{
    protected DomainEvent() : this(Guid.NewGuid(), DateTime.UtcNow)
    {
    }

    public Guid Id { get; init; } = id;
    public DateTime OccurredAtUtc { get; init; } = occurredAtUtc;
}
