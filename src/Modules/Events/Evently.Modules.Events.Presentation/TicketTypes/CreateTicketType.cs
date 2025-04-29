using Evently.Common.Domain;
using Evently.Common.Presentation.Endpoints;
using Evently.Common.Presentation.Results;
using Evently.Modules.Events.Application.TicketTypes.CreateTicketType;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Evently.Modules.Events.Presentation.TicketTypes;

internal sealed class CreateTicketType : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
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
            .WithTags(Tags.TicketTypes)
            .WithName("Create Ticket Type")
            .Produces<Guid>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithSummary("Creates a new ticket type")
            .WithDescription("Creates a ticket type for an event with the provided details and returns its unique identifier. Requires a valid event ID, non-empty name, valid price, supported currency, and non-negative quantity.");
    }

    private sealed record Request(
        Guid EventId,
        string Name,
        decimal Price,
        string Currency,
        decimal Quantity);
}
