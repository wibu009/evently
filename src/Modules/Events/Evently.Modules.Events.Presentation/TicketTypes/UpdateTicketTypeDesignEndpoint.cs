using Evently.Common.Domain;
using Evently.Common.Presentation.Endpoints;
using Evently.Common.Presentation.Results;
using Evently.Modules.Events.Application.TicketTypes.UpdateTicketTypeDesign;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Evently.Modules.Events.Presentation.TicketTypes;

internal sealed class UpdateTicketTypeDesignEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("ticket-types/{id:guid}/design", async (Guid id, Request request, ISender sender) =>
            {
                Result result = await sender.Send(
                    new UpdateTicketTypeDesignCommand(id, request.Color, request.BackgroundImageUrl));

                return result.Match(Results.NoContent, ApiResults.Problem);
            })
            .RequireAuthorization(Permissions.ModifyTicketTypes)
            .WithTags(Tags.TicketTypes)
            .WithName("Update Ticket Type Design")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithSummary("Customizes a ticket type's face design")
            .WithDescription("Sets an optional face color (hex, e.g. #7C3AED) and background image URL used when rendering issued tickets. Send null to clear a value. Design is display-only and is not propagated to other modules.");
    }

    private sealed record Request(string? Color, string? BackgroundImageUrl);
}
