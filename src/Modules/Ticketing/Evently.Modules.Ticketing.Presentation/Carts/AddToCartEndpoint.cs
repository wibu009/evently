using Evently.Common.Domain;
using Evently.Common.Presentation.Endpoints;
using Evently.Common.Presentation.Results;
using Evently.Modules.Ticketing.Application.Carts.AddItemToCart;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Evently.Modules.Ticketing.Presentation.Carts;

internal sealed class AddToCartEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("carts/add", async (Request request, ISender sender) =>
            {
                Result result = await sender.Send(
                    new AddItemToCartCommand(
                        request.CustomerId,
                        request.TicketTypeId,
                        request.Quantity));

                return result.Match(Results.NoContent, ApiResults.Problem);
            })
            .WithTags(Tags.Carts)
            .WithName("Add Item to Cart")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithSummary("Adds an item to a customer's cart")
            .WithDescription("Adds a specified quantity of a ticket type to the cart of a customer identified by their unique GUID. The operation is idempotent.");
    }
    
    private sealed record Request(Guid CustomerId, Guid TicketTypeId, decimal Quantity);
}
