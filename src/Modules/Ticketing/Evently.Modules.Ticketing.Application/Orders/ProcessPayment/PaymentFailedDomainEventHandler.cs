using Evently.Common.Application.Messaging;
using Evently.Modules.Ticketing.Application.Orders.CancelOrder;
using Evently.Modules.Ticketing.Domain.Payments;
using MediatR;

namespace Evently.Modules.Ticketing.Application.Orders.ProcessPayment;

/// <summary>
/// The compensation of the order fulfillment saga when the payment gateway rejects the charge:
/// the order is canceled and the reserved ticket inventory is released.
/// </summary>
internal sealed class PaymentFailedDomainEventHandler(ISender sender)
    : DomainEventHandler<PaymentFailedDomainEvent>
{
    public override async Task Handle(PaymentFailedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await sender.Send(
            new CancelOrderCommand(domainEvent.OrderId, $"Payment failed: {domainEvent.Reason}"),
            cancellationToken);
    }
}
