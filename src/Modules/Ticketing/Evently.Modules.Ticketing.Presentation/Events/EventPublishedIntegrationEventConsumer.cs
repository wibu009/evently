using Evently.Common.Application.Exceptions;
using Evently.Common.Domain;
using Evently.Modules.Events.IntegrationEvents;
using Evently.Modules.Ticketing.Application.Events.CreateEvent;
using MassTransit;
using MediatR;

namespace Evently.Modules.Ticketing.Presentation.Events;

public sealed class EventPublishedIntegrationEventConsumer(ISender sender) : IConsumer<EventPublishedIntegrationEvent>
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
                context.Message.EndAtUtc,
                context.Message.TicketTypes
                    .Select(t => new TicketTypeRequest(
                        t.Id,
                        context.Message.EventId,
                        t.Name,
                        t.Price,
                        t.Currency,
                        t.Quantity))
                    .ToList()),
            context.CancellationToken);

        if (result.IsFailure)
        {
            throw new EventlyException(nameof(CreateEventCommand), result.Error);
        }
    }
}
