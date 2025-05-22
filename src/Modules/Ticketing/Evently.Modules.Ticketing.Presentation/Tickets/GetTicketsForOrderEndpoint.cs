using Evently.Common.Domain;
using Evently.Common.Presentation.Endpoints;
using Evently.Common.Presentation.Results;
using Evently.Modules.Ticketing.Application.Tickets.GetTicket;
using Evently.Modules.Ticketing.Application.Tickets.GetTicketsForOrder;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Evently.Modules.Ticketing.Presentation.Tickets;

internal sealed class GetTicketsForOrderEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("tickets/order/{orderId:guid}", async (Guid orderId, ISender sender) =>
            {
                Result<IReadOnlyList<TicketResponse>> result = await sender.Send(
                    new GetTicketsForOrderQuery(orderId));

                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .RequireAuthorization(Permissions.GetTickets)
            .WithTags(Tags.Tickets)
            .WithName("Get Tickets For Order")
            .Produces<IReadOnlyList<TicketResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithSummary("Retrieves all tickets for a specific order")
            .WithDescription("Fetches a list of tickets associated with an order identified by its GUID. Returns the tickets if found, or an error if the order does not exist or has no tickets.");
    }
}
