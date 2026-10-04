using Evently.Common.Domain;

namespace Evently.Modules.Ticketing.Domain.WaitingList;

public sealed class CustomerJoinedWaitingListDomainEvent(Guid waitingListEntryId, Guid ticketTypeId, Guid customerId) : DomainEvent
{
    public Guid WaitingListEntryId { get; init; } = waitingListEntryId;

    public Guid TicketTypeId { get; init; } = ticketTypeId;

    public Guid CustomerId { get; init; } = customerId;
}
