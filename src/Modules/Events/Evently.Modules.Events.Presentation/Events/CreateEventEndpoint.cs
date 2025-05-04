using Evently.Common.Domain;
using Evently.Common.Presentation.Endpoints;
using Evently.Common.Presentation.Results;
using Evently.Modules.Events.Application.Events.CreateEvent;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Evently.Modules.Events.Presentation.Events;

internal sealed class CreateEventEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("events", async (Request request, ISender sender) =>
            {
                Result<Guid> result = await sender.Send(new CreateEventCommand(
                    request.CategoryId,
                    request.Title,
                    request.Description,
                    request.Location,
                    request.StartAtUtc,
                    request.EndAtUtc));
                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .WithTags(Tags.Events)
            .WithName("Create Event")
            .Produces<Guid>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithSummary("Creates a new event")
            .WithDescription("Creates an event with the provided details and returns its unique identifier. Requires a valid category ID, non-empty title, and valid start date.");
    }

    private sealed record Request(
        Guid CategoryId,
        string Title,
        string Description,
        string Location,
        DateTime StartAtUtc,
        DateTime? EndAtUtc);
}
