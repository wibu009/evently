using Evently.Common.Domain;

namespace Evently.Modules.Ticketing.Domain.Payments;

public sealed class PaymentRefundedDomainEvent(Guid paymentId, string transactionReference, string currency, decimal refundAmount) : DomainEvent
{
    public Guid PaymentId { get; init; } = paymentId;

    public string TransactionReference { get; init; } = transactionReference;

    public string Currency { get; init; } = currency;

    public decimal RefundAmount { get; init; } = refundAmount;
}
