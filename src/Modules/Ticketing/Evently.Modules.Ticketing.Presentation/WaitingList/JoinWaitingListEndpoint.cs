using Evently.Common.Application.Authentication;
using Evently.Common.Domain;
using Evently.Common.Presentation.Endpoints;
using Evently.Common.Presentation.Results;
using Evently.Modules.Ticketing.Application.WaitingList.JoinWaitingList;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Evently.Modules.Ticketing.Presentation.WaitingList;

internal sealed class JoinWaitingListEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("waiting-lists/{ticketTypeId:guid}/join", async (Guid ticketTypeId, ICurrentActor actor, ISender sender) =>
            {
                Result result = await sender.Send(new JoinWaitingListCommand(actor.Id, ticketTypeId));

                return result.Match(Results.NoContent, ApiResults.Problem);
            })
            .RequireAuthorization(Permissions.JoinWaitingList)
            .WithTags(Tags.WaitingLists)
            .WithName("Join Waiting List")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithSummary("Joins the waiting list of a sold out ticket type")
            .WithDescription("Adds the current customer to the waiting list of the given ticket type. When inventory becomes available again, the earliest waiting customers are notified in FIFO order.");
    }
}
