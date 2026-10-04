using Evently.Common.Application.Messaging;
using Evently.Common.Domain;
using Evently.Modules.Ticketing.Application.Abstractions.Data;
using Evently.Modules.Ticketing.Domain.Customers;
using Evently.Modules.Ticketing.Domain.Tickets;

namespace Evently.Modules.Ticketing.Application.Tickets.TransferTicket;

/// <summary>
/// Transfers a ticket from its current owner to another customer. The attendance module
/// is kept in sync through the TicketTransferredIntegrationEvent, so the new owner can
/// check in with the ticket while the previous owner no longer can.
/// </summary>
internal sealed class TransferTicketCommandHandler(
    ITicketRepository ticketRepository,
    ICustomerRepository customerRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<TransferTicketCommand>
{
    public async Task<Result> Handle(TransferTicketCommand request, CancellationToken cancellationToken)
    {
        Ticket? ticket = await ticketRepository.GetAsync(request.TicketId, cancellationToken);
        if (ticket is null)
        {
            return Result.Failure(TicketErrors.NotFound(request.TicketId));
        }

        Customer? fromCustomer = await customerRepository.GetAsync(ticket.CustomerId, cancellationToken);
        if (fromCustomer is null)
        {
            return Result.Failure(CustomerErrors.NotFound(ticket.CustomerId));
        }

        Customer? toCustomer = await customerRepository.GetAsync(request.ToCustomerId, cancellationToken);
        if (toCustomer is null)
        {
            return Result.Failure(CustomerErrors.NotFound(request.ToCustomerId));
        }

        Result result = ticket.Transfer(fromCustomer, toCustomer);
        if (result.IsFailure)
        {
            return Result.Failure(result.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
