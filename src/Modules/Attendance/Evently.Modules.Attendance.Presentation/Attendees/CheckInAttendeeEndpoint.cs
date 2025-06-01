using Evently.Common.Application.Authentication;
using Evently.Common.Domain;
using Evently.Common.Presentation.Endpoints;
using Evently.Common.Presentation.Results;
using Evently.Modules.Attendance.Application.Attendees.CheckInAttendee;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Evently.Modules.Attendance.Presentation.Attendees;

internal sealed class CheckInAttendeeEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("attendees/check-in", async (
                Request request,
                ICurrentActor actor,
                ISender sender) =>
            {
                Result result = await sender.Send(
                    new CheckInAttendeeCommand(actor.Id, request.TicketId));

                return result.Match(Results.NoContent, ApiResults.Problem);
            })
            .RequireAuthorization(Permissions.CheckInTicket)
            .WithTags(Tags.Attendees)
            .WithName("Check In Attendee")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithSummary("Checks in an attendee for a ticket")
            .WithDescription("Marks an attendee as checked in for a specific ticket.");
    }
    
    private sealed record Request(Guid TicketId);
}
