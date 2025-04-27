using Evently.Modules.Events.Application.TicketTypes.GetTicketType;
using Evently.Modules.Events.Application.TicketTypes.GetTicketTypes;
using Evently.Modules.Events.Domain.Abstractions;
using Evently.Modules.Events.Presentation.ApiResults;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Evently.Modules.Events.Presentation.TicketTypes;

internal static class GetTicketTypes
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("ticket-types", async (ISender sender) =>
        {
            Result<IReadOnlyCollection<TicketTypeResponse>> result = await sender.Send(new GetTicketTypesQuery());
            
            return result.Match(Results.Ok, ApiResults.ApiResults.Problem);
        })
        .WithTags(Tags.TicketTypes);
    }
}
