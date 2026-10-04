using System.Data.Common;
using Dapper;
using Evently.Common.Application.Data;
using Evently.Common.Application.Messaging;
using Evently.Common.Domain;

namespace Evently.Modules.Ticketing.Application.Payments.GetPayments;

/// <summary>
/// Lists the payments of a customer, most recent first. The customer id is resolved from the
/// authenticated actor on the presentation side.
/// </summary>
internal sealed class GetPaymentsQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetPaymentsQuery, GetPaymentsResponse>
{
    public async Task<Result<GetPaymentsResponse>> Handle(GetPaymentsQuery request, CancellationToken cancellationToken)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync(cancellationToken);

        int page = request.Page < 1 ? 1 : request.Page;
        int pageSize = request.PageSize < 1 ? 10 : request.PageSize;

        var parameters = new GetPaymentsParameters(
            request.CustomerId,
            pageSize,
            (page - 1) * pageSize);

        IReadOnlyList<PaymentResponse> payments = await GetPaymentsAsync(connection, parameters, cancellationToken);

        int totalCount = await CountPaymentsAsync(connection, parameters, cancellationToken);

        return new GetPaymentsResponse(page, pageSize, totalCount, payments);
    }

    private static async Task<IReadOnlyList<PaymentResponse>> GetPaymentsAsync(
        DbConnection connection,
        GetPaymentsParameters parameters,
        CancellationToken cancellationToken)
    {
        const string sql =
            $"""
             SELECT
                 p.id AS {nameof(PaymentResponse.Id)},
                 p.order_id AS {nameof(PaymentResponse.OrderId)},
                 p.transaction_reference AS {nameof(PaymentResponse.TransactionReference)},
                 p.amount AS {nameof(PaymentResponse.Amount)},
                 p.currency AS {nameof(PaymentResponse.Currency)},
                 CASE p.status
                     WHEN 0 THEN 'Pending'
                     WHEN 1 THEN 'Succeeded'
                     WHEN 2 THEN 'Failed'
                 END AS {nameof(PaymentResponse.Status)},
                 p.amount_refunded AS {nameof(PaymentResponse.AmountRefunded)},
                 p.failure_reason AS {nameof(PaymentResponse.FailureReason)},
                 p.created_at_utc AS {nameof(PaymentResponse.CreatedAtUtc)},
                 p.paid_at_utc AS {nameof(PaymentResponse.PaidAtUtc)},
                 p.refunded_at_utc AS {nameof(PaymentResponse.RefundedAtUtc)}
             FROM ticketing.payments p
             JOIN ticketing.orders o ON o.id = p.order_id
             WHERE o.customer_id = @CustomerId
             ORDER BY p.created_at_utc DESC
             OFFSET @Skip
             LIMIT @Take
             """;

        List<PaymentResponse> payments = (await connection.QueryAsync<PaymentResponse>(new CommandDefinition(sql, parameters, cancellationToken: cancellationToken))).AsList();

        return payments;
    }

    private static async Task<int> CountPaymentsAsync(
        DbConnection connection,
        GetPaymentsParameters parameters,
        CancellationToken cancellationToken)
    {
        const string sql =
            """
            SELECT COUNT(*)
            FROM ticketing.payments p
            JOIN ticketing.orders o ON o.id = p.order_id
            WHERE o.customer_id = @CustomerId
            """;

        int totalCount = await connection.ExecuteScalarAsync<int>(new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));

        return totalCount;
    }

    private sealed record GetPaymentsParameters(Guid CustomerId, int Take, int Skip);
}
