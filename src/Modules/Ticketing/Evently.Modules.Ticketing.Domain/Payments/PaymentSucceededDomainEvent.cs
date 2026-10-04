using Evently.Common.Domain;
using Evently.Modules.Ticketing.Domain.Orders;

namespace Evently.Modules.Ticketing.Domain.Payments;

public sealed class PaymentSucceededDomainEvent(
    Guid paymentId,
    Guid orderId,
    string transactionReference,
    decimal amount,
    string currency) : DomainEvent
{
    public Guid PaymentId { get; init; } = paymentId;

    public Guid OrderId { get; init; } = orderId;

    public string TransactionReference { get; init; } = transactionReference;

    public decimal Amount { get; init; } = amount;

    public string Currency { get; init; } = currency;
}
