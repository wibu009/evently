using Evently.Modules.Events.Application.Categories.UpdateCategory;
using Evently.Modules.Events.Domain.Abstractions;
using Evently.Modules.Events.Presentation.ApiResults;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Evently.Modules.Events.Presentation.Categories;

internal static class UpdateCategory
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("categories/{id:guid}", async (Guid id, Request request, ISender sender) =>
        {
            Result result = await sender.Send(new UpdateCategoryCommand(id, request.Name));

            return result.Match(Results.NoContent, ApiResults.ApiResults.Problem);
        })
        .WithTags(Tags.Categories);
    }
    
    private sealed record Request(string Name);
}
