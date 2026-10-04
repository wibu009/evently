using Evently.Common.Domain;
using Evently.Common.Presentation.Endpoints;
using Evently.Common.Presentation.Results;
using Evently.Modules.Ticketing.Application.PromoCodes.DeactivatePromoCode;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Evently.Modules.Ticketing.Presentation.PromoCodes;

internal sealed class DeactivatePromoCodeEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("promo-codes/{id:guid}/deactivate", async (Guid id, ISender sender) =>
            {
                Result result = await sender.Send(new DeactivatePromoCodeCommand(id));

                return result.Match(Results.NoContent, ApiResults.Problem);
            })
            .RequireAuthorization(Permissions.RemovePromoCode)
            .WithTags(Tags.PromoCodes)
            .WithName("Deactivate Promo Code")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithSummary("Deactivates a promo code")
            .WithDescription("Ends the validity of the promo code immediately, so it can no longer be applied at checkout.");
    }
}
