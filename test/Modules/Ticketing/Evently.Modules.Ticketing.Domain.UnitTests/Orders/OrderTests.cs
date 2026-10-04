using Evently.Common.Domain;
using Evently.Modules.Ticketing.Domain.Customers;
using Evently.Modules.Ticketing.Domain.Events;
using Evently.Modules.Ticketing.Domain.Orders;
using Evently.Modules.Ticketing.Domain.PromoCodes;
using Evently.Modules.Ticketing.Domain.UnitTests.Abstractions;
using FluentAssertions;

namespace Evently.Modules.Ticketing.Domain.UnitTests.Orders;

public class OrderTests : BaseTest
{
    private static Customer CreateCustomer()
    {
        return Customer.Create(
            Guid.CreateVersion7(),
            Faker.Internet.Email(),
            Faker.Name.FirstName(),
            Faker.Name.LastName());
    }

    private static (TicketType TicketType, Order Order) CreateOrderWithItem(decimal quantity = 2)
    {
        Customer customer = CreateCustomer();

        var ticketType = TicketType.Create(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Faker.Music.Genre(),
            100m,
            "USD",
            quantity * 10);

        var order = Order.Create(customer, DateTime.UtcNow.AddMinutes(15));
        order.AddItem(ticketType, quantity, ticketType.Price, ticketType.Currency);

        return (ticketType, order);
    }

    [Fact]
    public void Create_ShouldRaiseDomainEvent_WhenOrderIsCreated()
    {
        //Arrange
        Customer customer = CreateCustomer();

        //Act
        var result = Order.Create(customer);

        //Assert
        OrderCreatedDomainEvent domainEvent =
            AssertDomainEventWasPublished<OrderCreatedDomainEvent>(result);

        domainEvent.OrderId.Should().Be(result.Id);
    }

    [Fact]
    public void Create_ShouldStartInPendingStatus_WithPaymentDeadline()
    {
        // Arrange
        Customer customer = CreateCustomer();
        DateTime paymentDueUtc = DateTime.UtcNow.AddMinutes(15);

        // Act
        var order = Order.Create(customer, paymentDueUtc);

        // Assert
        order.Status.Should().Be(OrderStatus.Pending);
        order.PaymentDueUtc.Should().Be(paymentDueUtc);
        order.TicketsIssued.Should().BeFalse();
    }

    [Fact]
    public void IssueTicket_ShouldReturnFailure_WhenTicketAlreadyIssued()
    {
        //Arrange
        (_, Order order) = CreateOrderWithItem();

        order.MarkAsPaid();
        order.IssueTickets();

        //Act
        Result issueTicketsResult = order.IssueTickets();

        //Assert
        issueTicketsResult.Error.Should().Be(OrderErrors.TicketsAlreadyIssues);
    }

    [Fact]
    public void IssueTicket_ShouldRaiseDomainEvent_WhenTicketIsIssued()
    {
        //Arrange
        (_, Order order) = CreateOrderWithItem();

        order.MarkAsPaid();

        //Act
        Result result = order.IssueTickets();

        //Assert
        result.IsSuccess.Should().BeTrue();

        OrderTicketsIssuedDomainEvent domainEvent =
            AssertDomainEventWasPublished<OrderTicketsIssuedDomainEvent>(order);

        domainEvent.OrderId.Should().Be(order.Id);
    }

    [Fact]
    public void MarkAsPaid_ShouldRaiseDomainEvent_WhenOrderIsPending()
    {
        // Arrange
        (_, Order order) = CreateOrderWithItem();

        // Act
        Result result = order.MarkAsPaid();

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Paid);

        AssertDomainEventWasPublished<OrderPaidDomainEvent>(order);
    }

    [Fact]
    public void MarkAsPaid_ShouldReturnFailure_WhenOrderIsNotPending()
    {
        // Arrange
        (_, Order order) = CreateOrderWithItem();

        order.MarkAsPaid();

        // Act
        Result result = order.MarkAsPaid();

        // Assert
        result.Error.Should().Be(OrderErrors.NotPending);
    }

    [Fact]
    public void Cancel_ShouldRaiseDomainEvent_WhenOrderIsPending()
    {
        // Arrange
        (_, Order order) = CreateOrderWithItem();

        // Act
        Result result = order.Cancel("Customer changed their mind");

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Canceled);
        order.CancellationReason.Should().Be("Customer changed their mind");

        OrderCanceledDomainEvent domainEvent =
            AssertDomainEventWasPublished<OrderCanceledDomainEvent>(order);

        domainEvent.OrderId.Should().Be(order.Id);
        domainEvent.Reason.Should().Be("Customer changed their mind");
    }

    [Fact]
    public void Cancel_ShouldReturnFailure_WhenOrderIsNotPending()
    {
        // Arrange
        (_, Order order) = CreateOrderWithItem();

        order.MarkAsPaid();

        // Act
        Result result = order.Cancel();

        // Assert
        result.Error.Should().Be(OrderErrors.NotPending);
    }

    [Fact]
    public void Expire_ShouldRaiseDomainEvent_WhenOrderIsPending()
    {
        // Arrange
        (_, Order order) = CreateOrderWithItem();

        // Act
        Result result = order.Expire("The payment deadline passed");

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Expired);

        AssertDomainEventWasPublished<OrderExpiredDomainEvent>(order);
    }

    [Fact]
    public void Expire_ShouldReturnFailure_WhenOrderIsNotPending()
    {
        // Arrange
        (_, Order order) = CreateOrderWithItem();

        order.MarkAsPaid();

        // Act
        Result result = order.Expire();

        // Assert
        result.Error.Should().Be(OrderErrors.NotPending);
    }

    [Fact]
    public void Refund_ShouldRaiseDomainEvent_WhenOrderIsPaid()
    {
        // Arrange
        (_, Order order) = CreateOrderWithItem();

        order.MarkAsPaid();

        // Act
        Result result = order.Refund();

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Refunded);

        AssertDomainEventWasPublished<OrderRefundedDomainEvent>(order);
    }

    [Fact]
    public void Refund_ShouldReturnFailure_WhenOrderIsNotPaid()
    {
        // Arrange
        (_, Order order) = CreateOrderWithItem();

        // Act
        Result result = order.Refund();

        // Assert
        result.Error.Should().Be(OrderErrors.NotPaid);
    }

    [Fact]
    public void ApplyPromoCode_ShouldSetDiscount_WhenOrderIsPending()
    {
        // Arrange
        (_, Order order) = CreateOrderWithItem();

        var promoCodeId = Guid.CreateVersion7();

        // Act
        Result result = order.ApplyPromoCode(promoCodeId, 15m);

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.PromoCodeId.Should().Be(promoCodeId);
        order.DiscountAmount.Should().Be(15m);
        order.NetPrice.Should().Be(order.TotalPrice - 15m);
    }

    [Fact]
    public void ApplyPromoCode_ShouldReturnFailure_WhenDiscountExceedsTheOrderAmount()
    {
        // Arrange
        (_, Order order) = CreateOrderWithItem();

        // Act
        Result result = order.ApplyPromoCode(Guid.CreateVersion7(), order.TotalPrice + 1m);

        // Assert
        result.Error.Should().Be(PromoCodeErrors.DiscountExceedsOrderAmount);
    }
}
