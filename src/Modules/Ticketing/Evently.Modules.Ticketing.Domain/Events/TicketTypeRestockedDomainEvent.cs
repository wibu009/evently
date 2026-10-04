using Evently.Common.Domain;

namespace Evently.Modules.Ticketing.Domain.Events;

public sealed class TicketTypeRestockedDomainEvent(Guid ticketTypeId, decimal quantity) : DomainEvent
{
    public Guid TicketTypeId { get; init; } = ticketTypeId;

    public decimal Quantity { get; init; } = quantity;
}
