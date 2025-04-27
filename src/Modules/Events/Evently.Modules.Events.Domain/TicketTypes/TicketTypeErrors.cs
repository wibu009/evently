using Evently.Modules.Events.Domain.Abstractions;

namespace Evently.Modules.Events.Domain.TicketTypes;

public static class TicketTypeErrors
{
    public static Error NotFound(Guid ticketTypeId)
        => Error.NotFound("TicketTypes.NotFound", $"Ticket type with id {ticketTypeId} not found");
}
