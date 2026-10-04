using System.Data.Common;
using Dapper;
using Evently.Common.Application.Clock;
using Evently.Common.Application.Data;
using Evently.Common.Application.Messaging;
using Evently.Common.Domain;
using Evently.Modules.Ticketing.Application.Orders;
using Evently.Modules.Ticketing.Application.Orders.ExpireOrder;
using Evently.Modules.Ticketing.Domain.Orders;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;

namespace Evently.Modules.Ticketing.Infrastructure.Orders;

/// <summary>
/// The timeout of the order fulfillment saga: finds pending orders whose payment deadline has
/// passed and expires them, which releases the reserved ticket inventory as compensation.
/// </summary>
[DisallowConcurrentExecution]
internal sealed class ProcessOrderExpirationsJob(
    IDbConnectionFactory dbConnectionFactory,
    IServiceScopeFactory serviceScopeFactory,
    IDateTimeProvider dateTimeProvider,
    IOptions<OrdersOptions> ordersOptions,
    ILogger<ProcessOrderExpirationsJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        logger.LogInformation("Ticketing - Beginning to expire stale orders");

        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        IReadOnlyList<Guid> staleOrderIds = await GetStaleOrderIdsAsync(connection);

        foreach (Guid orderId in staleOrderIds)
        {
            using IServiceScope scope = serviceScopeFactory.CreateScope();

            ISender sender = scope.ServiceProvider.GetRequiredService<ISender>();

            Result result = await sender.Send(new ExpireOrderCommand(orderId), context.CancellationToken);

            if (result.IsFailure)
            {
                logger.LogError(
                    "Failed to expire the stale order {OrderId}: {Error}",
                    orderId,
                    result.Error.Description);
            }
        }

        logger.LogInformation("Ticketing - Completed expiring stale orders");
    }

    private async Task<IReadOnlyList<Guid>> GetStaleOrderIdsAsync(DbConnection connection)
    {
        const string sql =
            """
            SELECT
                id
            FROM ticketing.orders
            WHERE
                status = @PendingStatus AND
                payment_due_utc IS NOT NULL AND
                payment_due_utc < @Now
            LIMIT @BatchSize
            """;

        IEnumerable<Guid> orderIds = await connection.QueryAsync<Guid>(
            sql,
            new
            {
                PendingStatus = (int)OrderStatus.Pending,
                Now = dateTimeProvider.UtcNow,
                BatchSize = ordersOptions.Value.ExpirationBatchSize
            });

        return orderIds.ToList();
    }
}
