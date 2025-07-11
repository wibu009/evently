﻿namespace Evently.Common.Domain;

public abstract class Entity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    protected Entity()
    {
    }
    
    public IReadOnlyCollection<IDomainEvent> DomainEvents => [.. _domainEvents];
    
    public void ClearDomainEvents() => _domainEvents.Clear();
    
    protected void RaiseDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
}
