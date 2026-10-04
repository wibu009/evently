using Evently.Common.Application.Exceptions;
using Evently.Common.Application.Messaging;
using Evently.Common.Domain;
using Evently.Modules.Ticketing.Application.Tickets.CreateTicketBatch;
using Evently.Modules.Ticketing.Domain.Orders;
using MediatR;

namespace Evently.Modules.Ticketing.Application.Orders.ProcessPayment;

/// <summary>
/// Issues the tickets only after the order has been paid.
/// Customers never receive tickets for unpaid or abandoned checkouts.
/// </summary>
internal sealed class CreateTicketsDomainEventHandler(ISender sender) : DomainEventHandler<OrderPaidDomainEvent>
{
    public override async Task Handle(OrderPaidDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        Result result = await sender.Send(new CreateTicketBatchCommand(domainEvent.OrderId), cancellationToken);

        if (result.IsFailure)
        {
            throw new EventlyException(nameof(CreateTicketBatchCommand), result.Error);
        }
    }
}
