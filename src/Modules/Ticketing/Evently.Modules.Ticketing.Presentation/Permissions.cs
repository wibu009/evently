namespace Evently.Modules.Ticketing.Presentation;

internal static class Permissions
{
    internal const string GetCart = "carts:read";
    internal const string AddToCart = "carts:add";
    internal const string RemoveFromCart = "carts:remove";
    internal const string GetOrders = "orders:read";
    internal const string CreateOrder = "orders:create";
    internal const string CancelOrder = "orders:cancel";
    internal const string GetPayments = "payments:read";
    internal const string RefundPayments = "payments:refund";
    internal const string GetTickets = "tickets:read";
    internal const string TransferTicket = "tickets:transfer";
    internal const string GetWaitingList = "waiting-lists:read";
    internal const string JoinWaitingList = "waiting-lists:join";
    internal const string LeaveWaitingList = "waiting-lists:remove";
    internal const string GetPromoCodes = "promo-codes:read";
    internal const string CreatePromoCode = "promo-codes:create";
    internal const string RemovePromoCode = "promo-codes:remove";
}
