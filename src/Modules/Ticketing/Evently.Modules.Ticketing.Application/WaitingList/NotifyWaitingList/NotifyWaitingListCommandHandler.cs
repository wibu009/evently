using Evently.Common.Application.Messaging;
using Evently.Common.Domain;
using Evently.Modules.Ticketing.Application.Abstractions.Data;
using Evently.Modules.Ticketing.Application.Abstractions.Notifications;
using Evently.Modules.Ticketing.Domain.Customers;
using Evently.Modules.Ticketing.Domain.WaitingList;
using Microsoft.Extensions.Logging;

namespace Evently.Modules.Ticketing.Application.WaitingList.NotifyWaitingList;

/// <summary>
/// Notifies the earliest waiting customers (FIFO) that inventory became available for the
/// ticket type. Executed through the outbox whenever a ticket type is restocked by the
/// fulfillment saga (canceled, expired, or refunded orders).
/// The command is idempotent: entries that were already notified are skipped.
/// </summary>
internal sealed class NotifyWaitingListCommandHandler(
    IWaitingListRepository waitingListRepository,
    ICustomerRepository customerRepository,
    INotificationService notificationService,
    IUnitOfWork unitOfWork,
    ILogger<NotifyWaitingListCommandHandler> logger)
    : ICommandHandler<NotifyWaitingListCommand>
{
    public async Task<Result> Handle(NotifyWaitingListCommand request, CancellationToken cancellationToken)
    {
        IReadOnlyList<WaitingListEntry> entries = await waitingListRepository.GetWaitingForTicketTypeAsync(
            request.TicketTypeId,
            (int)request.Quantity,
            cancellationToken);

        if (entries.Count == 0)
        {
            return Result.Success();
        }

        foreach (WaitingListEntry entry in entries)
        {
            Customer? customer = await customerRepository.GetAsync(entry.CustomerId, cancellationToken);
            if (customer is null)
            {
                logger.LogWarning(
                    "The customer {CustomerId} on the waiting list of ticket type {TicketTypeId} no longer exists and was skipped",
                    entry.CustomerId,
                    request.TicketTypeId);

                continue;
            }

            Result result = entry.MarkNotified();
            if (result.IsFailure)
            {
                continue;
            }

            await notificationService.SendWaitingListSpotAvailableAsync(
                new WaitingListNotificationRequest(entry.TicketTypeId, customer.Id, customer.Email),
                cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
