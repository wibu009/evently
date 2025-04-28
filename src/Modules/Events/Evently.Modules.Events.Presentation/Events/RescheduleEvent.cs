using Evently.Common.Domain;
using Evently.Common.Presentation.ApiResults;
using Evently.Modules.Events.Application.Events.RescheduleEvent;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Evently.Modules.Events.Presentation.Events;

internal static class RescheduleEvent
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("events/{id:guid}/reschedule", async (Guid id, Request request, ISender sender) =>
        {
            Result result = await sender.Send(new RescheduleEventCommand(id, request.StartAtUtc, request.EndAtUtc));
            
            return result.Match(Results.NoContent, ApiResults.Problem);
        })
        .WithTags(Tags.Events);
    }
    
    private sealed record Request(DateTime StartAtUtc, DateTime? EndAtUtc);
}
