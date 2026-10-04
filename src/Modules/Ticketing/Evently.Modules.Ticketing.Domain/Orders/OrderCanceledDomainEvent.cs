using Evently.Common.Domain;

namespace Evently.Modules.Ticketing.Domain.Orders;

public sealed class OrderCanceledDomainEvent(Guid orderId, string? reason) : DomainEvent
{
    public Guid OrderId { get; init; } = orderId;

    public string? Reason { get; init; } = reason;
}
