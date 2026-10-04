using Evently.Common.Application.Messaging;
using Evently.Modules.Ticketing.Domain.Events;
using MediatR;

namespace Evently.Modules.Ticketing.Application.WaitingList.NotifyWaitingList;

/// <summary>
/// Whenever the fulfillment saga releases inventory back into a ticket type,
/// the waiting list is served in FIFO order.
/// </summary>
internal sealed class NotifyWaitingListOnTicketTypeRestockedDomainEventHandler(ISender sender)
    : DomainEventHandler<TicketTypeRestockedDomainEvent>
{
    public override async Task Handle(TicketTypeRestockedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await sender.Send(new NotifyWaitingListCommand(domainEvent.TicketTypeId, domainEvent.Quantity), cancellationToken);
    }
}
