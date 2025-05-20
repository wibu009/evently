using Evently.Common.Domain;
using Evently.Common.Presentation.Endpoints;
using Evently.Common.Presentation.Results;
using Evently.Modules.Events.Application.Events.RescheduleEvent;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Evently.Modules.Events.Presentation.Events;

internal sealed class RescheduleEventEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("events/{id:guid}/reschedule", async (Guid id, Request request, ISender sender) =>
            {
                Result result = await sender.Send(new RescheduleEventCommand(id, request.StartAtUtc, request.EndAtUtc));
                return result.Match(Results.NoContent, ApiResults.Problem);
            })
            .RequireAuthorization(Permissions.ModifyEvents)
            .WithTags(Tags.Events)
            .WithName("Reschedule Event")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithSummary("Reschedules an event")
            .WithDescription("Updates the start and end dates of an event identified by its unique ID. The start date must be valid, and the end date, if provided, must be after the start date.");
    }
    
    private sealed record Request(DateTime StartAtUtc, DateTime? EndAtUtc);
}
