using Evently.Common.Application.Messaging;
using Evently.Common.Domain;
using Evently.Modules.Ticketing.Application.Abstractions.Data;
using Evently.Modules.Ticketing.Domain.Orders;
using Evently.Modules.Ticketing.Domain.Payments;
using Microsoft.Extensions.Logging;

namespace Evently.Modules.Ticketing.Application.Orders.ExpireOrder;

/// <summary>
/// The timeout of the order fulfillment saga, executed by the order expiration background job:
/// a pending order whose payment deadline has passed is expired, its pending payment is marked
/// as failed, and its reserved ticket inventory is released.
/// The command is idempotent: orders that already left the pending state are ignored.
/// </summary>
internal sealed class ExpireOrderCommandHandler(
    IOrderRepository orderRepository,
    IPaymentRepository paymentRepository,
    IUnitOfWork unitOfWork,
    ILogger<ExpireOrderCommandHandler> logger)
    : ICommandHandler<ExpireOrderCommand>
{
    public async Task<Result> Handle(ExpireOrderCommand request, CancellationToken cancellationToken)
    {
        Order? order = await orderRepository.GetAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            return Result.Failure(OrderErrors.NotFound(request.OrderId));
        }

        if (order.Status != OrderStatus.Pending)
        {
            return Result.Success();
        }

        Result result = order.Expire(OrderErrors.PaymentNotCompleted.Description);
        if (result.IsFailure)
        {
            return Result.Failure(result.Error);
        }

        Payment? payment = await paymentRepository.GetForOrderAsync(order.Id, cancellationToken);
        if (payment is not null && payment.Status == PaymentStatus.Pending)
        {
            _ = payment.Fail("The payment deadline for the order passed");

            logger.LogInformation(
                "The pending payment {PaymentId} of expired order {OrderId} has been marked as failed",
                payment.Id,
                order.Id);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
