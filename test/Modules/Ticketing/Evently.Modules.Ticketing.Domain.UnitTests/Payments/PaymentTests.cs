using Evently.Common.Domain;
using Evently.Modules.Ticketing.Domain.Customers;
using Evently.Modules.Ticketing.Domain.Orders;
using Evently.Modules.Ticketing.Domain.Payments;
using Evently.Modules.Ticketing.Domain.UnitTests.Abstractions;
using FluentAssertions;

namespace Evently.Modules.Ticketing.Domain.UnitTests.Payments;

public class PaymentTests : BaseTest
{
    private static (Customer Customer, Order Order) CreateOrder()
    {
        var customer = Customer.Create(
            Guid.CreateVersion7(),
            Faker.Internet.Email(),
            Faker.Name.FirstName(),
            Faker.Name.LastName());

        var order = Order.Create(customer);

        return (customer, order);
    }

    [Fact]
    public void Create_ShouldRaiseDomainEvent_WhenPaymentIsCreated()
    {
        // Arrange
        (_, Order order) = CreateOrder();

        // Act
        Result<Payment> result = Payment.Create(
            order,
            Faker.Random.Decimal(),
            Faker.Random.String(3));

        // Assert
        PaymentCreatedDomainEvent domainEvent =
            AssertDomainEventWasPublished<PaymentCreatedDomainEvent>(result.Value);

        domainEvent.PaymentId.Should().Be(result.Value.Id);
    }

    [Fact]
    public void Create_ShouldStartInPendingStatus()
    {
        // Arrange
        (_, Order order) = CreateOrder();

        // Act
        Result<Payment> result = Payment.Create(order, 100m, "USD");

        // Assert
        result.Value.Status.Should().Be(PaymentStatus.Pending);
        result.Value.TransactionReference.Should().BeNull();
    }

    [Fact]
    public void Succeed_ShouldSetTransactionIdAndStatus_WhenPaymentIsPending()
    {
        // Arrange
        (_, Order order) = CreateOrder();

        var payment = Payment.Create(order, 100m, "USD");

        const string transactionReference = "pi_test_123";

        // Act
        Result result = payment.Succeed(transactionReference);

        // Assert
        result.IsSuccess.Should().BeTrue();
        payment.Status.Should().Be(PaymentStatus.Succeeded);
        payment.TransactionReference.Should().Be(transactionReference);
        payment.PaidAtUtc.Should().NotBeNull();

        AssertDomainEventWasPublished<PaymentSucceededDomainEvent>(payment);
    }

    [Fact]
    public void Succeed_ShouldReturnFailure_WhenPaymentIsNotPending()
    {
        // Arrange
        (_, Order order) = CreateOrder();

        var payment = Payment.Create(order, 100m, "USD");
        payment.Succeed("pi_test_123");

        // Act
        Result result = payment.Succeed("pi_test_123");

        // Assert
        result.Error.Should().Be(PaymentErrors.NotPending);
    }

    [Fact]
    public void Fail_ShouldSetStatusAndReason_WhenPaymentIsPending()
    {
        // Arrange
        (_, Order order) = CreateOrder();

        var payment = Payment.Create(order, 100m, "USD");

        // Act
        Result result = payment.Fail("The gateway rejected the charge");

        // Assert
        result.IsSuccess.Should().BeTrue();
        payment.Status.Should().Be(PaymentStatus.Failed);
        payment.FailureReason.Should().NotBeNull();

        AssertDomainEventWasPublished<PaymentFailedDomainEvent>(payment);
    }

    [Fact]
    public void Refund_ShouldReturnFailure_WhenPaymentIsNotSucceeded()
    {
        // Arrange
        (_, Order order) = CreateOrder();

        var payment = Payment.Create(order, 100m, "USD");

        // Act
        Result result = payment.Refund(100m);

        // Assert
        result.Error.Should().Be(PaymentErrors.NotSucceeded);
    }

    [Fact]
    public void Refund_ShouldReturnFailure_WhenAlreadyRefunded()
    {
        // Arrange
        decimal amount = Faker.Random.Decimal();

        (_, Order order) = CreateOrder();

        var payment = Payment.Create(order, amount, Faker.Random.String(3));
        payment.Succeed("pi_test_123");

        payment.Refund(amount);

        // Act
        Result result = payment.Refund(amount);

        // Assert
        result.Error.Should().Be(PaymentErrors.AlreadyRefunded);
    }

    [Fact]
    public void Refund_ShouldReturnFailure_WhenRefundAmountExceedsPaidAmount()
    {
        // Arrange
        (_, Order order) = CreateOrder();

        var payment = Payment.Create(order, 100m, "USD");
        payment.Succeed("pi_test_123");

        // Act
        Result result = payment.Refund(150m);

        // Assert
        result.Error.Should().Be(PaymentErrors.NotEnoughFunds);
    }

    [Fact]
    public void Refund_ShouldReturnFailure_WhenRefundAmountIsNotPositive()
    {
        // Arrange
        (_, Order order) = CreateOrder();

        var payment = Payment.Create(order, 100m, "USD");
        payment.Succeed("pi_test_123");

        // Act
        Result result = payment.Refund(decimal.Zero);

        // Assert
        result.Error.Should().Be(PaymentErrors.InvalidRefundAmount);
    }

    [Fact]
    public void Refund_ShouldRaisePartiallyRefundedEvent_WhenRefundAmountIsLessThanPaidAmount()
    {
        // Arrange
        (_, Order order) = CreateOrder();

        var payment = Payment.Create(order, 100m, "USD");
        payment.Succeed("pi_test_123");

        // Act
        Result result = payment.Refund(40m);

        // Assert
        result.IsSuccess.Should().BeTrue();
        payment.AmountRefunded.Should().Be(40m);
        payment.RefundedAtUtc.Should().BeNull();
        payment.IsFullyRefunded.Should().BeFalse();

        AssertDomainEventWasPublished<PaymentPartiallyRefundedDomainEvent>(payment);
    }

    [Fact]
    public void Refund_ShouldRaiseRefundedEvent_WhenPaymentIsFullyRefunded()
    {
        // Arrange
        (_, Order order) = CreateOrder();

        var payment = Payment.Create(order, 100m, "USD");
        payment.Succeed("pi_test_123");

        // Act
        Result result = payment.Refund(100m);

        // Assert
        result.IsSuccess.Should().BeTrue();
        payment.IsFullyRefunded.Should().BeTrue();
        payment.RefundedAtUtc.Should().NotBeNull();

        AssertDomainEventWasPublished<PaymentRefundedDomainEvent>(payment);
    }
}
