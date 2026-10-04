namespace Evently.Modules.Ticketing.Application.Abstractions.Payments;

/// <summary>
/// The result of a successful charge. The transaction reference is the gateway's
/// identifier of the charge (e.g. the Stripe PaymentIntent id) and is required for refunds.
/// </summary>
public sealed record ChargeResponse(string TransactionReference, decimal Amount, string Currency);
