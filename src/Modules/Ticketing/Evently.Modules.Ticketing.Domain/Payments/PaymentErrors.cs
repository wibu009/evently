using Evently.Common.Domain;

namespace Evently.Modules.Ticketing.Domain.Payments;

public static class PaymentErrors
{
    public static Error NotFound(Guid paymentId) => Error.NotFound("Payments.NotFound", $"Payment with id {paymentId} not found");
    public static readonly Error AlreadyRefunded = Error.Problem("Payments.AlreadyRefunded", "Payment is already refunded");
    public static readonly Error NotEnoughFunds = Error.Problem("Payments.NotEnoughFunds", "There is not enough funds to refund");
}