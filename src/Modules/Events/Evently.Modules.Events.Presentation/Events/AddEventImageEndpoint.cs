using Evently.Common.Domain;
using Evently.Common.Presentation.Endpoints;
using Evently.Common.Presentation.Results;
using Evently.Modules.Events.Application.Events.AddEventImage;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Evently.Modules.Events.Presentation.Events;

internal sealed class AddEventImageEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("events/{id:guid}/images", async (Guid id, Request request, ISender sender) =>
            {
                Result<Guid> result = await sender.Send(
                    new AddEventImageCommand(id, request.ImageUrl, request.SetAsCover));

                return result.Match(
                    imageId => Results.Created($"/events/{id}/images/{imageId}", new { id = imageId }),
                    ApiResults.Problem);
            })
            .RequireAuthorization(Permissions.ModifyEvents)
            .WithTags(Tags.Events)
            .WithName("Add Event Image")
            .Produces(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithSummary("Adds a gallery image to an event")
            .WithDescription("Registers an image URL in the event gallery. The first image becomes the cover; pass setAsCover to promote the new image immediately. Images are display-only and are not propagated to other modules.");
    }

    internal sealed record Request(string ImageUrl, bool SetAsCover);
}
