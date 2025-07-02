using Evently.Common.Application.Authentication;
using Evently.Common.Domain;
using Evently.Common.Presentation.Endpoints;
using Evently.Common.Presentation.Results;
using Evently.Modules.Ticketing.Application.Orders.GetOrders;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Evently.Modules.Ticketing.Presentation.Orders;

internal sealed class GetOrdersEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("orders", async (ICurrentActor actor, ISender sender) =>
            {
                Result<IReadOnlyList<OrderResponse>> result = await sender.Send(new GetOrdersQuery(actor.Id));

                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .RequireAuthorization(Permissions.GetOrders)
            .WithTags(Tags.Orders)
            .WithName("Get Orders")
            .Produces<IReadOnlyList<OrderResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithSummary("Retrieves a list of orders for the current user")
            .WithDescription("Fetches all orders associated with the current user. Returns the list of orders or an error if something goes wrong.");
    }
}
