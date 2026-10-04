using Evently.Modules.Ticketing.Domain.WaitingList;
using Evently.Modules.Ticketing.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Evently.Modules.Ticketing.Infrastructure.WaitingList;

internal sealed class WaitingListRepository(TicketingDbContext context) : IWaitingListRepository
{
    public async Task<WaitingListEntry?> GetAsync(Guid ticketTypeId, Guid customerId, CancellationToken cancellationToken = default)
    {
        return await context.WaitingListEntries.SingleOrDefaultAsync(
            e => e.TicketTypeId == ticketTypeId && e.CustomerId == customerId,
            cancellationToken);
    }

    public async Task<IReadOnlyList<WaitingListEntry>> GetWaitingForTicketTypeAsync(
        Guid ticketTypeId,
        int maxCount,
        CancellationToken cancellationToken = default)
    {
        return await context.WaitingListEntries
            .Where(e => e.TicketTypeId == ticketTypeId && e.Status == WaitingListEntryStatus.Waiting)
            .OrderBy(e => e.CreatedAtUtc)
            .Take(maxCount)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<WaitingListEntry>> GetForCustomerAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await context.WaitingListEntries
            .Where(e => e.CustomerId == customerId)
            .OrderByDescending(e => e.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public void Insert(WaitingListEntry waitingListEntry)
    {
        context.WaitingListEntries.Add(waitingListEntry);
    }

    public void Remove(WaitingListEntry waitingListEntry)
    {
        context.WaitingListEntries.Remove(waitingListEntry);
    }
}
