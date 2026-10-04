using Evently.Common.Application.EventBus;
using Evently.Common.Application.Exceptions;
using Evently.Common.Domain;
using Evently.Modules.Attendance.Application.Tickets.UpdateTicketOwner;
using Evently.Modules.Ticketing.IntegrationEvents;
using Evently.Modules.Ticketing.IntegrationEvents.Tickets;
using MediatR;

namespace Evently.Modules.Attendance.Presentation.Tickets;

/// <summary>
/// Moves the ticket to the new owner after a ticket transfer, so that check-in
/// validates the ticket against the new attendee.
/// </summary>
internal sealed class TicketTransferredIntegrationEventHandler(ISender sender)
    : IntegrationEventHandler<TicketTransferredIntegrationEvent>
{
    public override async Task Handle(
        TicketTransferredIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        Result result = await sender.Send(
            new UpdateTicketOwnerCommand(
                integrationEvent.TicketId,
                integrationEvent.ToCustomerId),
            cancellationToken);

        if (result.IsFailure)
        {
            throw new EventlyException(nameof(UpdateTicketOwnerCommand), result.Error);
        }
    }
}
