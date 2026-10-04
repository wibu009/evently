namespace Evently.Modules.Ticketing.Infrastructure.Payments;

/// <summary>
/// Stripe configuration. Bound from the <c>Ticketing:Stripe</c> configuration section.
/// When <see cref="Enabled"/> is <c>true</c> and a secret key is configured, the module
/// charges through Stripe; otherwise the in-memory fake gateway is used so that the local
/// development stack works without external dependencies.
/// </summary>
public sealed class StripeOptions
{
    public bool Enabled { get; init; }

    /// <summary>The Stripe secret key (e.g. <c>sk_test_...</c>).</summary>
    public string SecretKey { get; init; } = string.Empty;

    /// <summary>
    /// The payment method used for server-side confirmed charges (e.g. the Stripe test token
    /// <c>pm_card_visa</c>). In production this is replaced by a payment method collected
    /// on the client with Stripe.js.
    /// </summary>
    public string DefaultPaymentMethod { get; init; } = string.Empty;
}
