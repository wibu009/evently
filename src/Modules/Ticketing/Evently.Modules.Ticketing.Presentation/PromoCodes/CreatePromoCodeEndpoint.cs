using Evently.Common.Domain;
using Evently.Common.Presentation.Endpoints;
using Evently.Common.Presentation.Results;
using Evently.Modules.Ticketing.Application.PromoCodes.CreatePromoCode;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Evently.Modules.Ticketing.Presentation.PromoCodes;

internal sealed class CreatePromoCodeEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("promo-codes", async (Request request, ISender sender) =>
            {
                Result<Guid> result = await sender.Send(
                    new CreatePromoCodeCommand(
                        request.Code,
                        request.DiscountType,
                        request.DiscountValue,
                        request.Currency,
                        request.MaxRedemptions,
                        request.ValidFromUtc,
                        request.ValidUntilUtc));

                return result.Match(
                    promoCodeId => Results.Created($"/promo-codes/{promoCodeId}", promoCodeId),
                    ApiResults.Problem);
            })
            .RequireAuthorization(Permissions.CreatePromoCode)
            .WithTags(Tags.PromoCodes)
            .WithName("Create Promo Code")
            .Produces(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithSummary("Creates a promo code")
            .WithDescription("Creates a promo code with a percentage or fixed amount discount, an optional redemption limit, and an optional validity window.");
    }

    internal sealed record Request(
        string Code,
        int DiscountType,
        decimal DiscountValue,
        string Currency,
        int? MaxRedemptions,
        DateTime? ValidFromUtc,
        DateTime? ValidUntilUtc);
}
