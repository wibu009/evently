using Evently.Common.Application.Messaging;
using Evently.Common.Domain;
using Evently.Modules.Ticketing.Application.Abstractions.Data;
using Evently.Modules.Ticketing.Domain.Events;
using Evently.Modules.Ticketing.Domain.Orders;
using Microsoft.Extensions.Logging;

namespace Evently.Modules.Ticketing.Application.Orders.RestockOrderItems;

/// <summary>
/// The compensation step of the order fulfillment saga: releases the ticket inventory that was
/// reserved by an order when the order is canceled, expires, or is refunded.
/// </summary>
internal sealed class RestockOrderItemsCommandHandler(
    IOrderRepository orderRepository,
    ITicketTypeRepository ticketTypeRepository,
    IUnitOfWork unitOfWork,
    ILogger<RestockOrderItemsCommandHandler> logger)
    : ICommandHandler<RestockOrderItemsCommand>
{
    public async Task<Result> Handle(RestockOrderItemsCommand request, CancellationToken cancellationToken)
    {
        Order? order = await orderRepository.GetAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            return Result.Failure(OrderErrors.NotFound(request.OrderId));
        }

        foreach (OrderItem orderItem in order.OrderItems)
        {
            TicketType? ticketType = await ticketTypeRepository.GetAsync(orderItem.TicketTypeId, cancellationToken);
            if (ticketType is null)
            {
                logger.LogWarning(
                    "The ticket type {TicketTypeId} of order {OrderId} no longer exists and cannot be restocked",
                    orderItem.TicketTypeId,
                    order.Id);

                continue;
            }

            Result result = ticketType.Restock(orderItem.Quantity);
            if (result.IsFailure)
            {
                logger.LogError(
                    "Failed to restock {Quantity} ticket(s) of type {TicketTypeId} for order {OrderId}: {Error}",
                    orderItem.Quantity,
                    orderItem.TicketTypeId,
                    order.Id,
                    result.Error.Description);

                return Result.Failure(result.Error);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
