using System.Data.Common;
using Dapper;
using Evently.Modules.Events.Application.Abstractions.Data;
using MediatR;

namespace Evently.Modules.Events.Application.Events.GetEvent;

public sealed class GetEventCommandHandler(IDbConnectionFactory dbConnectionFactory) : IRequestHandler<GetEventQuery, EventResponse?>
{
    public async Task<EventResponse?> Handle(GetEventQuery request, CancellationToken cancellationToken)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync(cancellationToken);

        const string sql =
            $"""
             SELECT
                 id AS {nameof(EventResponse.Id)},
                 title AS {nameof(EventResponse.Title)},
                 description AS {nameof(EventResponse.Description)},
                 location AS {nameof(EventResponse.Location)},
                 start_at_utc AS {nameof(EventResponse.StartAtUtc)},
                 end_at_utc AS {nameof(EventResponse.EndAtUtc)}
             FROM events.events
             WHERE id = @eventId
             """;

        EventResponse? @event = await connection.QueryFirstOrDefaultAsync<EventResponse>(sql, request.EventId);

        return @event;
    }
}
