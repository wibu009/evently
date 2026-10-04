namespace Evently.Modules.Ticketing.Application.Payments.GetPayments;

public sealed record GetPaymentsResponse(
    int Page,
    int PageSize,
    int TotalCount,
    IReadOnlyList<PaymentResponse> Payments);
