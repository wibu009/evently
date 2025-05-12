using Evently.Modules.Ticketing.Domain.Events;

namespace Evently.Modules.Ticketing.Domain.Tickets;

public interface ITicketRepository
{
    Task<Ticket?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Ticket>> GetForEventAsync(Event @event, CancellationToken cancellationToken = default);
    void Insert(Ticket ticket);
    void InsertRange(IEnumerable<Ticket> tickets);
}
