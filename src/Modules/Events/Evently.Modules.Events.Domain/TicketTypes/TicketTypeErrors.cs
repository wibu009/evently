using Evently.Common.Domain;

namespace Evently.Modules.Events.Domain.TicketTypes;

public static class TicketTypeErrors
{
    public static Error NotFound(Guid ticketTypeId)
        => Error.NotFound("TicketTypes.NotFound", $"Ticket type with id {ticketTypeId} not found");

    public static readonly Error InvalidColor = Error.Problem("TicketTypes.InvalidColor", "The ticket color must be a hex color like #7C3AED");

    public static readonly Error InvalidBackgroundImageUrl = Error.Problem("TicketTypes.InvalidBackgroundImageUrl", "The background image URL must be a non-empty value of at most 500 characters");
}
