using Evently.Common.Domain;

namespace Evently.Modules.Attendance.Domain.Tickets;

public static class TicketErrors
{
    public static readonly Error NotFound = Error.Problem("Tickets.NotFound", "Ticket not found");
    public static readonly Error DuplicateCheckIn = Error.Problem("Tickets.DuplicateCheckIn", "Ticket is already checked in");
    public static readonly Error InvalidCheckIn = Error.Problem("Tickets.InvalidCheckIn", "Ticket check in is invalid");
}