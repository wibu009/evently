namespace Evently.Modules.Ticketing.Application.Orders;

/// <summary>
/// Configures the checkout and order fulfillment saga behavior.
/// Bound from the <c>Ticketing:Orders</c> configuration section.
/// </summary>
public sealed class OrdersOptions
{
    /// <summary>
    /// The number of minutes a pending order holds the reserved ticket inventory
    /// before it expires and the inventory is released.
    /// </summary>
    public int PaymentTimeoutMinutes { get; init; } = 15;

    /// <summary>
    /// The interval, in seconds, at which the order expiration job runs.
    /// </summary>
    public int ExpirationIntervalInSeconds { get; init; } = 60;

    /// <summary>
    /// The maximum number of stale orders processed per expiration job run.
    /// </summary>
    public int ExpirationBatchSize { get; init; } = 100;
}
