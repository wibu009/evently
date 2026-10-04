using Evently.Common.Application.Authentication;
using Evently.Common.Domain;
using Evently.Common.Presentation.Endpoints;
using Evently.Common.Presentation.Results;
using Evently.Modules.Ticketing.Application.WaitingList.LeaveWaitingList;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Evently.Modules.Ticketing.Presentation.WaitingList;

internal sealed class LeaveWaitingListEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("waiting-lists/{ticketTypeId:guid}/leave", async (Guid ticketTypeId, ICurrentActor actor, ISender sender) =>
            {
                Result result = await sender.Send(new LeaveWaitingListCommand(actor.Id, ticketTypeId));

                return result.Match(Results.NoContent, ApiResults.Problem);
            })
            .RequireAuthorization(Permissions.LeaveWaitingList)
            .WithTags(Tags.WaitingLists)
            .WithName("Leave Waiting List")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithSummary("Leaves the waiting list of a ticket type")
            .WithDescription("Removes the current customer from the waiting list of the given ticket type.");
    }
}
