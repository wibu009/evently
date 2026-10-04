using Evently.Common.Domain;

namespace Evently.Modules.Ticketing.Domain.Orders;

public sealed class OrderPaidDomainEvent(Guid orderId) : DomainEvent
{
    public Guid OrderId { get; init; } = orderId;
}
