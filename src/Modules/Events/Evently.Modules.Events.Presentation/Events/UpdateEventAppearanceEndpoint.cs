using Evently.Common.Domain;
using Evently.Common.Presentation.Endpoints;
using Evently.Common.Presentation.Results;
using Evently.Modules.Events.Application.Events.UpdateEventAppearance;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Evently.Modules.Events.Presentation.Events;

internal sealed class UpdateEventAppearanceEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("events/{id:guid}/appearance", async (Guid id, Request request, ISender sender) =>
            {
                Result result = await sender.Send(
                    new UpdateEventAppearanceCommand(id, request.HeroBannerUrl, request.AccentColor));

                return result.Match(Results.NoContent, ApiResults.Problem);
            })
            .RequireAuthorization(Permissions.ModifyEvents)
            .WithTags(Tags.Events)
            .WithName("Update Event Appearance")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithSummary("Customizes the hero banner and accent color of an event")
            .WithDescription("Sets an optional hero banner image URL and accent hex color (e.g. #7C3AED) used to theme the event detail page and its tickets. Send null to clear a value. Appearance is display-only and is not propagated to other modules.");
    }

    internal sealed record Request(string? HeroBannerUrl, string? AccentColor);
}
