using Evently.Common.Domain;
using Evently.Common.Presentation.Endpoints;
using Evently.Common.Presentation.Results;
using Evently.Modules.Attendance.Application.EventStatistics.GetEventStatistics;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Evently.Modules.Attendance.Presentation.EventStatistics;

internal sealed class GetEventStatisticsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("event-statistics/{eventId:guid}", async (Guid eventId, ISender sender) =>
            {
                Result<Application.EventStatistics.EventStatistics> result = await sender.Send(new GetEventStatisticsQuery(eventId));

                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .RequireAuthorization(Permissions.GetEventStatistics)
            .WithTags(Tags.EventStatistics)
            .WithName("Get Event Statistics")
            .Produces<Application.EventStatistics.EventStatistics>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithSummary("Gets statistics for an event")
            .WithDescription("Retrieves statistics for an event.");
    }
}
