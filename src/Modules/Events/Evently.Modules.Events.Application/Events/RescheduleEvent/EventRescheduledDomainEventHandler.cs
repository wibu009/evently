using Evently.Common.Application.Messaging;
using Evently.Modules.Events.Domain.Events;

namespace Evently.Modules.Events.Application.Events.RescheduleEvent;

internal sealed class EventRescheduledDomainEventHandler : DomainEventHandler<EventRescheduledDomainEvent>
{
    public override Task Handle(EventRescheduledDomainEvent notification, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
