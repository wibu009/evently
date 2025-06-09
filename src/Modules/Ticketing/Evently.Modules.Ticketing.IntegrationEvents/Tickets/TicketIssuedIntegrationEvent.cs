using Evently.Common.Application.EventBus;

namespace Evently.Modules.Ticketing.IntegrationEvents.Tickets;

public sealed class TicketIssuedIntegrationEvent(
    Guid id,
    DateTime occurredOnUtc,
    Guid ticketId,
    Guid customerId,
    Guid eventId,
    string code)
    : IntegrationEvent(id, occurredOnUtc)
{
    public Guid TicketId { get; init; } = ticketId;
    public Guid CustomerId { get; init; } = customerId;
    public Guid EventId { get; init; } = eventId;
    public string Code { get; init; } = code;
}
