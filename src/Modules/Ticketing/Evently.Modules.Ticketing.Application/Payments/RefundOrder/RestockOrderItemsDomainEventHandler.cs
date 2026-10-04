using Evently.Common.Application.Messaging;
using Evently.Modules.Ticketing.Application.Orders.RestockOrderItems;
using Evently.Modules.Ticketing.Domain.Orders;
using MediatR;

namespace Evently.Modules.Ticketing.Application.Payments.RefundOrder;

/// <summary>
/// Compensation step of the refund flow: releases the ticket inventory that was sold by the refunded order.
/// </summary>
internal sealed class RestockOrderItemsDomainEventHandler(ISender sender)
    : DomainEventHandler<OrderRefundedDomainEvent>
{
    public override async Task Handle(OrderRefundedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await sender.Send(new RestockOrderItemsCommand(domainEvent.OrderId), cancellationToken);
    }
}
