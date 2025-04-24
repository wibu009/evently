using Evently.Modules.Events.Api.Database;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Evently.Modules.Events.Api.Events;

public static class CreateEvent
{
    public static void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("events", async (Request request, EventsDbContext context) =>
            {
                var @event = new Event
                {
                    Id = Guid.NewGuid(),
                    Title = request.Title,
                    Description = request.Description,
                    Location = request.Location,
                    StartAtUtc = request.StartAtUtc,
                    EndAtUtc = request.EndAtUtc,
                    Status = EventStatus.Draft
                };

                await context.Events.AddAsync(@event);
                await context.SaveChangesAsync();

                return Results.Created($"/events/{@event.Id}", @event);
            })
            .WithTags(Tags.Events);
    }

    internal sealed class Request
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public DateTime StartAtUtc { get; set; } 
        public DateTime? EndAtUtc { get; set; }
    }
}
