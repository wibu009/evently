using Evently.Common.Domain;
using Evently.Common.Presentation.Endpoints;
using Evently.Common.Presentation.Results;
using Evently.Modules.Events.Application.Events.SetCoverImage;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Evently.Modules.Events.Presentation.Events;

internal sealed class SetCoverImageEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("events/{id:guid}/images/{imageId:guid}/cover", async (Guid id, Guid imageId, ISender sender) =>
            {
                Result result = await sender.Send(new SetCoverImageCommand(id, imageId));

                return result.Match(Results.NoContent, ApiResults.Problem);
            })
            .RequireAuthorization(Permissions.ModifyEvents)
            .WithTags(Tags.Events)
            .WithName("Set Cover Image")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithSummary("Promotes a gallery image to the event cover")
            .WithDescription("The cover image is used on catalog cards and as the fallback hero banner of the event detail page.");
    }
}
