using Evently.Common.Application.EventBus;
using Evently.Common.Application.Exceptions;
using Evently.Common.Application.Messaging;
using Evently.Common.Domain;
using Evently.Modules.Events.Application.Events.GetEvent;
using Evently.Modules.Events.Domain.Events;
using Evently.Modules.Events.IntegrationEvents.Events;
using MediatR;

namespace Evently.Modules.Events.Application.Events.UpdateEvent;

internal sealed class EventUpdatedDomainEventHandler(ISender sender, IEventBus eventBus)
    : DomainEventHandler<EventUpdatedDomainEvent>
{
    public override async Task Handle(EventUpdatedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        Result<EventResponse> result = await sender.Send(new GetEventQuery(domainEvent.EventId), cancellationToken);

        if (result.IsFailure)
        {
            throw new EventlyException(nameof(GetEventQuery), result.Error);
        }

        await eventBus.PublishAsync(
            new EventUpdatedIntegrationEvent(
                domainEvent.Id,
                domainEvent.OccurredOnUtc,
                result.Value.Id,
                result.Value.Title,
                result.Value.Description,
                result.Value.Location),
            cancellationToken);
    }
}
