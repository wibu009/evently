using Evently.Common.Application.Messaging;
using Evently.Modules.Ticketing.Application.Abstractions.Notifications;
using Evently.Modules.Ticketing.Domain.Customers;
using Evently.Modules.Ticketing.Domain.Orders;
using Microsoft.Extensions.Logging;

namespace Evently.Modules.Ticketing.Application.Orders.ExpireOrder;

/// <summary>
/// Notifies the customer that the order expired because the payment deadline passed.
/// </summary>
internal sealed class SendOrderExpirationNotificationDomainEventHandler(
    IOrderRepository orderRepository,
    ICustomerRepository customerRepository,
    INotificationService notificationService,
    ILogger<SendOrderExpirationNotificationDomainEventHandler> logger)
    : DomainEventHandler<OrderExpiredDomainEvent>
{
    public override async Task Handle(OrderExpiredDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        Order? order = await orderRepository.GetAsync(domainEvent.OrderId, cancellationToken);
        if (order is null)
        {
            logger.LogWarning(
                "The order {OrderId} could not be found while sending the expiration notification",
                domainEvent.OrderId);

            return;
        }

        Customer? customer = await customerRepository.GetAsync(order.CustomerId, cancellationToken);
        if (customer is null)
        {
            logger.LogWarning(
                "The customer {CustomerId} could not be found while sending the expiration notification for order {OrderId}",
                order.CustomerId,
                order.Id);

            return;
        }

        await notificationService.SendOrderExpirationAsync(
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
