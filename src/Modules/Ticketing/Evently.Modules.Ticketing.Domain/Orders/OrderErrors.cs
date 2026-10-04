using Evently.Common.Domain;

namespace Evently.Modules.Ticketing.Domain.Orders;

public static class OrderErrors
{
    public static Error NotFound(Guid orderId) => Error.NotFound("Orders.NotFound", $"Order with id {orderId} not found");
    public static readonly Error TicketsAlreadyIssues = Error.Problem("Orders.TicketsAlreadyIssues", "Tickets for this order are already issued");
    public static readonly Error NotPending = Error.Problem("Orders.NotPending", "The order is not in pending status");
    public static readonly Error NotPaid = Error.Problem("Orders.NotPaid", "The order is not in paid status");
    public static readonly Error AlreadyCanceled = Error.Problem("Orders.AlreadyCanceled", "The order was already canceled");
    public static readonly Error AlreadyExpired = Error.Problem("Orders.AlreadyExpired", "The order was already expired");
    public static readonly Error AlreadyRefunded = Error.Problem("Orders.AlreadyRefunded", "The order was already refunded");
    public static readonly Error PaymentNotCompleted = Error.Problem("Orders.PaymentNotCompleted", "The payment for this order has not been completed yet");
}
