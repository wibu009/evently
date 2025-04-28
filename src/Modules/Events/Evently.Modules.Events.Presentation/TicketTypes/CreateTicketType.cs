using Evently.Common.Domain;
using Evently.Common.Presentation.ApiResults;
using Evently.Modules.Events.Application.TicketTypes.CreateTicketType;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Evently.Modules.Events.Presentation.TicketTypes;

internal static class CreateTicketType
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("ticket-types", async (Request request, ISender sender) =>
        {
            Result<Guid> result = await sender.Send(new CreateTicketTypeCommand(
                request.EventId,
                request.Name,
                request.Price,
                request.Currency,
                request.Quantity));
            
            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .WithTags(Tags.TicketTypes);
    }

    private sealed record Request(
        Guid EventId,
        string Name,
        decimal Price,
        string Currency,
        decimal Quantity);
}
