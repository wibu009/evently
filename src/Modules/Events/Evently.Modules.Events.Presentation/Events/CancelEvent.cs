using Evently.Modules.Events.Application.Events.CancelEvent;
using Evently.Modules.Events.Domain.Abstractions;
using Evently.Modules.Events.Presentation.ApiResults;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Evently.Modules.Events.Presentation.Events;

internal static class CancelEvent
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("events/{id:guid}/cancel", async (Guid id, ISender sender) =>
        {
            Result result = await sender.Send(new CancelEventCommand(id));
            
            return result.Match(Results.NoContent, ApiResults.ApiResults.Problem);
        })
        .WithTags(Tags.Events);
    }
}
