using Evently.Common.Application.Exceptions;
using Evently.Common.Application.Messaging;
using Evently.Common.Domain;
using Evently.Modules.Ticketing.Application.Orders.RestockOrderItems;
using Evently.Modules.Ticketing.Application.Tickets.ArchiveOrderTickets;
using Evently.Modules.Ticketing.Domain.Orders;
using MediatR;

namespace Evently.Modules.Ticketing.Application.Payments.RefundOrder;

/// <summary>
/// Compensation step of the refund flow: archives the tickets that were issued for the refunded order.
/// </summary>
internal sealed class ArchiveOrderTicketsDomainEventHandler(ISender sender)
    : DomainEventHandler<OrderRefundedDomainEvent>
{
    public override async Task Handle(OrderRefundedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        Result result = await sender.Send(new ArchiveOrderTicketsCommand(domainEvent.OrderId), cancellationToken);

        if (result.IsFailure)
        {
            throw new EventlyException(nameof(ArchiveOrderTicketsCommand), result.Error);
        }
    }
}
