using Evently.Common.Application.Messaging;
using Evently.Common.Domain;
using Evently.Modules.Ticketing.Application.Abstractions.Data;
using Evently.Modules.Ticketing.Domain.Orders;
using Evently.Modules.Ticketing.Domain.Payments;

namespace Evently.Modules.Ticketing.Application.Payments.RefundPayment;

/// <summary>
/// Refunds the given payment, either partially (an explicit amount) or fully (the remaining balance).
/// Once the payment is fully refunded, the related order transitions to the refunded state, which
/// triggers the compensation steps: issued tickets are archived and the inventory is released.
/// The refund of the actual gateway transaction is executed asynchronously through the outbox
/// (<c>PaymentRefundedDomainEventHandler</c> / <c>PaymentPartiallyRefundedDomainEventHandler</c>).
/// </summary>
internal sealed class RefundPaymentCommandHandler(
    IPaymentRepository paymentRepository,
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<RefundPaymentCommand>
{
    public async Task<Result> Handle(RefundPaymentCommand request, CancellationToken cancellationToken)
    {
        Payment? payment = await paymentRepository.GetAsync(request.PaymentId, cancellationToken);
        if (payment is null)
        {
            return Result.Failure(PaymentErrors.NotFound(request.PaymentId));
        }

        decimal refundAmount = request.Amount ?? payment.Amount - (payment.AmountRefunded ?? decimal.Zero);

        Result result = payment.Refund(refundAmount);
        if (result.IsFailure)
        {
            return Result.Failure(result.Error);
        }

        if (payment.IsFullyRefunded)
        {
            Order? order = await orderRepository.GetAsync(payment.OrderId, cancellationToken);

            if (order is not null)
            {
                _ = order.Refund();
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
