using Evently.Common.Application.EventBus;
using Evently.Common.Application.Exceptions;
using Evently.Common.Domain;
using Evently.Modules.Events.IntegrationEvents.Events;
using Evently.Modules.Ticketing.Application.Events.RescheduleEvent;
using MediatR;

namespace Evently.Modules.Ticketing.Presentation.Events;

/// <summary>
/// Keeps the local event replica in sync when an event is rescheduled,
/// so that order cancellation rules and customer communication use the correct dates.
/// </summary>
internal sealed class EventRescheduledIntegrationEventHandler(ISender sender)
    : IntegrationEventHandler<EventRescheduledIntegrationEvent>
{
    public override async Task Handle(EventRescheduledIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        Result result = await sender.Send(
            new RescheduleEventCommand(
                integrationEvent.EventId,
                integrationEvent.StartAtUtc,
                integrationEvent.EndAtUtc),
            cancellationToken);

        if (result.IsFailure)
        {
            throw new EventlyException(nameof(RescheduleEventCommand), result.Error);
        }
    }
}
