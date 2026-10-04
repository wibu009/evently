namespace Evently.Modules.Ticketing.Application.Payments;

public sealed record PaymentResponse(
    Guid Id,
    Guid OrderId,
    string? TransactionReference,
    decimal Amount,
    string Currency,
    string Status,
    decimal AmountRefunded,
    string? FailureReason,
    DateTime CreatedAtUtc,
    DateTime? PaidAtUtc,
    DateTime? RefundedAtUtc);
