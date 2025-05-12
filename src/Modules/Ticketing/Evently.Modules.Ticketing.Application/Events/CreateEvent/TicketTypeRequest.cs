namespace Evently.Modules.Ticketing.Application.Events.CreateEvent;

public sealed record TicketTypeRequest(
    Guid TicketTypeId,
    Guid EventId,
    string Name,
    decimal Price,
    string Currency,
    decimal Quantity);