using Evently.Common.Domain;

namespace Evently.Modules.Ticketing.Domain.Orders;

public sealed class OrderExpiredDomainEvent(Guid orderId) : DomainEvent
{
    public Guid OrderId { get; init; } = orderId;
}
