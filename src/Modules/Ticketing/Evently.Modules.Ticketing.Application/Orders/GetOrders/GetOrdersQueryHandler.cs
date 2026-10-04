using System.Data.Common;
using Dapper;
using Evently.Common.Application.Data;
using Evently.Common.Application.Messaging;
using Evently.Common.Domain;
using Evently.Modules.Ticketing.Application.Orders.GetOrder;

namespace Evently.Modules.Ticketing.Application.Orders.GetOrders;

internal sealed class GetOrdersQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetOrdersQuery, GetOrdersResponse>
{
    public async Task<Result<GetOrdersResponse>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync(cancellationToken);

        int page = request.Page < 1 ? 1 : request.Page;
        int pageSize = request.PageSize < 1 ? 10 : request.PageSize;

        var parameters = new GetOrdersParameters(
            request.CustomerId,
            pageSize,
            (page - 1) * pageSize);

        IReadOnlyList<OrderResponse> orders = await GetOrdersAsync(connection, parameters, cancellationToken);

        int totalCount = await CountOrdersAsync(connection, parameters, cancellationToken);

        return new GetOrdersResponse(page, pageSize, totalCount, orders);
    }

    private static async Task<IReadOnlyList<OrderResponse>> GetOrdersAsync(
        DbConnection connection,
        GetOrdersParameters parameters,
        CancellationToken cancellationToken)
    {
        const string sql =
            $"""
             SELECT
                 id AS {nameof(OrderResponse.Id)},
                 customer_id AS {nameof(OrderResponse.CustomerId)},
                 CASE status
                     WHEN 0 THEN 'Pending'
                     WHEN 1 THEN 'Paid'
                     WHEN 2 THEN 'Refunded'
                     WHEN 3 THEN 'Canceled'
                     WHEN 4 THEN 'Expired'
                 END AS {nameof(OrderResponse.Status)},
                 total_price AS {nameof(OrderResponse.TotalPrice)},
                 discount_amount AS {nameof(OrderResponse.DiscountAmount)},
                 currency AS {nameof(OrderResponse.Currency)},
                 created_at_utc AS {nameof(OrderResponse.CreatedAtUtc)}
             FROM ticketing.orders
             WHERE customer_id = @CustomerId
             ORDER BY created_at_utc DESC
             OFFSET @Skip
             LIMIT @Take
             """;

        List<OrderResponse> orders = (await connection.QueryAsync<OrderResponse>(new CommandDefinition(sql, parameters, cancellationToken: cancellationToken))).AsList();

        return orders;
    }

    private static async Task<int> CountOrdersAsync(
        DbConnection connection,
        GetOrdersParameters parameters,
        CancellationToken cancellationToken)
    {
        const string sql =
            """
            SELECT COUNT(*)
            FROM ticketing.orders
            WHERE customer_id = @CustomerId
            """;

        int totalCount = await connection.ExecuteScalarAsync<int>(new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));

        return totalCount;
    }

    private sealed record GetOrdersParameters(Guid CustomerId, int Take, int Skip);
}
