using Evently.Modules.Ticketing.Application.Abstractions.Payments;

namespace Evently.Modules.Ticketing.Infrastructure.Payments;

/// <summary>
/// The in-memory fake gateway used in local development and tests. Every charge succeeds
/// immediately with a generated transaction reference; refunds are accepted and discarded.
/// </summary>
internal sealed class FakePaymentService : IPaymentService
{
    public Task<ChargeResponse> ChargeAsync(Guid paymentId, decimal amount, string currency)
    {
        return Task.FromResult(new ChargeResponse($"fake_{paymentId}", amount, currency));
    }

    public Task RefundAsync(string transactionReference, string currency, decimal amount)
    {
        return Task.CompletedTask;
    }
}
