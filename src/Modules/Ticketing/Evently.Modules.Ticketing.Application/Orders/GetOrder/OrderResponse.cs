namespace Evently.Modules.Ticketing.Application.Orders.GetOrder;

public sealed record OrderResponse(
    Guid Id,
    Guid CustomerId,
    string Status,
    decimal TotalPrice,
    string Currency,
    DateTime CreatedAtUtc)
{
    public List<OrderItemResponse> OrderItems { get; set; } = [];
}