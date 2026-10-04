using System.Data.Common;
using Dapper;
using Evently.Common.Application.Data;
using Evently.Common.Application.Messaging;
using Evently.Common.Domain;
using Evently.Modules.Events.Application.Abstractions.Data;

namespace Evently.Modules.Events.Application.Events.GetEvents;

internal sealed class GetEventsQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetEventsQuery, GetEventsResponse>
{
    public async Task<Result<GetEventsResponse>> Handle(GetEventsQuery request, CancellationToken cancellationToken)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync(cancellationToken);

        int page = request.Page < 1 ? 1 : request.Page;
        int pageSize = request.PageSize < 1 ? 10 : request.PageSize;

        var parameters = new GetEventsParameters(pageSize, (page - 1) * pageSize);

        IReadOnlyCollection<EventResponse> events = await GetEventsAsync(connection, parameters, cancellationToken);

        int totalCount = await CountEventsAsync(connection, cancellationToken);

        return new GetEventsResponse(page, pageSize, totalCount, events);
    }

    private static async Task<IReadOnlyCollection<EventResponse>> GetEventsAsync(
        DbConnection connection,
        GetEventsParameters parameters,
        CancellationToken cancellationToken)
    {
        const string query =
            $"""
             SELECT
                 e.id AS {nameof(EventResponse.Id)},
                 e.category_id AS {nameof(EventResponse.CategoryId)},
                 e.title AS {nameof(EventResponse.Title)},
                 e.description AS {nameof(EventResponse.Description)},
                 e.location AS {nameof(EventResponse.Location)},
                 e.start_at_utc AS {nameof(EventResponse.StartAtUtc)},
                 e.end_at_utc AS {nameof(EventResponse.EndAtUtc)},
                 CASE e.status
                     WHEN 0 THEN 'Draft'
                     WHEN 1 THEN 'Published'
                     WHEN 2 THEN 'Completed'
                     WHEN 3 THEN 'Canceled'
                 END AS {nameof(EventResponse.Status)},
                 (
                     SELECT ei.image_url
                     FROM events.event_images ei
                     WHERE ei.event_id = e.id
                     ORDER BY ei.is_cover DESC, ei.display_order
                     LIMIT 1
                 ) AS {nameof(EventResponse.CoverImageUrl)}
             FROM events.events e
             ORDER BY e.start_at_utc
             OFFSET @Skip
             LIMIT @Take
             """;

        List<EventResponse> events = (await connection.QueryAsync<EventResponse>(
            new CommandDefinition(query, parameters, cancellationToken: cancellationToken))).AsList();

        return events;
    }

    private static async Task<int> CountEventsAsync(
        DbConnection connection,
        CancellationToken cancellationToken)
    {
        const string sql =
            """
            SELECT COUNT(*)
            FROM events.events
            """;

        int totalCount = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(sql, cancellationToken: cancellationToken));

        return totalCount;
    }

    private sealed record GetEventsParameters(int Take, int Skip);
}
