using Evently.Common.Domain;
using Evently.Common.Presentation.Endpoints;
using Evently.Common.Presentation.Results;
using Evently.Modules.Events.Application.Events.PublishEvent;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Evently.Modules.Events.Presentation.Events;

internal sealed class PublishEventEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("events/{id:guid}/publish", async (Guid id, ISender sender) =>
            {
                Result result = await sender.Send(new PublishEventCommand(id));
                return result.Match(Results.NoContent, ApiResults.Problem);
            })
            .RequireAuthorization()
            .WithTags(Tags.Events)
            .WithName("Publish Event")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithSummary("Publishes an event by its ID")
            .WithDescription("Marks an event as published, making it publicly visible. The operation is idempotent.");
    }
}
