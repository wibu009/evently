using Evently.Common.Application.Messaging;
using Evently.Common.Domain;
using Evently.Modules.Ticketing.Application.Abstractions.Data;
using Evently.Modules.Ticketing.Domain.Orders;
using Evently.Modules.Ticketing.Domain.Tickets;

namespace Evently.Modules.Ticketing.Application.Tickets.ArchiveOrderTickets;

/// <summary>
/// Invalidates all tickets that were issued for the given order, for example when the order
/// is refunded. Archived tickets can no longer be checked in.
/// </summary>
internal sealed class ArchiveOrderTicketsCommandHandler(
    IOrderRepository orderRepository,
    ITicketRepository ticketRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<ArchiveOrderTicketsCommand>
{
    public async Task<Result> Handle(ArchiveOrderTicketsCommand request, CancellationToken cancellationToken)
    {
        Order? order = await orderRepository.GetAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            return Result.Failure(OrderErrors.NotFound(request.OrderId));
        }

        IEnumerable<Ticket> tickets = await ticketRepository.GetForOrderAsync(order.Id, cancellationToken);
        foreach (Ticket ticket in tickets)
        {
            ticket.Archive();
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
