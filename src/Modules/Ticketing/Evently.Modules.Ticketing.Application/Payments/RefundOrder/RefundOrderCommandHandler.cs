using Evently.Common.Application.Messaging;
using Evently.Common.Domain;
using Evently.Modules.Ticketing.Application.Abstractions.Data;
using Evently.Modules.Ticketing.Domain.Orders;
using Evently.Modules.Ticketing.Domain.Payments;

namespace Evently.Modules.Ticketing.Application.Payments.RefundOrder;

/// <summary>
/// Refunds the remaining balance of the payment that belongs to the given order.
/// Once the payment is fully refunded, the order transitions to the refunded state, which
/// triggers the compensation steps: the issued tickets are archived and the inventory is released.
/// The refund of the actual gateway transaction is executed asynchronously through the outbox
/// (<c>PaymentRefundedDomainEventHandler</c>).
/// </summary>
internal sealed class RefundOrderCommandHandler(
    IOrderRepository orderRepository,
    IPaymentRepository paymentRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<RefundOrderCommand>
{
    public async Task<Result> Handle(RefundOrderCommand request, CancellationToken cancellationToken)
    {
        Order? order = await orderRepository.GetAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            return Result.Failure(OrderErrors.NotFound(request.OrderId));
        }

        Payment? payment = await paymentRepository.GetForOrderAsync(order.Id, cancellationToken);
        if (payment is null)
        {
            return Result.Failure(PaymentErrors.NotFoundForOrder(order.Id));
        }

        decimal refundAmount = payment.Amount - (payment.AmountRefunded ?? decimal.Zero);

        Result refundResult = payment.Refund(refundAmount);
        if (refundResult.IsFailure)
        {
            return Result.Failure(refundResult.Error);
        }

        if (payment.IsFullyRefunded)
        {
            Result orderResult = order.Refund();
            if (orderResult.IsFailure)
            {
                return Result.Failure(orderResult.Error);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
