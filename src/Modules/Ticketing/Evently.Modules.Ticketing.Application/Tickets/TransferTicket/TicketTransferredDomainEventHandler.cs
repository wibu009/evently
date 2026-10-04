using Evently.Common.Application.EventBus;
using Evently.Common.Application.Messaging;
using Evently.Modules.Ticketing.Domain.Tickets;
using Evently.Modules.Ticketing.IntegrationEvents.Tickets;

namespace Evently.Modules.Ticketing.Application.Tickets.TransferTicket;

internal sealed class TicketTransferredDomainEventHandler(IEventBus eventBus)
    : DomainEventHandler<TicketTransferredDomainEvent>
{
    public override async Task Handle(TicketTransferredDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await eventBus.PublishAsync(
            new TicketTransferredIntegrationEvent(
                domainEvent.Id,
                domainEvent.OccurredOnUtc,
                domainEvent.TicketId,
                domainEvent.Code,
                domainEvent.FromCustomerId,
                domainEvent.ToCustomerId),
            cancellationToken);
    }
}
