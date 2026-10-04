using Evently.Common.Domain;
using Evently.Common.Presentation.Endpoints;
using Evently.Common.Presentation.Results;
using Evently.Modules.Ticketing.Application.PromoCodes;
using Evently.Modules.Ticketing.Application.PromoCodes.GetPromoCodes;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Evently.Modules.Ticketing.Presentation.PromoCodes;

internal sealed class GetPromoCodesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("promo-codes", async (ISender sender, int page = 1, int pageSize = 10) =>
            {
                Result<GetPromoCodesResponse> result = await sender.Send(new GetPromoCodesQuery(page, pageSize));

                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .RequireAuthorization(Permissions.GetPromoCodes)
            .WithTags(Tags.PromoCodes)
            .WithName("Get Promo Codes")
            .Produces<GetPromoCodesResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithSummary("Retrieves a paged list of promo codes")
            .WithDescription("Fetches all promo codes with their redemption counts and validity windows.");
    }
}
