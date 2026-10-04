using Evently.Common.Application.Messaging;
using Evently.Common.Domain;
using Evently.Modules.Ticketing.Application.Abstractions.Data;
using Evently.Modules.Ticketing.Application.Payments.RefundOrder;
using Evently.Modules.Ticketing.Domain.Orders;
using MediatR;

namespace Evently.Modules.Ticketing.Application.Orders.CancelOrder;

/// <summary>
/// Cancels an order:
/// <list type="bullet">
/// <item>Pending orders are canceled outright and their reserved inventory is released.</item>
/// <item>Paid orders enter the refund flow — the payment is refunded, the issued tickets are
/// archived and the inventory is released.</item>
/// </list>
/// Canceled, expired, and refunded orders cannot be canceled again.
/// </summary>
internal sealed class CancelOrderCommandHandler(
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork,
    ISender sender)
    : ICommandHandler<CancelOrderCommand>
{
    public async Task<Result> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        Order? order = await orderRepository.GetAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            return Result.Failure(OrderErrors.NotFound(request.OrderId));
        }

        switch (order.Status)
        {
            case OrderStatus.Pending:
            {
                Result result = order.Cancel(request.Reason);

                if (result.IsFailure)
                {
                    return Result.Failure(result.Error);
                }

                await unitOfWork.SaveChangesAsync(cancellationToken);

                return Result.Success();
            }

            case OrderStatus.Paid:
                return await sender.Send(new RefundOrderCommand(order.Id, request.Reason), cancellationToken);

            case OrderStatus.Canceled:
                return Result.Failure(OrderErrors.AlreadyCanceled);

            case OrderStatus.Expired:
                return Result.Failure(OrderErrors.AlreadyExpired);

            case OrderStatus.Refunded:
            default:
                return Result.Failure(OrderErrors.AlreadyRefunded);
        }
    }
}
