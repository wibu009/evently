using Evently.Common.Domain;
using Evently.Common.Presentation.Endpoints;
using Evently.Common.Presentation.Results;
using Evently.Modules.Ticketing.Application.Payments.RefundPayment;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Evently.Modules.Ticketing.Presentation.Payments;

internal sealed class RefundPaymentEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("payments/{id:guid}/refund", async (Guid id, Request request, ISender sender) =>
            {
                Result result = await sender.Send(new RefundPaymentCommand(id, request.Amount));

                return result.Match(Results.NoContent, ApiResults.Problem);
            })
            .RequireAuthorization(Permissions.RefundPayments)
            .WithTags(Tags.Payments)
            .WithName("Refund Payment")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithSummary("Refunds a payment, either partially or fully")
            .WithDescription("Refunds the given amount of the payment, or its full remaining balance when no amount is provided. Once a payment is fully refunded, the related order is marked as refunded, its tickets are invalidated and the ticket inventory is released.");
    }

    internal sealed record Request(decimal? Amount);
}
