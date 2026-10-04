using Evently.Common.Domain;
using Evently.Common.Presentation.Endpoints;
using Evently.Common.Presentation.Results;
using Evently.Modules.Attendance.Application.Attendees.CheckInTicketByCode;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Evently.Modules.Attendance.Presentation.Attendees;

internal sealed class CheckInTicketByCodeEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("attendees/check-in-by-code", async (Request request, ISender sender) =>
            {
                Result<CheckInTicketByCodeResponse> result = await sender.Send(
                    new CheckInTicketByCodeCommand(request.TicketCode));

                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .RequireAuthorization(Permissions.CheckInTicket)
            .WithTags(Tags.Attendees)
            .WithName("Check In Ticket By Code")
            .Produces<CheckInTicketByCodeResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithSummary("Checks in a ticket by its QR code")
            .WithDescription("Scans a ticket code at the gate and checks in its owner. Returns an explicit outcome (CheckedIn, Duplicate, NotFound, Invalid) so scanners keep flowing without retries. Resolves in a single indexed lookup.");
    }

    private sealed record Request(string TicketCode);
}
