using Evently.Common.Domain;

namespace Evently.Modules.Ticketing.Domain.Payments;

public sealed class PaymentFailedDomainEvent(Guid paymentId, Guid orderId, string reason) : DomainEvent
{
    public Guid PaymentId { get; init; } = paymentId;

    public Guid OrderId { get; init; } = orderId;

    public string Reason { get; init; } = reason;
}
