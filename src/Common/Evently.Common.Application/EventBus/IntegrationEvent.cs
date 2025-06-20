﻿namespace Evently.Common.Application.EventBus;

public abstract class IntegrationEvent : IIntegrationEvent
{
    protected IntegrationEvent(Guid id, DateTime occurredOnUtc) =>
        (Id, OccurredOnUtc) = (id, occurredOnUtc); 
    
    public Guid Id { get; init; }
    public DateTime OccurredOnUtc { get; init; }
}