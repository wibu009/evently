using Evently.Common.Application.Messaging;
using Evently.Common.Domain;
using Evently.Modules.Ticketing.Application.Abstractions.Data;
using Evently.Modules.Ticketing.Application.Abstractions.Payments;
using Evently.Modules.Ticketing.Domain.Events;
using Evently.Modules.Ticketing.Domain.Orders;
using Evently.Modules.Ticketing.Domain.Payments;
using Microsoft.Extensions.Logging;

namespace Evently.Modules.Ticketing.Application.Orders.ProcessPayment;

/// <summary>
/// The payment step of the order fulfillment saga, executed asynchronously through the outbox
/// (never inside the checkout transaction). Charges the payment gateway and transitions the
/// payment and the order to their succeeded/paid states.
/// <list type="bullet">
/// <item>On success: the payment is marked succeeded and the order is marked paid, which triggers ticket issuance.</item>
/// <item>On gateway failure: the payment is marked failed, which compensates by canceling the order
/// and releasing the reserved inventory.</item>
/// </list>
/// The command is idempotent: it becomes a no-op when the payment is no longer pending.
/// </summary>
internal sealed class ProcessPaymentCommandHandler(
    IPaymentRepository paymentRepository,
    IOrderRepository orderRepository,
    IPaymentService paymentService,
    IUnitOfWork unitOfWork,
    ILogger<ProcessPaymentCommandHandler> logger)
    : ICommandHandler<ProcessPaymentCommand>
{
    public async Task<Result> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        Payment? payment = await paymentRepository.GetAsync(request.PaymentId, cancellationToken);
        if (payment is null)
        {
            return Result.Failure(PaymentErrors.NotFound(request.PaymentId));
        }

        if (payment.Status != PaymentStatus.Pending)
        {
            return Result.Success();
        }

        Order? order = await orderRepository.GetAsync(payment.OrderId, cancellationToken);
        if (order is null)
        {
            return Result.Failure(OrderErrors.NotFound(payment.OrderId));
        }

        if (order.Status != OrderStatus.Pending)
        {
            return Result.Success();
        }

        ChargeResponse chargeResponse;
        try
        {
            chargeResponse = await paymentService.ChargeAsync(payment.Id, payment.Amount, payment.Currency);
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "The payment gateway rejected the charge of {Amount} {Currency} for order {OrderId}",
                payment.Amount,
                payment.Currency,
                order.Id);

            _ = payment.Fail("The payment gateway rejected the charge");

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }

        _ = payment.Succeed(chargeResponse.TransactionReference);

        Result result = order.MarkAsPaid();
        if (result.IsFailure)
        {
            return Result.Failure(result.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
