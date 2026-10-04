namespace Evently.Modules.Ticketing.Application.Orders.GetOrders;

public sealed record GetOrdersResponse(
    int Page,
    int PageSize,
    int TotalCount,
    IReadOnlyList<OrderResponse> Orders);
