namespace Evently.Modules.Ticketing.Application.Abstractions.Notifications;

public sealed record OrderNotificationRequest(
    Guid OrderId,
    Guid CustomerId,
    string CustomerEmail,
    decimal TotalPrice,
    string Currency,
    string? Reason = null);

public sealed record TicketNotificationRequest(
    Guid OrderId,
    Guid TicketId,
    Guid CustomerId,
    string CustomerEmail,
    string TicketCode);

public sealed record WaitingListNotificationRequest(
    Guid TicketTypeId,
    Guid CustomerId,
    string CustomerEmail);

/// <summary>
/// Sends transactional notifications (order confirmations, cancellations, refunds, ticket deliveries,
/// waiting list availability) to customers. The default implementation logs the notifications; replace
/// or decorate it with a real email/SMS provider integration when moving to production.
/// </summary>
public interface INotificationService
{
    Task SendOrderConfirmationAsync(OrderNotificationRequest request, CancellationToken cancellationToken = default);

    Task SendOrderCancellationAsync(OrderNotificationRequest request, CancellationToken cancellationToken = default);

    Task SendOrderExpirationAsync(OrderNotificationRequest request, CancellationToken cancellationToken = default);

    Task SendOrderRefundAsync(OrderNotificationRequest request, CancellationToken cancellationToken = default);

    Task SendTicketIssuedAsync(TicketNotificationRequest request, CancellationToken cancellationToken = default);

    Task SendWaitingListSpotAvailableAsync(WaitingListNotificationRequest request, CancellationToken cancellationToken = default);
}
