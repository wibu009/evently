using Evently.Common.Application.Authentication;
using Evently.Common.Domain;
using Evently.Common.Presentation.Endpoints;
using Evently.Common.Presentation.Results;
using Evently.Modules.Ticketing.Application.Orders.CreateOrder;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Evently.Modules.Ticketing.Presentation.Orders;

internal sealed class CreateOrderEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("orders", async (ICurrentActor actor, ISender sender) =>
            {
                Result<Guid> result = await sender.Send(new CreateOrderCommand(actor.Id));

                return result.Match(
                    orderId => Results.CreatedAtRoute(
                        "Get Order",
                        new { id = orderId },
                        new { id = orderId }),
                    ApiResults.Problem);
            })
            .RequireAuthorization(Permissions.CreateOrder)
            .WithTags(Tags.Orders)
            .WithName("Create Order")
            .Produces(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithSummary("Checks out the cart and creates a new pending order for the user")
            .WithDescription("Reserves the ticket inventory of the cart and creates a pending order with a payment deadline. Returns the id of the created order.");
    }
}
