using System.Data.Common;
using Dapper;
using Evently.Common.Application.Data;
using Evently.Common.Application.Messaging;
using Evently.Common.Domain;
using Evently.Modules.Ticketing.Domain.Payments;

namespace Evently.Modules.Ticketing.Application.Payments.GetPayment;

internal sealed class GetPaymentQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetPaymentQuery, PaymentResponse>
{
    public async Task<Result<PaymentResponse>> Handle(GetPaymentQuery request, CancellationToken cancellationToken)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync(cancellationToken);

        const string sql =
            $"""
             SELECT
                 id AS {nameof(PaymentResponse.Id)},
                 order_id AS {nameof(PaymentResponse.OrderId)},
                 transaction_reference AS {nameof(PaymentResponse.TransactionReference)},
                 amount AS {nameof(PaymentResponse.Amount)},
                 currency AS {nameof(PaymentResponse.Currency)},
                 CASE status
                     WHEN 0 THEN 'Pending'
                     WHEN 1 THEN 'Succeeded'
                     WHEN 2 THEN 'Failed'
                 END AS {nameof(PaymentResponse.Status)},
                 amount_refunded AS {nameof(PaymentResponse.AmountRefunded)},
                 failure_reason AS {nameof(PaymentResponse.FailureReason)},
                 created_at_utc AS {nameof(PaymentResponse.CreatedAtUtc)},
                 paid_at_utc AS {nameof(PaymentResponse.PaidAtUtc)},
                 refunded_at_utc AS {nameof(PaymentResponse.RefundedAtUtc)}
             FROM ticketing.payments
             WHERE id = @PaymentId
             """;

        PaymentResponse? payment = await connection.QuerySingleOrDefaultAsync<PaymentResponse>(
            new CommandDefinition(sql, request, cancellationToken: cancellationToken));

        return payment ?? Result.Failure<PaymentResponse>(PaymentErrors.NotFound(request.PaymentId));
    }
}
