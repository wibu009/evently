using Evently.Common.Application.Authentication;
using Evently.Common.Domain;
using Evently.Common.Presentation.Endpoints;
using Evently.Common.Presentation.Results;
using Evently.Modules.Ticketing.Application.WaitingList.GetMyWaitingList;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Evently.Modules.Ticketing.Presentation.WaitingList;

internal sealed class GetMyWaitingListEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("waiting-lists", async (ICurrentActor actor, ISender sender) =>
            {
                Result<IReadOnlyList<WaitingListEntryResponse>> result =
                    await sender.Send(new GetMyWaitingListQuery(actor.Id));

                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .RequireAuthorization(Permissions.GetWaitingList)
            .WithTags(Tags.WaitingLists)
            .WithName("Get My Waiting List")
            .Produces<IReadOnlyList<WaitingListEntryResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithSummary("Retrieves the waiting list entries of the current customer")
            .WithDescription("Fetches all waiting list entries of the current customer, most recent first.");
    }
}
