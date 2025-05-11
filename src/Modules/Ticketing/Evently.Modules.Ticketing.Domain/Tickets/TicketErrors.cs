using Evently.Common.Domain;

namespace Evently.Modules.Ticketing.Domain.Tickets;

public static class TicketErrors
{
    public static Error NotFound(Guid ticketId) => Error.NotFound("Tickets.NotFound", $"Ticket with id {ticketId} not found");
    public static Error NotFound(string code) => Error.NotFound("Tickets.NotFound", $"Ticket with code {code} not found");
}