using System.Data.Common;
using Dapper;
using Evently.Common.Application.Data;
using Evently.Common.Application.Messaging;
using Evently.Common.Domain;

namespace Evently.Modules.Ticketing.Application.WaitingList.GetMyWaitingList;

internal sealed class GetMyWaitingListQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetMyWaitingListQuery, IReadOnlyList<WaitingListEntryResponse>>
{
    public async Task<Result<IReadOnlyList<WaitingListEntryResponse>>> Handle(
        GetMyWaitingListQuery request,
        CancellationToken cancellationToken)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync(cancellationToken);

        const string sql =
            $"""
             SELECT
                 id AS {nameof(WaitingListEntryResponse.Id)},
                 ticket_type_id AS {nameof(WaitingListEntryResponse.TicketTypeId)},
                 CASE status
                     WHEN 0 THEN 'Waiting'
                     WHEN 1 THEN 'Notified'
                 END AS {nameof(WaitingListEntryResponse.Status)},
                 created_at_utc AS {nameof(WaitingListEntryResponse.CreatedAtUtc)},
                 notified_at_utc AS {nameof(WaitingListEntryResponse.NotifiedAtUtc)}
             FROM ticketing.waiting_list_entries
             WHERE customer_id = @CustomerId
             ORDER BY created_at_utc DESC
             """;

        List<WaitingListEntryResponse> entries = (await connection.QueryAsync<WaitingListEntryResponse>(
            new CommandDefinition(sql, request, cancellationToken: cancellationToken))).AsList();

        return entries;
    }
}
