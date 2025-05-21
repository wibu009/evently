using Evently.Common.Domain;
using Evently.Common.Presentation.Endpoints;
using Evently.Common.Presentation.Results;
using Evently.Modules.Ticketing.Application.Tickets.GetTicket;
using Evently.Modules.Ticketing.Application.Tickets.GetTicketByCode;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Evently.Modules.Ticketing.Presentation.Tickets;

internal sealed class GetTicketByCodeEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("tickets/code/{code}", async (string code, ISender sender) =>
            {
                Result<TicketResponse> result = await sender.Send(new GetTicketByCodeQuery(code));

                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .RequireAuthorization(Permissions.GetTickets)
            .WithTags(Tags.Tickets)
            .WithName("Get Ticket By Code")
            .Produces<TicketResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithSummary("Retrieves a ticket by its unique code")
            .WithDescription("Fetches the details of a ticket identified by its unique code. Returns the ticket if found, or an error if no ticket matches the code.");
    }
}
