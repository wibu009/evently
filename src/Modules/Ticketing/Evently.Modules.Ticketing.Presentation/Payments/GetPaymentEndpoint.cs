using Evently.Common.Domain;
using Evently.Common.Presentation.Endpoints;
using Evently.Common.Presentation.Results;
using Evently.Modules.Ticketing.Application.Payments;
using Evently.Modules.Ticketing.Application.Payments.GetPayment;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Evently.Modules.Ticketing.Presentation.Payments;

internal sealed class GetPaymentEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("payments/{id:guid}", async (Guid id, ISender sender) =>
            {
                Result<PaymentResponse> result = await sender.Send(new GetPaymentQuery(id));

                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .RequireAuthorization(Permissions.GetPayments)
            .WithTags(Tags.Payments)
            .WithName("Get Payment")
            .Produces<PaymentResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithSummary("Retrieves a payment by its unique identifier")
            .WithDescription("Fetches the details of a payment, including its lifecycle status and refunded amount. Returns the payment if found, or an error if the payment does not exist.");
    }
}
