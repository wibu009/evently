using Evently.Common.Domain;
using Evently.Modules.Ticketing.Application.Carts;
using Evently.Modules.Ticketing.Application.Carts.AddItemToCart;
using Evently.Modules.Ticketing.Application.Orders.CreateOrder;
using Evently.Modules.Ticketing.Application.Orders.ProcessPayment;
using Evently.Modules.Ticketing.Application.Payments.RefundPayment;
using Evently.Modules.Ticketing.Domain.Orders;
using Evently.Modules.Ticketing.Domain.Payments;
using Evently.Modules.Ticketing.IntegrationTests.Abstractions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Evently.Modules.Ticketing.IntegrationTests.Payments;

public class RefundPaymentTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Should_ReturnFailure_WhenPaymentDoesNotExist()
    {
        //Arrange
        var command = new RefundPaymentCommand(Guid.CreateVersion7(), null);

        //Act
        Result result = await Sender.Send(command);

        //Assert
        result.Error.Should().Be(PaymentErrors.NotFound(command.PaymentId));
    }

    [Fact]
    public async Task Should_ReturnFailure_WhenPaymentWasNeverCharged()
    {
        //Arrange
        await CleanDatabaseAsync();

        Guid customerId = await Sender.CreateCustomerAsync(Guid.CreateVersion7());
        var eventId = Guid.CreateVersion7();
        var ticketTypeId = Guid.CreateVersion7();

        await Sender.CreateEventWithTicketTypeAsync(eventId, ticketTypeId, 5m);

        await Sender.Send(new AddItemToCartCommand(customerId, ticketTypeId, 1m));

        Result<Guid> orderResult = await Sender.Send(new CreateOrderCommand(customerId));

        Guid paymentId = await DbContext.Set<Payment>()
            .Where(p => p.OrderId == orderResult.Value)
            .Select(p => p.Id)
            .SingleAsync(CancellationToken.None);

        //Act — refund a payment that is still pending
        Result result = await Sender.Send(new RefundPaymentCommand(paymentId, null));

        //Assert
        result.Error.Should().Be(PaymentErrors.NotSucceeded);
    }

    [Fact]
    public async Task Should_RefundFullAmount_AndMarkOrderAsRefunded_WhenNoAmountIsProvided()
    {
        //Arrange
        await CleanDatabaseAsync();

        Guid customerId = await Sender.CreateCustomerAsync(Guid.CreateVersion7());
        var eventId = Guid.CreateVersion7();
        var ticketTypeId = Guid.CreateVersion7();

        await Sender.CreateEventWithTicketTypeAsync(eventId, ticketTypeId, 5m);

        await Sender.Send(new AddItemToCartCommand(customerId, ticketTypeId, 1m));

        Result<Guid> orderResult = await Sender.Send(new CreateOrderCommand(customerId));

        Guid paymentId = await DbContext.Set<Payment>()
            .Where(p => p.OrderId == orderResult.Value)
            .Select(p => p.Id)
            .SingleAsync(CancellationToken.None);

        await Sender.Send(new ProcessPaymentCommand(paymentId));

        //Act
        Result result = await Sender.Send(new RefundPaymentCommand(paymentId, null));

        //Assert
        result.IsSuccess.Should().BeTrue();

        Payment? payment = await DbContext.Set<Payment>().SingleOrDefaultAsync(p => p.Id == paymentId, CancellationToken.None);
        payment!.AmountRefunded.Should().Be(payment.Amount);
        payment.RefundedAtUtc.Should().NotBeNull();

        Order? order = await DbContext.Set<Order>().SingleOrDefaultAsync(o => o.Id == orderResult.Value, CancellationToken.None);
        order!.Status.Should().Be(OrderStatus.Refunded);
    }

    [Fact]
    public async Task Should_RefundPartially_AndKeepOrderPaid_WhenAmountIsProvided()
    {
        //Arrange
        await CleanDatabaseAsync();

        Guid customerId = await Sender.CreateCustomerAsync(Guid.CreateVersion7());
        var eventId = Guid.CreateVersion7();
        var ticketTypeId = Guid.CreateVersion7();

        await Sender.CreateEventWithTicketTypeAsync(eventId, ticketTypeId, 5m);

        await Sender.Send(new AddItemToCartCommand(customerId, ticketTypeId, 1m));

        Result<Guid> orderResult = await Sender.Send(new CreateOrderCommand(customerId));

        Guid paymentId = await DbContext.Set<Payment>()
            .Where(p => p.OrderId == orderResult.Value)
            .Select(p => p.Id)
            .SingleAsync(CancellationToken.None);

        await Sender.Send(new ProcessPaymentCommand(paymentId));

        decimal paidAmount = await DbContext.Set<Payment>()
            .Where(p => p.Id == paymentId)
            .Select(p => p.Amount)
            .SingleAsync(CancellationToken.None);

        //Act
        Result result = await Sender.Send(new RefundPaymentCommand(paymentId, paidAmount / 2));

        //Assert
        result.IsSuccess.Should().BeTrue();

        Payment? payment = await DbContext.Set<Payment>().SingleOrDefaultAsync(p => p.Id == paymentId, CancellationToken.None);
        payment!.AmountRefunded.Should().Be(paidAmount / 2);
        payment.RefundedAtUtc.Should().BeNull();

        Order? order = await DbContext.Set<Order>().SingleOrDefaultAsync(o => o.Id == orderResult.Value, CancellationToken.None);
        order!.Status.Should().Be(OrderStatus.Paid);
    }
}
