using Evently.Common.Domain;
using Evently.Modules.Ticketing.Application.Carts;
using Evently.Modules.Ticketing.Application.Carts.AddItemToCart;
using Evently.Modules.Ticketing.Application.Orders.CreateOrder;
using Evently.Modules.Ticketing.Application.Orders.ProcessPayment;
using Evently.Modules.Ticketing.Domain.Orders;
using Evently.Modules.Ticketing.Domain.Payments;
using Evently.Modules.Ticketing.IntegrationTests.Abstractions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Evently.Modules.Ticketing.IntegrationTests.Orders;

public class ProcessPaymentTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Should_MarkOrderAsPaid_AndPaymentAsSucceeded_WhenPaymentIsProcessed()
    {
        //Arrange
        await CleanDatabaseAsync();

        Guid customerId = await Sender.CreateCustomerAsync(Guid.CreateVersion7());
        var eventId = Guid.CreateVersion7();
        var ticketTypeId = Guid.CreateVersion7();

        await Sender.CreateEventWithTicketTypeAsync(eventId, ticketTypeId, 5m);

        await Sender.Send(new AddItemToCartCommand(customerId, ticketTypeId, 2m));

        Result<Guid> orderResult = await Sender.Send(new CreateOrderCommand(customerId));
        orderResult.IsSuccess.Should().BeTrue();

        Guid paymentId = await DbContext.Set<Payment>()
            .Where(p => p.OrderId == orderResult.Value)
            .Select(p => p.Id)
            .SingleAsync(CancellationToken.None);

        var command = new ProcessPaymentCommand(paymentId);

        //Act
        Result result = await Sender.Send(command);

        //Assert
        result.IsSuccess.Should().BeTrue();

        Order? order = await DbContext.Set<Order>().SingleOrDefaultAsync(o => o.Id == orderResult.Value, CancellationToken.None);
        order!.Status.Should().Be(OrderStatus.Paid);

        Payment? payment = await DbContext.Set<Payment>().SingleOrDefaultAsync(p => p.Id == paymentId, CancellationToken.None);
        payment!.Status.Should().Be(PaymentStatus.Succeeded);
        payment.TransactionReference.Should().NotBeNull();
        payment.PaidAtUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task Should_BeIdempotent_WhenPaymentIsProcessedTwice()
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
        Result result = await Sender.Send(new ProcessPaymentCommand(paymentId));

        //Assert
        result.IsSuccess.Should().BeTrue();

        Payment? payment = await DbContext.Set<Payment>().SingleOrDefaultAsync(p => p.Id == paymentId, CancellationToken.None);
        payment!.Status.Should().Be(PaymentStatus.Succeeded);
    }

    [Fact]
    public async Task Should_ReturnFailure_WhenPaymentDoesNotExist()
    {
        //Arrange
        var command = new ProcessPaymentCommand(Guid.CreateVersion7());

        //Act
        Result result = await Sender.Send(command);

        //Assert
        result.Error.Should().Be(PaymentErrors.NotFound(command.PaymentId));
    }
}
