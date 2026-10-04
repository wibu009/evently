namespace Evently.Modules.Attendance.Domain.Tickets;

public interface ITicketRepository
{
    Task<Ticket?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// High-performance lookup by the unique ticket code (the QR payload).
    /// Backed by a unique index, so gate scans stay O(1) under crowd load.
    /// </summary>
    Task<Ticket?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    void Insert(Ticket ticket);
}