using Evently.Common.Domain;
using Evently.Modules.Ticketing.Application.Carts;
using Evently.Modules.Ticketing.Application.Carts.AddItemToCart;
using Evently.Modules.Ticketing.Application.Orders.CancelOrder;
using Evently.Modules.Ticketing.Application.Orders.CreateOrder;
using Evently.Modules.Ticketing.Application.Orders.ProcessPayment;
using Evently.Modules.Ticketing.Application.Orders.RestockOrderItems;
using Evently.Modules.Ticketing.Domain.Events;
using Evently.Modules.Ticketing.Domain.Orders;
using Evently.Modules.Ticketing.Domain.Payments;
using Evently.Modules.Ticketing.IntegrationTests.Abstractions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Evently.Modules.Ticketing.IntegrationTests.Orders;

public class CancelOrderTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Should_ReturnFailure_WhenOrderDoesNotExist()
    {
        //Arrange
        var command = new CancelOrderCommand(Guid.CreateVersion7(), null);

        //Act
        Result result = await Sender.Send(command);

        //Assert
        result.Error.Should().Be(OrderErrors.NotFound(command.OrderId));
    }

    [Fact]
    public async Task Should_CancelPendingOrder_WhenOrderHasNotBeenPaidYet()
    {
        //Arrange
        await CleanDatabaseAsync();

        Guid customerId = await Sender.CreateCustomerAsync(Guid.CreateVersion7());
        var eventId = Guid.CreateVersion7();
        var ticketTypeId = Guid.CreateVersion7();

        await Sender.CreateEventWithTicketTypeAsync(eventId, ticketTypeId, 5m);

        await Sender.Send(new AddItemToCartCommand(customerId, ticketTypeId, 3m));

        Result<Guid> orderResult = await Sender.Send(new CreateOrderCommand(customerId));
        orderResult.IsSuccess.Should().BeTrue();

        //Act
        Result result = await Sender.Send(new CancelOrderCommand(orderResult.Value, "Customer changed their mind"));

        //Assert
        result.IsSuccess.Should().BeTrue();

        Order? order = await DbContext.Set<Order>().SingleOrDefaultAsync(o => o.Id == orderResult.Value, CancellationToken.None);
        order!.Status.Should().Be(OrderStatus.Canceled);
        order!.CancellationReason.Should().Be("Customer changed their mind");
    }

    [Fact]
    public async Task Should_RestockInventory_WhenCompensationIsExecuted()
    {
        //Arrange
        await CleanDatabaseAsync();

        Guid customerId = await Sender.CreateCustomerAsync(Guid.CreateVersion7());
        var eventId = Guid.CreateVersion7();
        var ticketTypeId = Guid.CreateVersion7();

        await Sender.CreateEventWithTicketTypeAsync(eventId, ticketTypeId, 5m);

        await Sender.Send(new AddItemToCartCommand(customerId, ticketTypeId, 3m));

        Result<Guid> orderResult = await Sender.Send(new CreateOrderCommand(customerId));
        orderResult.IsSuccess.Should().BeTrue();

        await Sender.Send(new CancelOrderCommand(orderResult.Value, null));

        //Act — the compensation step of the fulfillment saga
        Result result = await Sender.Send(new RestockOrderItemsCommand(orderResult.Value));

        //Assert
        result.IsSuccess.Should().BeTrue();

        TicketType? ticketType = await DbContext.Set<TicketType>().SingleOrDefaultAsync(t => t.Id == ticketTypeId, CancellationToken.None);
        ticketType!.AvailableQuantity.Should().Be(5m);
    }

    [Fact]
    public async Task Should_RefundPaidOrder_WhenOrderIsCanceledAfterPayment()
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

        await Sender.Send(new ProcessPaymentCommand(paymentId));

        //Act
        Result result = await Sender.Send(new CancelOrderCommand(orderResult.Value, "Customer requested a refund"));

        //Assert
        result.IsSuccess.Should().BeTrue();

        Order? order = await DbContext.Set<Order>().SingleOrDefaultAsync(o => o.Id == orderResult.Value, CancellationToken.None);
        order!.Status.Should().Be(OrderStatus.Refunded);

        Payment? payment = await DbContext.Set<Payment>().SingleOrDefaultAsync(p => p.Id == paymentId, CancellationToken.None);
        payment!.AmountRefunded.Should().Be(payment.Amount);
        payment.RefundedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task Should_ReturnFailure_WhenOrderIsAlreadyCanceled()
    {
        //Arrange
        await CleanDatabaseAsync();

        Guid customerId = await Sender.CreateCustomerAsync(Guid.CreateVersion7());
        var eventId = Guid.CreateVersion7();
        var ticketTypeId = Guid.CreateVersion7();

        await Sender.CreateEventWithTicketTypeAsync(eventId, ticketTypeId, 5m);

        await Sender.Send(new AddItemToCartCommand(customerId, ticketTypeId, 1m));

        Result<Guid> orderResult = await Sender.Send(new CreateOrderCommand(customerId));

        await Sender.Send(new CancelOrderCommand(orderResult.Value, null));

        //Act
        Result result = await Sender.Send(new CancelOrderCommand(orderResult.Value, null));

        //Assert
        result.Error.Should().Be(OrderErrors.AlreadyCanceled);
    }
}
