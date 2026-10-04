using System.Data.Common;
using Dapper;
using Evently.Common.Application.Data;
using Evently.Common.Application.Messaging;
using Evently.Common.Domain;
using Evently.Modules.Events.Application.Abstractions.Data;
using Evently.Modules.Events.Domain.Events;

namespace Evently.Modules.Events.Application.Events.GetEvent;

internal sealed class GetEventQueryHandler(
    IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetEventQuery, EventResponse>
{
    public async Task<Result<EventResponse>> Handle(GetEventQuery request, CancellationToken cancellationToken)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync(cancellationToken);

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
                 e.hero_banner_url AS {nameof(EventResponse.HeroBannerUrl)},
                 e.accent_color AS {nameof(EventResponse.AccentColor)},
                 tt.id AS {nameof(TicketTypeResponse.TicketTypeId)},
                 tt.name AS {nameof(TicketTypeResponse.Name)},
                 tt.price AS {nameof(TicketTypeResponse.Price)},
                 tt.currency AS {nameof(TicketTypeResponse.Currency)},
                 tt.quantity AS {nameof(TicketTypeResponse.Quantity)},
                 tt.color AS {nameof(TicketTypeResponse.Color)},
                 tt.background_image_url AS {nameof(TicketTypeResponse.BackgroundImageUrl)}
             FROM events.events e
             LEFT JOIN events.ticket_types tt ON tt.event_id = e.id
             WHERE e.id = @EventId
             """;
        Dictionary<Guid, EventResponse> eventsDictionary = [];
        await connection.QueryAsync<EventResponse, TicketTypeResponse?, EventResponse>(
            sql,
            (@event, ticketType) =>
            {
                if (eventsDictionary.TryGetValue(@event.Id, out EventResponse? existingEvent))
                {
                    @event = existingEvent;
                }
                else
                {
                    eventsDictionary.Add(@event.Id, @event);
                }

                if (ticketType is not null)
                {
                    @event.TicketTypes.Add(ticketType);
                }

                return @event;
            },
            request,
            splitOn: nameof(TicketTypeResponse.TicketTypeId));

        if (!eventsDictionary.TryGetValue(request.EventId, out EventResponse eventDictionary))
        {
            return Result.Failure<EventResponse>(EventErrors.NotFound(request.EventId));
        }

        const string imagesSql =
            $"""
             SELECT
                 id AS {nameof(EventImageResponse.ImageId)},
                 image_url AS {nameof(EventImageResponse.ImageUrl)},
                 is_cover AS {nameof(EventImageResponse.IsCover)},
                 display_order AS {nameof(EventImageResponse.DisplayOrder)}
             FROM events.event_images
             WHERE event_id = @EventId
             ORDER BY display_order
             """;

        IEnumerable<EventImageResponse> images = await connection.QueryAsync<EventImageResponse>(
            new CommandDefinition(imagesSql, request, cancellationToken: cancellationToken));

        eventDictionary.Images.AddRange(images);

        return eventDictionary;
    }
}
