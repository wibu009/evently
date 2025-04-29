using Evently.Common.Domain;
using Evently.Common.Presentation.Endpoints;
using Evently.Common.Presentation.Results;
using Evently.Modules.Events.Application.Categories.UpdateCategory;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Evently.Modules.Events.Presentation.Categories;

internal sealed class UpdateCategory : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("categories/{id:guid}", async (Guid id, Request request, ISender sender) =>
            {
                Result result = await sender.Send(new UpdateCategoryCommand(id, request.Name));
                return result.Match(Results.NoContent, ApiResults.Problem);
            })
            .WithTags(Tags.Categories)
            .WithName("Update Category")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithSummary("Updates a category's name")
            .WithDescription("Updates the name of a category identified by its unique ID. The new name must be unique and non-empty.");
    }
    
    private sealed record Request(string Name);
}
