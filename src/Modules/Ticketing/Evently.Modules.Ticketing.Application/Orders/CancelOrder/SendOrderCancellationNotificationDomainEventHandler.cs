using Evently.Common.Application.Messaging;
using Evently.Modules.Ticketing.Application.Abstractions.Notifications;
using Evently.Modules.Ticketing.Domain.Customers;
using Evently.Modules.Ticketing.Domain.Orders;
using Microsoft.Extensions.Logging;

namespace Evently.Modules.Ticketing.Application.Orders.CancelOrder;

/// <summary>
/// Notifies the customer that the order has been canceled.
/// </summary>
internal sealed class SendOrderCancellationNotificationDomainEventHandler(
    IOrderRepository orderRepository,
    ICustomerRepository customerRepository,
    INotificationService notificationService,
    ILogger<SendOrderCancellationNotificationDomainEventHandler> logger)
    : DomainEventHandler<OrderCanceledDomainEvent>
{
    public override async Task Handle(OrderCanceledDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        Order? order = await orderRepository.GetAsync(domainEvent.OrderId, cancellationToken);
        if (order is null)
        {
            logger.LogWarning(
                "The order {OrderId} could not be found while sending the cancellation notification",
                domainEvent.OrderId);

            return;
        }

        Customer? customer = await customerRepository.GetAsync(order.CustomerId, cancellationToken);
        if (customer is null)
        {
            logger.LogWarning(
                "The customer {CustomerId} could not be found while sending the cancellation notification for order {OrderId}",
                order.CustomerId,
                order.Id);

            return;
        }

        await notificationService.SendOrderCancellationAsync(
            new OrderNotificationRequest(
                order.Id,
                customer.Id,
                customer.Email,
                order.TotalPrice,
                order.Currency,
                domainEvent.Reason),
            cancellationToken);
    }
}
