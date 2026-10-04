using Evently.Common.Application.Messaging;
using Evently.Common.Domain;
using Evently.Modules.Ticketing.Application.Abstractions.Data;
using Evently.Modules.Ticketing.Domain.WaitingList;

namespace Evently.Modules.Ticketing.Application.WaitingList.LeaveWaitingList;

internal sealed class LeaveWaitingListCommandHandler(
    IWaitingListRepository waitingListRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<LeaveWaitingListCommand>
{
    public async Task<Result> Handle(LeaveWaitingListCommand request, CancellationToken cancellationToken)
    {
        WaitingListEntry? entry = await waitingListRepository.GetAsync(request.TicketTypeId, request.CustomerId, cancellationToken);
        if (entry is null)
        {
            return Result.Failure(WaitingListErrors.NotJoined);
        }

        waitingListRepository.Remove(entry);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
