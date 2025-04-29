using Evently.Common.Domain;
using Evently.Common.Presentation.Endpoints;
using Evently.Common.Presentation.Results;
using Evently.Modules.Events.Application.Categories.CreateCategory;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Evently.Modules.Events.Presentation.Categories;

internal sealed class CreateCategory : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("categories", async (Request request, ISender sender) =>
            {
                Result<Guid> result = await sender.Send(new CreateCategoryCommand(request.Name));
                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .WithTags(Tags.Categories)
            .WithName("Create Category")
            .Produces<Guid>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithSummary("Creates a new category")
            .WithDescription("Creates a category with the provided name and returns its unique identifier. The name must be unique and non-empty.");
    }

    private sealed record Request(string Name);
}
