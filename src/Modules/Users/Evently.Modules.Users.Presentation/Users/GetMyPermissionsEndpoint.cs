using Evently.Common.Application.Authentication;
using Evently.Common.Application.Authorization;
using Evently.Common.Domain;
using Evently.Common.Presentation.Endpoints;
using Evently.Common.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Evently.Modules.Users.Presentation.Users;

/// <summary>
/// Returns the permissions of the authenticated user. Permission claims are resolved
/// server-side on every request and are not embedded in the access token, so the
/// web UI fetches them once after sign-in to drive permission-gated navigation.
/// </summary>
internal sealed class GetMyPermissionsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("users/my-permissions", async (ICurrentActor actor, IPermissionService permissionService) =>
            {
                Result<PermissionsResponse> result = await permissionService.GetUserPermissionsAsync(actor.IdentityId);

                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .RequireAuthorization()
            .WithTags(Tags.Users)
            .WithName("Get My Permissions")
            .Produces<PermissionsResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithSummary("Retrieves the permissions of the current user")
            .WithDescription("Fetches the set of permission codes granted to the authenticated user via their roles.");
    }
}
