using Evently.Modules.Attendance.Domain.Tickets;
using Evently.Modules.Attendance.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Evently.Modules.Attendance.Infrastructure.Tickets;

internal sealed class TicketRepository(AttendanceDbContext context) : ITicketRepository
{
    public async Task<Ticket?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Tickets.SingleOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<Ticket?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await context.Tickets.SingleOrDefaultAsync(t => t.Code == code, cancellationToken);
    }

    public void Insert(Ticket ticket)
    {
        context.Tickets.Add(ticket);
    }
}