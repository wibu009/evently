using Evently.Common.Domain;
using Evently.Common.Presentation.Endpoints;
using Evently.Common.Presentation.Results;
using Evently.Modules.Users.Application.Users.UpdateUser;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Evently.Modules.Users.Presentation.Users;

internal sealed class UpdateUserProfileEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("users/{id}/profile", async (Guid id, Request request, ISender sender) =>
            {
                Result result = await sender.Send(new UpdateUserCommand(
                    id,
                    request.FirstName,
                    request.LastName));
                return result.Match(Results.NoContent, ApiResults.Problem);
            })
            .WithTags(Tags.Users)
            .WithName("Update User Profile")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithSummary("Updates a user's profile")
            .WithDescription("Updates the first name and last name of a user identified by their unique GUID. The operation is idempotent.");
    }
    
    private sealed record Request(string FirstName, string LastName);
}
