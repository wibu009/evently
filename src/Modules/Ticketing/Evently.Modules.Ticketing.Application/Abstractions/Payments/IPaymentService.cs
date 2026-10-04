namespace Evently.Modules.Ticketing.Application.Abstractions.Payments;

public interface IPaymentService
{
    /// <summary>
    /// Charges the given amount. The <paramref name="paymentId"/> is used to derive a stable
    /// Stripe idempotency key, so outbox retries can never double-charge a payment.
    /// </summary>
    Task<ChargeResponse> ChargeAsync(Guid paymentId, decimal amount, string currency);

    /// <summary>
    /// Refunds the given amount of a previous charge. Idempotent per transaction reference and amount.
    /// </summary>
    Task RefundAsync(string transactionReference, string currency, decimal amount);
}
