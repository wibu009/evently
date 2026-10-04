using System.Data.Common;
using Dapper;
using Evently.Common.Application.Data;
using Evently.Common.Application.Messaging;
using Evently.Common.Domain;
using Evently.Modules.Events.Application.Abstractions.Data;
using Evently.Modules.Events.Application.Events.GetEvents;
using Evently.Modules.Events.Domain.Events;

namespace Evently.Modules.Events.Application.Events.SearchEvents;

/// <summary>
/// Searches the public event catalog. Only published events are returned; the result can be
/// narrowed by a free-text search term (title, description, or location), a category, and a
/// date range. The page query and the count query apply the exact same filters.
/// </summary>
internal sealed class SearchEventsQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<SearchEventsQuery, SearchEventsResponse>
{
    public async Task<Result<SearchEventsResponse>> Handle(SearchEventsQuery request, CancellationToken cancellationToken)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync(cancellationToken);

        int page = request.Page < 1 ? 1 : request.Page;
        var parameters = new SearchEventsParameters(
            (int)EventStatus.Published,
            string.IsNullOrWhiteSpace(request.Search) ? null : $"%{request.Search.Trim()}%",
            request.CategoryId,
            request.StartDate?.Date,
            request.EndDate?.Date,
            request.PageSize,
            (page - 1) * request.PageSize);

        IReadOnlyCollection<EventResponse> events = await GetEventsAsync(connection, parameters, cancellationToken);

        int total = await CountEventsAsync(connection, parameters, cancellationToken);

        return new SearchEventsResponse(page, request.PageSize, total, events);
    }

    private static async Task<IReadOnlyCollection<EventResponse>> GetEventsAsync(
        DbConnection connection,
        SearchEventsParameters parameters,
        CancellationToken cancellationToken)
    {
        const string sql =
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
             WHERE
                 e.status = @Status AND
                 (@Search::text IS NULL OR e.title ILIKE @Search OR e.description ILIKE @Search OR e.location ILIKE @Search) AND
                 (@CategoryId IS NULL OR e.category_id = @CategoryId) AND
                 (@StartDate::timestamp IS NULL OR e.start_at_utc >= @StartDate::timestamp) AND
                 (@EndDate::timestamp IS NULL OR e.end_at_utc <= @EndDate::timestamp)
             ORDER BY e.start_at_utc
             OFFSET @Skip
             LIMIT @Take
             """;

        List<EventResponse> events = (await connection.QueryAsync<EventResponse>(
            new CommandDefinition(sql, parameters, cancellationToken: cancellationToken))).AsList();

        return events;
    }

    private static async Task<int> CountEventsAsync(
        DbConnection connection,
        SearchEventsParameters parameters,
        CancellationToken cancellationToken)
    {
        const string sql =
            """
            SELECT COUNT(*)
            FROM events.events
            WHERE
               status = @Status AND
               (@Search::text IS NULL OR title ILIKE @Search OR description ILIKE @Search OR location ILIKE @Search) AND
               (@CategoryId IS NULL OR category_id = @CategoryId) AND
               (@StartDate::timestamp IS NULL OR start_at_utc >= @StartDate::timestamp) AND
               (@EndDate::timestamp IS NULL OR end_at_utc <= @EndDate::timestamp)
            """;

        int totalCount = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));

        return totalCount;
    }

    private sealed record SearchEventsParameters(
        int Status,
        string? Search,
        Guid? CategoryId,
        DateTime? StartDate,
        DateTime? EndDate,
        int Take,
        int Skip);
}
