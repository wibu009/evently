using Evently.Common.Application.Exceptions;
using Evently.Common.Domain;
using Evently.Modules.Events.IntegrationEvents;
using Evently.Modules.Ticketing.Application.TicketTypes.UpdateTicketTypePrice;
using MassTransit;
using MediatR;

namespace Evently.Modules.Ticketing.Presentation.TicketTypes;

public sealed class TicketTypePriceChangedIntegrationEventConsumer(ISender sender) : IConsumer<TicketTypePriceChangedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<TicketTypePriceChangedIntegrationEvent> context)
    {
        Result result = await sender.Send(
            new UpdateTicketTypePriceCommand(context.Message.TicketTypeId, context.Message.Price),
            context.CancellationToken);

        if (result.IsFailure)
        {
            throw new EventlyException(nameof(UpdateTicketTypePriceCommand), result.Error);
        }
    }
}
