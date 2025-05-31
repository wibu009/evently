using Evently.Common.Application.Exceptions;
using Evently.Common.Domain;
using Evently.Modules.Attendance.Application.Events.CreateEvent;
using Evently.Modules.Events.IntegrationEvents;
using MassTransit;
using MediatR;

namespace Evently.Modules.Attendance.Presentation.Events;

public sealed class EventPublishedIntegrationEventConsumer(ISender sender)
    : IConsumer<EventPublishedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<EventPublishedIntegrationEvent> context)
    {
        Result result = await sender.Send(
            new CreateEventCommand(
                context.Message.EventId,
                context.Message.Title,
                context.Message.Description,
                context.Message.Location,
                context.Message.StartAtUtc,
                context.Message.EndAtUtc),
            context.CancellationToken);

        if (result.IsFailure)
        {
            throw new EventlyException(nameof(CreateEventCommand), result.Error);
        }
    }
}
