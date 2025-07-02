using Evently.Common.Domain;
using Evently.Common.Presentation.Endpoints;
using Evently.Common.Presentation.Results;
using Evently.Modules.Ticketing.Application.Orders.GetOrder;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using OrderResponse = Evently.Modules.Ticketing.Application.Orders.GetOrder.OrderResponse;

namespace Evently.Modules.Ticketing.Presentation.Orders;

internal sealed class GetOrderEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("orders/{id:guid}", async (Guid id, ISender sender) =>
            {
                Result<OrderResponse> result = await sender.Send(new GetOrderQuery(id));

                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .RequireAuthorization(Permissions.GetOrders)
            .WithTags(Tags.Orders)
            .WithName("Get Order")
            .Produces<OrderResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithSummary("Retrieves an order by its unique identifier")
            .WithDescription("Fetches the details of an order identified by its GUID. Returns the order if found, or an error if the order does not exist.");
    }
}
