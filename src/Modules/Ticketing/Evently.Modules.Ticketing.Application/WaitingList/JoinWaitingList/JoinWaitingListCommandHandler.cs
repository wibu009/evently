using Evently.Common.Application.Messaging;
using Evently.Common.Domain;
using Evently.Modules.Ticketing.Application.Abstractions.Data;
using Evently.Modules.Ticketing.Domain.Customers;
using Evently.Modules.Ticketing.Domain.Events;
using Evently.Modules.Ticketing.Domain.WaitingList;

namespace Evently.Modules.Ticketing.Application.WaitingList.JoinWaitingList;

/// <summary>
/// Lets a customer join the waiting list of a sold out ticket type.
/// When inventory becomes available again (cancellations or refunds), the earliest
/// waiting customers are notified in FIFO order.
/// </summary>
internal sealed class JoinWaitingListCommandHandler(
    ICustomerRepository customerRepository,
    ITicketTypeRepository ticketTypeRepository,
    IWaitingListRepository waitingListRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<JoinWaitingListCommand>
{
    public async Task<Result> Handle(JoinWaitingListCommand request, CancellationToken cancellationToken)
    {
        Customer? customer = await customerRepository.GetAsync(request.CustomerId, cancellationToken);
        if (customer is null)
        {
            return Result.Failure(CustomerErrors.NotFound(request.CustomerId));
        }

        TicketType? ticketType = await ticketTypeRepository.GetAsync(request.TicketTypeId, cancellationToken);
        if (ticketType is null)
        {
            return Result.Failure(TicketTypeErrors.NotFound(request.TicketTypeId));
        }

        if (ticketType.AvailableQuantity > decimal.Zero)
        {
            return Result.Failure(WaitingListErrors.NotSoldOut);
        }

        WaitingListEntry? existingEntry = await waitingListRepository.GetAsync(
            request.TicketTypeId,
            request.CustomerId,
            cancellationToken);

        if (existingEntry is not null)
        {
            return Result.Failure(WaitingListErrors.AlreadyJoined);
        }

        Result<WaitingListEntry> result = WaitingListEntry.Create(request.TicketTypeId, request.CustomerId);
        if (result.IsFailure)
        {
            return Result.Failure(result.Error);
        }

        waitingListRepository.Insert(result.Value);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
