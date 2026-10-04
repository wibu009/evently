using Evently.Common.Domain;

namespace Evently.Modules.Ticketing.Domain.Tickets;

public static class TicketErrors
{
    public static Error NotFound(Guid ticketId) => Error.NotFound("Tickets.NotFound", $"Ticket with id {ticketId} not found");
    public static Error NotFound(string code) => Error.NotFound("Tickets.NotFound", $"Ticket with code {code} not found");
    public static readonly Error CannotTransferArchivedTicket =
        Error.Problem("Tickets.CannotTransferArchivedTicket", "Archived tickets cannot be transferred");
    public static Error NotOwnedByCustomer(Guid customerId) =>
        Error.Problem("Tickets.NotOwnedByCustomer", $"The ticket is not owned by the customer with id {customerId}");
    public static readonly Error CannotTransferToSelf =
        Error.Problem("Tickets.CannotTransferToSelf", "The ticket cannot be transferred to the same customer");
}