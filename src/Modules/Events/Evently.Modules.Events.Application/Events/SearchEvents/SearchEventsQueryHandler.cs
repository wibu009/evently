using System.Data.Common;
using Dapper;
using Evently.Common.Application.Data;
using Evently.Common.Application.Messaging;
using Evently.Common.Domain;
using Evently.Modules.Events.Application.Abstractions.Data;
using Evently.Modules.Events.Application.Events.GetEvents;
using Evently.Modules.Events.Domain.Events;

namespace Evently.Modules.Events.Application.Events.SearchEvents;

internal sealed class SearchEventsQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<SearchEventsQuery, SearchEventsResponse>
{
    public async Task<Result<SearchEventsResponse>> Handle(SearchEventsQuery request, CancellationToken cancellationToken)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync(cancellationToken);
        
        int page = request.Page < 1 ? 1 : request.Page;
        var parameters = new SearchEventsParameters(
            (int)EventStatus.Published,
            request.CategoryId,
            request.StartDate?.Date,
            request.EndDate?.Date,
            request.PageSize,
            (page - 1) * request.PageSize);
        
        IReadOnlyCollection<EventResponse> events = await GetEventsAsync(connection, parameters);
        
        int total = await CountEventsAsync(connection, parameters);
        
        return new SearchEventsResponse(request.Page, request.PageSize, total, events);
    }

    private static async Task<IReadOnlyCollection<EventResponse>> GetEventsAsync(DbConnection connection, SearchEventsParameters parameters)
    {
        const string sql =
            $"""
             SELECT
                 id AS {nameof(EventResponse.Id)},
                 category_id AS {nameof(EventResponse.CategoryId)},
                 title AS {nameof(EventResponse.Title)},
                 description AS {nameof(EventResponse.Description)},
                 location AS {nameof(EventResponse.Location)},
                 start_at_utc AS {nameof(EventResponse.StartAtUtc)},
                 end_at_utc AS {nameof(EventResponse.EndAtUtc)}
             FROM events.events
             WHERE
                 status = @Status AND
                 (@CategoryId IS NULL OR category_id = @CategoryId) AND
                 (@StartDate::timestamp IS NULL OR start_at_utc >= @StartDate::timestamp) AND
                 (@EndDate::timestamp IS NULL OR end_at_utc <= @EndDate::timestamp)
             ORDER BY start_at_utc
             OFFSET @Skip
             LIMIT @Take
             """;
        
        List<EventResponse> events = (await connection.QueryAsync<EventResponse>(sql, parameters)).AsList();
        
        return events;
    }

    private static async Task<int> CountEventsAsync(DbConnection connection, SearchEventsParameters parameters)
    {
        const string sql =
            """
            SELECT COUNT(*)
            FROM events.events
            WHERE
               status = @Status AND
               (@CategoryId IS NULL OR category_id = @CategoryId) AND
               (@StartDate::timestamp IS NULL OR start_at_utc >= @StartDate::timestamp) AND
               (@EndDate::timestamp IS NULL OR end_at_utc >= @EndDate::timestamp)
            """;
        int totalCount = await connection.ExecuteScalarAsync<int>(sql, parameters);
        
        return totalCount;
    }
    
    private sealed record SearchEventsParameters(
        int Status,
        Guid? CategoryId,
        DateTime? StartDate,
        DateTime? EndDate,
        int Take,
        int Skip);
}
