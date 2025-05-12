namespace Evently.Modules.Ticketing.Application.Orders.GetOrder;

public sealed record OrderItemResponse(
    Guid OrderItemId,
    Guid OrderId,
    Guid TicketTypeId,
    decimal Quantity,
    decimal UnitPrice,
    string Price,
    string Currency);