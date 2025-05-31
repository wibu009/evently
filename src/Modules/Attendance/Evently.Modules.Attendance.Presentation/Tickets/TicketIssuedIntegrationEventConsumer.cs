using Evently.Common.Application.Exceptions;
using Evently.Common.Domain;
using Evently.Modules.Attendance.Application.Tickets.CreateTicket;
using Evently.Modules.Ticketing.IntegrationEvents;
using MassTransit;
using MediatR;

namespace Evently.Modules.Attendance.Presentation.Tickets;

public sealed class TicketIssuedIntegrationEventConsumer(ISender sender) : IConsumer<TicketIssuedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<TicketIssuedIntegrationEvent> context)
    {
        Result result = await sender.Send(
            new CreateTicketCommand(
                context.Message.TicketId,
                context.Message.CustomerId,
                context.Message.EventId,
                context.Message.Code),
            context.CancellationToken);

        if (result.IsFailure)
        {
            throw new EventlyException(nameof(CreateTicketCommand), result.Error);
        }
    }
}
