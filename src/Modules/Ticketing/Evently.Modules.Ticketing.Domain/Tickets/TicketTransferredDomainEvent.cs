using Evently.Common.Domain;

namespace Evently.Modules.Ticketing.Domain.Tickets;

public sealed class TicketTransferredDomainEvent(Guid ticketId, string code, Guid fromCustomerId, Guid toCustomerId) : DomainEvent
{
    public Guid TicketId { get; init; } = ticketId;

    public string Code { get; init; } = code;

    public Guid FromCustomerId { get; init; } = fromCustomerId;

    public Guid ToCustomerId { get; init; } = toCustomerId;
}
