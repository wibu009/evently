using Evently.Common.Domain;
using Evently.Modules.Ticketing.Application.Carts.AddItemToCart;
using Evently.Modules.Ticketing.Application.Orders.CreateOrder;
using Evently.Modules.Ticketing.Application.Orders.ProcessPayment;
using Evently.Modules.Ticketing.Application.Tickets.CreateTicketBatch;
using Evently.Modules.Ticketing.Application.Tickets.TransferTicket;
using Evently.Modules.Ticketing.Domain.Customers;
using Evently.Modules.Ticketing.Domain.Tickets;
using Evently.Modules.Ticketing.IntegrationTests.Abstractions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Evently.Modules.Ticketing.IntegrationTests.Tickets;

public class TransferTicketTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    private async Task<Guid> CreatePaidOrderWithTicketAsync()
    {
        await CleanDatabaseAsync();

        Guid customerId = await Sender.CreateCustomerAsync(Guid.CreateVersion7());
        var eventId = Guid.CreateVersion7();
        var ticketTypeId = Guid.CreateVersion7();

        await Sender.CreateEventWithTicketTypeAsync(eventId, ticketTypeId, 5m);

        await Sender.Send(new AddItemToCartCommand(customerId, ticketTypeId, 1m));

        Result<Guid> orderResult = await Sender.Send(new CreateOrderCommand(customerId));
        orderResult.IsSuccess.Should().BeTrue();

        Guid paymentId = await DbContext.Set<Domain.Payments.Payment>()
            .Where(p => p.OrderId == orderResult.Value)
            .Select(p => p.Id)
            .SingleAsync(CancellationToken.None);

        await Sender.Send(new ProcessPaymentCommand(paymentId));

        await Sender.Send(new CreateTicketBatchCommand(orderResult.Value));

        return await DbContext.Set<Ticket>()
            .Where(t => t.OrderId == orderResult.Value)
            .Select(t => t.Id)
            .SingleAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Should_ChangeOwner_WhenTicketIsTransferred()
    {
        //Arrange
        Guid ticketId = await CreatePaidOrderWithTicketAsync();

        Guid newCustomerId = await Sender.CreateCustomerAsync(Guid.CreateVersion7());

        var command = new TransferTicketCommand(ticketId, newCustomerId);

        //Act
        Result result = await Sender.Send(command);

        //Assert
        result.IsSuccess.Should().BeTrue();

        Ticket? ticket = await DbContext.Set<Ticket>()
            .SingleOrDefaultAsync(t => t.Id == ticketId, CancellationToken.None);

        ticket!.CustomerId.Should().Be(newCustomerId);
        ticket.Archived.Should().BeFalse();
    }

    [Fact]
    public async Task Should_ReturnFailure_WhenNewCustomerDoesNotExist()
    {
        //Arrange
        Guid ticketId = await CreatePaidOrderWithTicketAsync();

        var command = new TransferTicketCommand(ticketId, Guid.CreateVersion7());

        //Act
        Result result = await Sender.Send(command);

        //Assert
        result.Error.Should().Be(CustomerErrors.NotFound(command.ToCustomerId));
    }
}
