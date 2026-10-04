using Evently.Common.Domain;

namespace Evently.Modules.Ticketing.Domain.WaitingList;

public static class WaitingListErrors
{
    public static Error NotFound(Guid ticketTypeId) =>
        Error.NotFound("WaitingList.NotFound", $"Waiting list entry for ticket type with id {ticketTypeId} not found");

    public static readonly Error NotSoldOut =
        Error.Problem("WaitingList.NotSoldOut", "Customers can only join the waiting list when the ticket type is sold out");

    public static readonly Error AlreadyJoined =
        Error.Conflict("WaitingList.AlreadyJoined", "The customer already joined the waiting list for this ticket type");

    public static readonly Error NotJoined =
        Error.NotFound("WaitingList.NotJoined", "The customer has not joined the waiting list for this ticket type");

    public static readonly Error AlreadyNotified =
        Error.Problem("WaitingList.AlreadyNotified", "The waiting list entry was already notified");
}
