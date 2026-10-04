using Evently.Modules.Ticketing.Application.Abstractions.Notifications;
using Microsoft.Extensions.Logging;

namespace Evently.Modules.Ticketing.Infrastructure.Notifications;

/// <summary>
/// The default notification sender. It writes a structured log entry for every notification,
/// which keeps the transactional notification flow observable in Seq without an external provider.
/// Swap this implementation (or decorate it) with a real email/SMS provider when going to production.
/// </summary>
internal sealed class NotificationService(ILogger<NotificationService> logger) : INotificationService
{
    public Task SendOrderConfirmationAsync(OrderNotificationRequest request, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Notification {NotificationType} sent to customer {CustomerId} for order {OrderId}: your order totaling {Amount} {Currency} has been paid and your tickets are on the way",
            nameof(SendOrderConfirmationAsync),
            request.CustomerId,
            request.OrderId,
            request.TotalPrice,
            request.Currency);

        return Task.CompletedTask;
    }

    public Task SendOrderCancellationAsync(OrderNotificationRequest request, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Notification {NotificationType} sent to customer {CustomerId} for order {OrderId}: your order has been canceled. Reason: {Reason}. Reserved tickets have been released",
            nameof(SendOrderCancellationAsync),
            request.CustomerId,
            request.OrderId,
            request.Reason);

        return Task.CompletedTask;
    }

    public Task SendOrderExpirationAsync(OrderNotificationRequest request, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Notification {NotificationType} sent to customer {CustomerId} for order {OrderId}: your order expired because the payment deadline passed. Reason: {Reason}",
            nameof(SendOrderExpirationAsync),
            request.CustomerId,
            request.OrderId,
            request.Reason);

        return Task.CompletedTask;
    }

    public Task SendOrderRefundAsync(OrderNotificationRequest request, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Notification {NotificationType} sent to customer {CustomerId} for order {OrderId}: a refund of {Amount} {Currency} has been issued. Reason: {Reason}",
            nameof(SendOrderRefundAsync),
            request.CustomerId,
            request.OrderId,
            request.TotalPrice,
            request.Currency,
            request.Reason);

        return Task.CompletedTask;
    }

    public Task SendTicketIssuedAsync(TicketNotificationRequest request, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Notification {NotificationType} sent to customer {CustomerId} for ticket {TicketId}: your ticket with code {TicketCode} is ready",
            nameof(SendTicketIssuedAsync),
            request.CustomerId,
            request.TicketId,
            request.TicketCode);

        return Task.CompletedTask;
    }

    public Task SendWaitingListSpotAvailableAsync(WaitingListNotificationRequest request, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Notification {NotificationType} sent to customer {CustomerId} for ticket type {TicketTypeId}: tickets became available and you are next in line",
            nameof(SendWaitingListSpotAvailableAsync),
            request.CustomerId,
            request.TicketTypeId);

        return Task.CompletedTask;
    }
}
