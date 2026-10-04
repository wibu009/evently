using Evently.Common.Application.Messaging;
using Evently.Modules.Ticketing.Application.Orders.RestockOrderItems;
using Evently.Modules.Ticketing.Domain.Orders;
using MediatR;

namespace Evently.Modules.Ticketing.Application.Orders.ExpireOrder;

/// <summary>
/// Releases the ticket inventory that was reserved by the expired order.
/// This is the compensation of the fulfillment saga timeout.
/// </summary>
internal sealed class RestockOrderItemsDomainEventHandler(ISender sender)
    : DomainEventHandler<OrderExpiredDomainEvent>
{
    public override async Task Handle(OrderExpiredDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await sender.Send(new RestockOrderItemsCommand(domainEvent.OrderId), cancellationToken);
    }
}
