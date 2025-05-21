using Evently.Common.Application.Authentication;
using Evently.Common.Domain;
using Evently.Common.Presentation.Endpoints;
using Evently.Common.Presentation.Results;
using Evently.Modules.Ticketing.Application.Carts;
using Evently.Modules.Ticketing.Application.Carts.GetCart;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Evently.Modules.Ticketing.Presentation.Carts;

internal sealed class GetCartEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("carts", async (ICurrentActor actor, ISender sender) =>
            {
                Result<Cart> result = await sender.Send(new GetCartQuery(actor.Id));

                return result.Match(Results.Ok<Cart>, ApiResults.Problem);
            })
            .RequireAuthorization(Permissions.GetCart)
            .WithTags(Tags.Carts)
            .WithName("Get Cart")
            .Produces<Cart>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithSummary("Retrieves a user's cart")
            .WithDescription("Fetches the details of the cart for the current user identified by their unique ID.");
    }
}