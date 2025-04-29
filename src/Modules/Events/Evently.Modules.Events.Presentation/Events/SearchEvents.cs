using Evently.Common.Domain;
using Evently.Common.Presentation.Endpoints;
using Evently.Common.Presentation.Results;
using Evently.Modules.Events.Application.Events.SearchEvents;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Evently.Modules.Events.Presentation.Events;

internal sealed class SearchEvents : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("events/search", async (
                ISender sender, 
                Guid? categoryId,
                DateTime? startDate,
                DateTime? endDate,
                int page = 1,
                int pageSize = 10) =>
            {
                Result<SearchEventsResponse> result = await sender.Send(
                    new SearchEventsQuery(categoryId, startDate, endDate, page, pageSize));
                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .WithTags(Tags.Events)
            .WithName("Search Events")
            .Produces<SearchEventsResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithSummary("Searches for events")
            .WithDescription("Searches for events based on optional category ID, date range, and pagination parameters. Returns a paginated list of matching events.");
    }
}
