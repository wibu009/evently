using Evently.Common.Domain;
using Evently.Common.Presentation.Endpoints;
using Evently.Common.Presentation.Results;
using Evently.Modules.Ticketing.Application.Orders.CancelOrder;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Evently.Modules.Ticketing.Presentation.Orders;

internal sealed class CancelOrderEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("orders/{id:guid}/cancel", async (Guid id, Request request, ISender sender) =>
            {
                Result result = await sender.Send(new CancelOrderCommand(id, request.Reason));

                return result.Match(Results.NoContent, ApiResults.Problem);
            })
            .RequireAuthorization(Permissions.CancelOrder)
            .WithTags(Tags.Orders)
            .WithName("Cancel Order")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithSummary("Cancels an order")
            .WithDescription("Cancels a pending order and releases the reserved ticket inventory. Paid orders are refunded instead: the payment is refunded, the issued tickets are invalidated and the inventory is released.");
    }

    internal sealed record Request(string? Reason);
}
