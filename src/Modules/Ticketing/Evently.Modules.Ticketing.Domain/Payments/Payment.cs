using Evently.Common.Domain;
using Evently.Modules.Ticketing.Domain.Orders;

namespace Evently.Modules.Ticketing.Domain.Payments;

public sealed class Payment : Entity
{
    private Payment() { }

    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }

    /// <summary>
    /// The gateway reference of the completed charge (e.g. the Stripe PaymentIntent id).
    /// It stays <c>null</c> while the payment is pending and is set once the charge succeeds.
    /// </summary>
    public string? TransactionReference { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; }
    public PaymentStatus Status { get; private set; }
    public decimal? AmountRefunded { get; private set; } = decimal.Zero;
    public string? FailureReason { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? PaidAtUtc { get; private set; }
    public DateTime? RefundedAtUtc { get; private set; }

    public bool IsFullyRefunded => AmountRefunded >= Amount;

    public static Payment Create(Order order, decimal amount, string currency)
    {
        var payment = new Payment
        {
            Id = Guid.CreateVersion7(),
            OrderId = order.Id,
            TransactionReference = null,
            Amount = amount,
            Currency = currency,
            Status = PaymentStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow
        };

        payment.RaiseDomainEvent(new PaymentCreatedDomainEvent(payment.Id));

        return payment;
    }

    /// <summary>
    /// Marks the payment as succeeded after the payment gateway accepted the charge.
    /// This completes the payment step of the order fulfillment saga.
    /// </summary>
    public Result Succeed(string transactionReference)
    {
        if (Status != PaymentStatus.Pending)
        {
            return Result.Failure(PaymentErrors.NotPending);
        }

        TransactionReference = transactionReference;
        Status = PaymentStatus.Succeeded;
        PaidAtUtc = DateTime.UtcNow;

        RaiseDomainEvent(new PaymentSucceededDomainEvent(Id, OrderId, transactionReference, Amount, Currency));

        return Result.Success();
    }

    /// <summary>
    /// Marks the payment as failed when the payment gateway rejected the charge.
    /// The order fulfillment saga compensates by canceling the order and releasing the inventory.
    /// </summary>
    public Result Fail(string reason)
    {
        if (Status != PaymentStatus.Pending)
        {
            return Result.Failure(PaymentErrors.NotPending);
        }

        Status = PaymentStatus.Failed;
        FailureReason = reason;

        RaiseDomainEvent(new PaymentFailedDomainEvent(Id, OrderId, reason));

        return Result.Success();
    }

    public Result Refund(decimal refundAmount)
    {
        if (Status != PaymentStatus.Succeeded)
        {
            return Result.Failure(PaymentErrors.NotSucceeded);
        }

        if (refundAmount <= decimal.Zero)
        {
            return Result.Failure(PaymentErrors.InvalidRefundAmount);
        }

        if (AmountRefunded == Amount)
        {
            return Result.Failure(PaymentErrors.AlreadyRefunded);
        }

        if (AmountRefunded + refundAmount > Amount)
        {
            return Result.Failure(PaymentErrors.NotEnoughFunds);
        }

        AmountRefunded += refundAmount;

        if (Amount == AmountRefunded)
        {
            RefundedAtUtc = DateTime.UtcNow;
            RaiseDomainEvent(new PaymentRefundedDomainEvent(Id, TransactionReference!, Currency, refundAmount));
        }
        else
        {
            RaiseDomainEvent(new PaymentPartiallyRefundedDomainEvent(Id, TransactionReference!, Currency, refundAmount));
        }

        return Result.Success();
    }
}
