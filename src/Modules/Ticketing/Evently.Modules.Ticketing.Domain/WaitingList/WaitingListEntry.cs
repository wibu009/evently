using Evently.Common.Domain;

namespace Evently.Modules.Ticketing.Domain.WaitingList;

public sealed class WaitingListEntry : Entity
{
    private WaitingListEntry() { }

    public Guid Id { get; private set; }
    public Guid TicketTypeId { get; private set; }
    public Guid CustomerId { get; private set; }
    public WaitingListEntryStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? NotifiedAtUtc { get; private set; }

    public static Result<WaitingListEntry> Create(Guid ticketTypeId, Guid customerId)
    {
        if (ticketTypeId == Guid.Empty)
        {
            return Result.Failure<WaitingListEntry>(WaitingListErrors.NotFound(ticketTypeId));
        }

        var entry = new WaitingListEntry
        {
            Id = Guid.CreateVersion7(),
            TicketTypeId = ticketTypeId,
            CustomerId = customerId,
            Status = WaitingListEntryStatus.Waiting,
            CreatedAtUtc = DateTime.UtcNow
        };

        entry.RaiseDomainEvent(new CustomerJoinedWaitingListDomainEvent(entry.Id, ticketTypeId, customerId));

        return entry;
    }

    /// <summary>
    /// Marks the entry as notified: inventory became available and the customer was told
    /// about it in FIFO order.
    /// </summary>
    public Result MarkNotified()
    {
        if (Status != WaitingListEntryStatus.Waiting)
        {
            return Result.Failure(WaitingListErrors.AlreadyNotified);
        }

        Status = WaitingListEntryStatus.Notified;
        NotifiedAtUtc = DateTime.UtcNow;

        return Result.Success();
    }
}
