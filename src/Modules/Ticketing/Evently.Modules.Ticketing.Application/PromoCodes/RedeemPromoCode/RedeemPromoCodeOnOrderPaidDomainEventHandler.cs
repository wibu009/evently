using Evently.Common.Application.Messaging;
using Evently.Modules.Ticketing.Domain.Orders;
using MediatR;

namespace Evently.Modules.Ticketing.Application.PromoCodes.RedeemPromoCode;

/// <summary>
/// Counts the promo code redemption only after the order is paid.
/// </summary>
internal sealed class RedeemPromoCodeOnOrderPaidDomainEventHandler(ISender sender)
    : DomainEventHandler<OrderPaidDomainEvent>
{
    public override async Task Handle(OrderPaidDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await sender.Send(new RedeemPromoCodeCommand(domainEvent.OrderId), cancellationToken);
    }
}
