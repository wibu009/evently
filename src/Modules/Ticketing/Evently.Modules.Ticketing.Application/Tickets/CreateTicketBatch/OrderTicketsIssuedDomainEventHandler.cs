using Evently.Common.Application.Exceptions;
using Evently.Common.Application.Messaging;
using Evently.Common.Domain;
using Evently.Modules.Ticketing.Application.Abstractions.Notifications;
using Evently.Modules.Ticketing.Application.Tickets.GetTicket;
using Evently.Modules.Ticketing.Application.Tickets.GetTicketsForOrder;
using Evently.Modules.Ticketing.Domain.Customers;
using Evently.Modules.Ticketing.Domain.Orders;
using MediatR;

namespace Evently.Modules.Ticketing.Application.Tickets.CreateTicketBatch;

/// <summary>
/// Delivers the issued tickets to the customer.
/// </summary>
internal sealed class OrderTicketsIssuedDomainEventHandler(
    ISender sender,
    ICustomerRepository customerRepository,
    INotificationService notificationService)
    : DomainEventHandler<OrderTicketsIssuedDomainEvent>
{
    public override async Task Handle(OrderTicketsIssuedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        Result<IReadOnlyList<TicketResponse>> result = await sender.Send(
            new GetTicketsForOrderQuery(domainEvent.OrderId),
            cancellationToken);

        if (result.IsFailure)
        {
            throw new EventlyException(nameof(GetTicketsForOrderQuery), result.Error);
        }

        if (result.Value.Count == 0)
        {
            return;
        }

        Customer? customer = await customerRepository.GetAsync(result.Value[0].CustomerId, cancellationToken);
        if (customer is null)
        {
            throw new EventlyException(
                nameof(customerRepository),
                CustomerErrors.NotFound(result.Value[0].CustomerId));
        }

        foreach (TicketResponse ticket in result.Value)
        {
            await notificationService.SendTicketIssuedAsync(
                new TicketNotificationRequest(
                    ticket.OrderId,
                    ticket.Id,
                    customer.Id,
                    customer.Email,
                    ticket.Code),
                cancellationToken);
        }
    }
}
