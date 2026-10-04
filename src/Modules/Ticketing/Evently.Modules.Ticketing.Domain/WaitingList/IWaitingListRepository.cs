namespace Evently.Modules.Ticketing.Domain.WaitingList;

public interface IWaitingListRepository
{
    Task<WaitingListEntry?> GetAsync(Guid ticketTypeId, Guid customerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the customers waiting for the given ticket type, in FIFO order.
    /// </summary>
    Task<IReadOnlyList<WaitingListEntry>> GetWaitingForTicketTypeAsync(
        Guid ticketTypeId,
        int maxCount,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<WaitingListEntry>> GetForCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);

    void Insert(WaitingListEntry waitingListEntry);

    void Remove(WaitingListEntry waitingListEntry);
}
