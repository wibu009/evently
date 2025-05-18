using Evently.Common.Domain;
using Evently.Common.Presentation.Endpoints;
using Evently.Common.Presentation.Results;
using Evently.Modules.Events.Application.Categories.ArchiveCategory;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Evently.Modules.Events.Presentation.Categories;

internal sealed class ArchiveCategoryEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("categories/{id:guid}/archive", async (Guid id, ISender sender) =>
            {
                Result result = await sender.Send(new ArchiveCategoryCommand(id));
                return result.Match(Results.NoContent, ApiResults.Problem);
            })
            .RequireAuthorization()
            .WithTags(Tags.Categories)
            .WithName("Archive Category")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithSummary("Archives a category by its ID")
            .WithDescription("Marks a category as archived, making it inactive without permanent deletion. The operation is idempotent.");
    }
}
