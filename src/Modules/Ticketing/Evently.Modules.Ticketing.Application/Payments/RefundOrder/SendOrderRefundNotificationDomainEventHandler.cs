using Evently.Common.Application.Messaging;
using Evently.Modules.Ticketing.Application.Abstractions.Notifications;
using Evently.Modules.Ticketing.Domain.Customers;
using Evently.Modules.Ticketing.Domain.Orders;
using Microsoft.Extensions.Logging;

namespace Evently.Modules.Ticketing.Application.Payments.RefundOrder;

/// <summary>
/// Notifies the customer that the order has been refunded.
/// </summary>
internal sealed class SendOrderRefundNotificationDomainEventHandler(
    IOrderRepository orderRepository,
    ICustomerRepository customerRepository,
    INotificationService notificationService,
    ILogger<SendOrderRefundNotificationDomainEventHandler> logger)
    : DomainEventHandler<OrderRefundedDomainEvent>
{
    public override async Task Handle(OrderRefundedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        Order? order = await orderRepository.GetAsync(domainEvent.OrderId, cancellationToken);
        if (order is null)
        {
            logger.LogWarning(
                "The order {OrderId} could not be found while sending the refund notification",
                domainEvent.OrderId);

            return;
        }

        Customer? customer = await customerRepository.GetAsync(order.CustomerId, cancellationToken);
        if (customer is null)
        {
            logger.LogWarning(
                "The customer {CustomerId} could not be found while sending the refund notification for order {OrderId}",
                order.CustomerId,
                order.Id);

            return;
        }

        await notificationService.SendOrderRefundAsync(
            new OrderNotificationRequest(
                order.Id,
                customer.Id,
                customer.Email,
                order.TotalPrice,
                order.Currency,
                order.CancellationReason),
            cancellationToken);
    }
}
