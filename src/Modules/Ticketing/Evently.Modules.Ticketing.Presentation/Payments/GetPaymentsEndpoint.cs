using Evently.Common.Application.Authentication;
using Evently.Common.Domain;
using Evently.Common.Presentation.Endpoints;
using Evently.Common.Presentation.Results;
using Evently.Modules.Ticketing.Application.Payments;
using Evently.Modules.Ticketing.Application.Payments.GetPayments;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Evently.Modules.Ticketing.Presentation.Payments;

internal sealed class GetPaymentsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("payments", async (ICurrentActor actor, ISender sender, int page = 1, int pageSize = 10) =>
            {
                Result<GetPaymentsResponse> result = await sender.Send(new GetPaymentsQuery(actor.Id, page, pageSize));

                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .RequireAuthorization(Permissions.GetPayments)
            .WithTags(Tags.Payments)
            .WithName("Get Payments")
            .Produces<GetPaymentsResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithSummary("Retrieves a paged list of payments for the current user")
            .WithDescription("Fetches the payments associated with the orders of the current user, most recent first. Returns a paged result or an error if something goes wrong.");
    }
}
