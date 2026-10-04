using Evently.Common.Application.EventBus;

namespace Evently.Modules.Ticketing.IntegrationEvents.Tickets;

public sealed class TicketTransferredIntegrationEvent(
    Guid id,
    DateTime occuredOnUtc,
    Guid ticketId,
    string code,
    Guid fromCustomerId,
    Guid toCustomerId)
    : IntegrationEvent(id, occuredOnUtc)
{
    public Guid TicketId { get; init; } = ticketId;

    public string Code { get; init; } = code;

    public Guid FromCustomerId { get; init; } = fromCustomerId;

    public Guid ToCustomerId { get; init; } = toCustomerId;
}
