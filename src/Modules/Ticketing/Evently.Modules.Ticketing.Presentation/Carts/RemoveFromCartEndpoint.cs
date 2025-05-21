using Evently.Common.Application.Authentication;
using Evently.Common.Domain;
using Evently.Common.Presentation.Endpoints;
using Evently.Common.Presentation.Results;
using Evently.Modules.Ticketing.Application.Carts.RemoveItemFromCart;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Evently.Modules.Ticketing.Presentation.Carts;

internal sealed class RemoveFromCartEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("carts/remove", async (Request request, ICurrentActor actor, ISender sender) =>
            {
                Result result = await sender.Send(
                    new RemoveItemFromCartCommand(actor.Id, request.TicketTypeId));

                return result.Match(Results.NoContent, ApiResults.Problem);
            })
            .RequireAuthorization(Permissions.RemoveFromCart)
            .WithTags(Tags.Carts)
            .WithName("Remove Item from Cart")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithSummary("Removes an item from a user's cart")
            .WithDescription("Removes a specified ticket type from the cart of the current user identified by their unique ID. The operation is idempotent.");
    }

    private sealed record Request(Guid TicketTypeId);
}