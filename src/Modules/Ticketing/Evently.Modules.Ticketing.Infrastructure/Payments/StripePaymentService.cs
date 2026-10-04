using Evently.Modules.Ticketing.Application.Abstractions.Payments;
using Stripe;
using StripeException = Stripe.StripeException;

namespace Evently.Modules.Ticketing.Infrastructure.Payments;

/// <summary>
/// Charges and refunds through Stripe.
/// <list type="bullet">
/// <item>Charges create a server-side confirmed PaymentIntent; its id becomes the payment's
/// transaction reference.</item>
/// <item>Every request carries an idempotency key derived from the payment id or the
/// transaction reference and the amount, so outbox retries can never double-charge or
/// double-refund.</item>
/// <item>The charge uses the configured <see cref="StripeOptions.DefaultPaymentMethod"/>; in
/// production, payment methods are collected on the client with Stripe.js and this flow
/// switches to confirming against the customer's saved payment method.</item>
/// </list>
/// </summary>
internal sealed class StripePaymentService(StripeClient stripeClient, StripeOptions options) : IPaymentService
{
    public async Task<ChargeResponse> ChargeAsync(Guid paymentId, decimal amount, string currency)
    {
        var requestOptions = new RequestOptions
        {
            // Stable per payment: if the charge reached Stripe but the response was lost,
            // a retry returns the same PaymentIntent instead of charging again.
            IdempotencyKey = $"charge:{paymentId}"
        };

        var createOptions = new PaymentIntentCreateOptions
        {
            Amount = CurrencyMinorUnits.ToMinorUnits(amount, currency),
            Currency = currency.ToLowerInvariant(),
            Confirm = true,
            PaymentMethod = options.DefaultPaymentMethod,
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
            {
                Enabled = true,
                AllowRedirects = "never"
            }
        };

        PaymentIntent paymentIntent = await new PaymentIntentService(stripeClient).CreateAsync(createOptions, requestOptions);

        if (paymentIntent.Status != "succeeded")
        {
            throw new StripeException(
                $"The payment intent {paymentIntent.Id} could not be confirmed server-side. Status: {paymentIntent.Status}");
        }

        return new ChargeResponse(paymentIntent.Id, amount, currency);
    }

    public async Task RefundAsync(string transactionReference, string currency, decimal amount)
    {
        long amountMinor = CurrencyMinorUnits.ToMinorUnits(amount, currency);

        var requestOptions = new RequestOptions
        {
            // Deduplicates retries of the same refund: same intent + same amount => same key.
            IdempotencyKey = $"refund:{transactionReference}:{amountMinor}"
        };

        var createOptions = new RefundCreateOptions
        {
            PaymentIntent = transactionReference,
            Amount = amountMinor
        };

        await new RefundService(stripeClient).CreateAsync(createOptions, requestOptions);
    }
}
