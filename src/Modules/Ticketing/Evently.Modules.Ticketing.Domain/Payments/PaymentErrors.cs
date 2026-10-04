using Evently.Common.Domain;

namespace Evently.Modules.Ticketing.Domain.Payments;

public static class PaymentErrors
{
    public static Error NotFound(Guid paymentId) => Error.NotFound("Payments.NotFound", $"Payment with id {paymentId} not found");
    public static Error NotFoundForOrder(Guid orderId) => Error.NotFound("Payments.NotFoundForOrder", $"Payment for order with id {orderId} not found");
    public static readonly Error AlreadyRefunded = Error.Problem("Payments.AlreadyRefunded", "Payment is already refunded");
    public static readonly Error NotEnoughFunds = Error.Problem("Payments.NotEnoughFunds", "There is not enough funds to refund");
    public static readonly Error NotPending = Error.Problem("Payments.NotPending", "The payment is not in pending status");
    public static readonly Error NotSucceeded = Error.Problem("Payments.NotSucceeded", "The payment is not in succeeded status and cannot be refunded");
    public static readonly Error InvalidRefundAmount = Error.Problem("Payments.InvalidRefundAmount", "The refund amount must be greater than zero");
}
