using Evently.Common.Domain;

namespace Evently.Modules.Ticketing.Domain.Events;

public static class TicketTypeErrors
{
    public static Error NotFound(Guid ticketTypeId) =>
        Error.NotFound("TicketTypes.NotFound", $"Ticket type with id {ticketTypeId} not found");
    public static Error NotEnoughQuantity(decimal availableQuantity) =>
        Error.Problem("TicketTypes.NotEnoughQuantity", $"Not enough quantity available. Available quantity: {availableQuantity}");
    public static readonly Error InvalidQuantity =
        Error.Problem("TicketTypes.InvalidQuantity", "The quantity must be greater than zero");
    public static Error InvalidRestockQuantity(decimal maxRestockableQuantity) =>
        Error.Problem("TicketTypes.InvalidRestockQuantity", $"The restock quantity exceeds the remaining capacity of {maxRestockableQuantity}");
}
