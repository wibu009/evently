using Evently.Common.Application.Exceptions;
using Evently.Common.Application.Messaging;
using Evently.Common.Domain;
using Evently.Modules.Ticketing.Application.Abstractions.Notifications;
using Evently.Modules.Ticketing.Application.Orders.GetOrder;
using Evently.Modules.Ticketing.Domain.Customers;
using Evently.Modules.Ticketing.Domain.Orders;
using MediatR;

namespace Evently.Modules.Ticketing.Application.Orders.ProcessPayment;

/// <summary>
/// Sends the order confirmation notification once the order has been paid.
/// </summary>
internal sealed class SendOrderConfirmationDomainEventHandler(
    ISender sender,
    ICustomerRepository customerRepository,
    INotificationService notificationService)
    : DomainEventHandler<OrderPaidDomainEvent>
{
    public override async Task Handle(OrderPaidDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        Result<OrderResponse> orderResult = await sender.Send(new GetOrderQuery(domainEvent.OrderId), cancellationToken);

        if (orderResult.IsFailure)
        {
            throw new EventlyException(nameof(GetOrderQuery), orderResult.Error);
        }

        Customer? customer = await customerRepository.GetAsync(orderResult.Value.CustomerId, cancellationToken);
        if (customer is null)
        {
            throw new EventlyException(
                nameof(customerRepository),
                CustomerErrors.NotFound(orderResult.Value.CustomerId));
        }

        await notificationService.SendOrderConfirmationAsync(
            new OrderNotificationRequest(
                orderResult.Value.Id,
                customer.Id,
                customer.Email,
                orderResult.Value.TotalPrice,
                orderResult.Value.Currency),
            cancellationToken);
    }
}
