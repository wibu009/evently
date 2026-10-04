using Evently.Common.Domain;
using Evently.Modules.Ticketing.Application.Carts;
using Evently.Modules.Ticketing.Application.Carts.AddItemToCart;
using Evently.Modules.Ticketing.Application.Orders.CreateOrder;
using Evently.Modules.Ticketing.Domain.Customers;
using Evently.Modules.Ticketing.Domain.Events;
using Evently.Modules.Ticketing.Domain.Orders;
using Evently.Modules.Ticketing.Domain.Payments;
using Evently.Modules.Ticketing.IntegrationTests.Abstractions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Evently.Modules.Ticketing.IntegrationTests.Orders;

public class CreateOrderTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Should_ReturnFailure_WhenCustomerDoesNotExist()
    {
        //Arrange
        var command = new CreateOrderCommand(Guid.CreateVersion7());

        //Act
        Result<Guid> result = await Sender.Send(command);

        //Assert
        result.Error.Should().Be(CustomerErrors.NotFound(command.CustomerId));
    }

    [Fact]
    public async Task Should_ReturnFailure_WhenCartIsEmpty()
    {
        //Arrange
        Guid customerId = await Sender.CreateCustomerAsync(Guid.CreateVersion7());

        var command = new CreateOrderCommand(customerId);

        //Act
        Result<Guid> result = await Sender.Send(command);

        //Assert
        result.Error.Should().Be(CartErrors.Empty);
    }

    [Fact]
    public async Task Should_CreatePendingOrder_AndReserveInventory_WhenCartContainsItems()
    {
        //Arrange
        await CleanDatabaseAsync();

        Guid customerId = await Sender.CreateCustomerAsync(Guid.CreateVersion7());
        var eventId = Guid.CreateVersion7();
        var ticketTypeId = Guid.CreateVersion7();

        await Sender.CreateEventWithTicketTypeAsync(eventId, ticketTypeId, 5m);

        await Sender.Send(new AddItemToCartCommand(customerId, ticketTypeId, 5m));

        var command = new CreateOrderCommand(customerId);

        //Act
        Result<Guid> result = await Sender.Send(command);

        //Assert
        result.IsSuccess.Should().BeTrue();

        Order? order = await DbContext.Set<Order>()
            .Include(o => o.OrderItems)
            .SingleOrDefaultAsync(o => o.Id == result.Value, CancellationToken.None);

        order.Should().NotBeNull();
        order!.Status.Should().Be(OrderStatus.Pending);
        order.PaymentDueUtc.Should().NotBeNull();
        order.TotalPrice.Should().BeGreaterThan(decimal.Zero);
        order.OrderItems.Should().ContainSingle(oi => oi.TicketTypeId == ticketTypeId);

        Payment? payment = await DbContext.Set<Payment>()
            .SingleOrDefaultAsync(p => p.OrderId == result.Value, CancellationToken.None);

        payment.Should().NotBeNull();
        payment!.Status.Should().Be(PaymentStatus.Pending);
        payment.TransactionReference.Should().BeNull();

        TicketType? ticketType = await DbContext.Set<TicketType>()
            .SingleOrDefaultAsync(t => t.Id == ticketTypeId, CancellationToken.None);

        ticketType!.AvailableQuantity.Should().Be(decimal.Zero);
    }
}
