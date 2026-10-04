using Evently.Common.Domain;
using Evently.Modules.Ticketing.Domain.Customers;
using Evently.Modules.Ticketing.Domain.Events;
using Evently.Modules.Ticketing.Domain.PromoCodes;

namespace Evently.Modules.Ticketing.Domain.Orders;

public sealed class Order : Entity
{
    private readonly List<OrderItem> _orderItems = [];

    private Order() { }

    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }
    public decimal TotalPrice { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public Guid? PromoCodeId { get; private set; }
    public string Currency { get; private set; }
    public bool TicketsIssued { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    /// <summary>
    /// The deadline until which the reserved ticket inventory is held for this order.
    /// Once the deadline passes without a successful payment, the order expires
    /// and the reserved quantity is released back into inventory.
    /// </summary>
    public DateTime? PaymentDueUtc { get; private set; }

    public string? CancellationReason { get; private set; }

    public IReadOnlyCollection<OrderItem> OrderItems => [.. _orderItems];

    public static Order Create(Customer customer, DateTime? paymentDueUtc = null)
    {
        var order = new Order
        {
            Id = Guid.CreateVersion7(),
            CustomerId = customer.Id,
            Status = OrderStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow,
            PaymentDueUtc = paymentDueUtc
        };

        order.RaiseDomainEvent(new OrderCreatedDomainEvent(order.Id));

        return order;
    }

    public void AddItem(TicketType ticketType, decimal quantity, decimal price, string currency)
    {
        var orderItem = OrderItem.Create(Id, ticketType.Id, quantity, price, currency);
        _orderItems.Add(orderItem);

        TotalPrice = _orderItems.Sum(o => o.Price);
        Currency = currency;
    }

    /// <summary>
    /// The net amount that has to be paid after the applied promo code discount.
    /// </summary>
    public decimal NetPrice => decimal.Max(decimal.Zero, TotalPrice - DiscountAmount);

    /// <summary>
    /// Applies the discount of a promo code to the order.
    /// The discount is calculated by the promo code aggregate and passed in by the handler,
    /// which also validated that the code applies to the order currency.
    /// </summary>
    public Result ApplyPromoCode(Guid promoCodeId, decimal discountAmount)
    {
        if (Status != OrderStatus.Pending)
        {
            return Result.Failure(OrderErrors.NotPending);
        }

        if (discountAmount < decimal.Zero || discountAmount > TotalPrice)
        {
            return Result.Failure(PromoCodeErrors.DiscountExceedsOrderAmount);
        }

        PromoCodeId = promoCodeId;
        DiscountAmount = discountAmount;

        return Result.Success();
    }

    public Result IssueTickets()
    {
        if (TicketsIssued)
        {
            return Result.Failure(OrderErrors.TicketsAlreadyIssues);
        }

        TicketsIssued = true;
        RaiseDomainEvent(new OrderTicketsIssuedDomainEvent(Id));

        return Result.Success();
    }

    /// <summary>
    /// Confirms the order after the payment charge succeeded.
    /// Tickets can only be issued for paid orders.
    /// </summary>
    public Result MarkAsPaid()
    {
        if (Status != OrderStatus.Pending)
        {
            return Result.Failure(OrderErrors.NotPending);
        }

        Status = OrderStatus.Paid;

        RaiseDomainEvent(new OrderPaidDomainEvent(Id));

        return Result.Success();
    }

    /// <summary>
    /// Cancels a pending order that has not been paid yet.
    /// The reserved ticket inventory is released by the corresponding domain event handler.
    /// </summary>
    public Result Cancel(string? reason = null)
    {
        if (Status != OrderStatus.Pending)
        {
            return Result.Failure(OrderErrors.NotPending);
        }

        Status = OrderStatus.Canceled;
        CancellationReason = reason;

        RaiseDomainEvent(new OrderCanceledDomainEvent(Id, reason));

        return Result.Success();
    }

    /// <summary>
    /// Expires a pending order whose payment deadline has passed.
    /// This is the timeout of the order fulfillment saga and releases the reserved inventory.
    /// </summary>
    public Result Expire(string? reason = null)
    {
        if (Status != OrderStatus.Pending)
        {
            return Result.Failure(OrderErrors.NotPending);
        }

        Status = OrderStatus.Expired;
        CancellationReason = reason;

        RaiseDomainEvent(new OrderExpiredDomainEvent(Id));

        return Result.Success();
    }

    /// <summary>
    /// Marks the order as refunded once its payment has been fully refunded.
    /// Issued tickets are archived and the inventory is released by the corresponding domain event handlers.
    /// </summary>
    public Result Refund()
    {
        if (Status != OrderStatus.Paid)
        {
            return Result.Failure(OrderErrors.NotPaid);
        }

        Status = OrderStatus.Refunded;

        RaiseDomainEvent(new OrderRefundedDomainEvent(Id));

        return Result.Success();
    }
}
