using Evently.Common.Domain;
using Evently.Common.Presentation.Endpoints;
using Evently.Common.Presentation.Results;
using Evently.Modules.Ticketing.Application.Tickets.TransferTicket;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Evently.Modules.Ticketing.Presentation.Tickets;

internal sealed class TransferTicketEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("tickets/{id:guid}/transfer", async (Guid id, Request request, ISender sender) =>
            {
                Result result = await sender.Send(new TransferTicketCommand(id, request.ToCustomerId));

                return result.Match(Results.NoContent, ApiResults.Problem);
            })
            .RequireAuthorization(Permissions.TransferTicket)
            .WithTags(Tags.Tickets)
            .WithName("Transfer Ticket")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithSummary("Transfers a ticket to another customer")
            .WithDescription("Hands the ticket over to another customer. The previous owner loses access, the new owner can check in with the ticket.");
    }

    internal sealed record Request(Guid ToCustomerId);
}
