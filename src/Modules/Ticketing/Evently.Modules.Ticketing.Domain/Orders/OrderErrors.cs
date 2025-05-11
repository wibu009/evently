using Evently.Common.Domain;

namespace Evently.Modules.Ticketing.Domain.Orders;

public static class OrderErrors
{
    public static Error NotFound(Guid orderId) => Error.NotFound("Orders.NotFound", $"Order with id {orderId} not found");
    public static readonly Error TicketsAlreadyIssues = Error.Problem("Orders.TicketsAlreadyIssues", "Tickets for this order are already issued");
}