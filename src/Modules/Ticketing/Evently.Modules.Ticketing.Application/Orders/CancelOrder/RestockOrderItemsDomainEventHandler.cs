using Evently.Common.Application.Messaging;
using Evently.Modules.Ticketing.Application.Orders.RestockOrderItems;
using Evently.Modules.Ticketing.Domain.Orders;
using MediatR;

namespace Evently.Modules.Ticketing.Application.Orders.CancelOrder;

/// <summary>
/// Releases the ticket inventory that was reserved by the canceled order.
/// This is the compensation of the fulfillment saga for unpaid checkouts.
/// </summary>
internal sealed class RestockOrderItemsDomainEventHandler(ISender sender)
    : DomainEventHandler<OrderCanceledDomainEvent>
{
    public override async Task Handle(OrderCanceledDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await sender.Send(new RestockOrderItemsCommand(domainEvent.OrderId), cancellationToken);
    }
}
