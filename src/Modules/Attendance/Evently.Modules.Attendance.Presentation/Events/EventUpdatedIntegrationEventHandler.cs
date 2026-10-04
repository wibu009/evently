using Evently.Common.Application.EventBus;
using Evently.Common.Application.Exceptions;
using Evently.Common.Domain;
using Evently.Modules.Attendance.Application.Events.UpdateEvent;
using Evently.Modules.Events.IntegrationEvents.Events;
using MediatR;

namespace Evently.Modules.Attendance.Presentation.Events;

/// <summary>
/// Keeps the attendance event replica in sync when the details of an event change.
/// </summary>
internal sealed class EventUpdatedIntegrationEventHandler(ISender sender)
    : IntegrationEventHandler<EventUpdatedIntegrationEvent>
{
    public override async Task Handle(EventUpdatedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        Result result = await sender.Send(
            new UpdateEventCommand(
                integrationEvent.EventId,
                integrationEvent.Title,
                integrationEvent.Description,
                integrationEvent.Location),
            cancellationToken);

        if (result.IsFailure)
        {
            throw new EventlyException(nameof(UpdateEventCommand), result.Error);
        }
    }
}
