using Evently.Common.Domain;
using Evently.Modules.Ticketing.Application.Carts.AddItemToCart;
using Evently.Modules.Ticketing.Application.Orders.CreateOrder;
using Evently.Modules.Ticketing.Application.WaitingList.JoinWaitingList;
using Evently.Modules.Ticketing.Application.WaitingList.LeaveWaitingList;
using Evently.Modules.Ticketing.Application.WaitingList.NotifyWaitingList;
using Evently.Modules.Ticketing.Domain.WaitingList;
using Evently.Modules.Ticketing.IntegrationTests.Abstractions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Evently.Modules.Ticketing.IntegrationTests.WaitingList;

public class WaitingListTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Should_ReturnFailure_WhenTicketTypeIsNotSoldOut()
    {
        //Arrange
        await CleanDatabaseAsync();

        Guid customerId = await Sender.CreateCustomerAsync(Guid.CreateVersion7());
        var eventId = Guid.CreateVersion7();
        var ticketTypeId = Guid.CreateVersion7();

        await Sender.CreateEventWithTicketTypeAsync(eventId, ticketTypeId, 5m);

        var command = new JoinWaitingListCommand(customerId, ticketTypeId);

        //Act
        Result result = await Sender.Send(command);

        //Assert
        result.Error.Should().Be(WaitingListErrors.NotSoldOut);
    }

    [Fact]
    public async Task Should_JoinWaitingList_WhenTicketTypeIsSoldOut()
    {
        //Arrange
        await CleanDatabaseAsync();

        Guid customerId = await Sender.CreateCustomerAsync(Guid.CreateVersion7());
        var eventId = Guid.CreateVersion7();
        var ticketTypeId = Guid.CreateVersion7();

        await Sender.CreateEventWithTicketTypeAsync(eventId, ticketTypeId, 1m);

        await Sender.Send(new AddItemToCartCommand(customerId, ticketTypeId, 1m));
        await Sender.Send(new CreateOrderCommand(customerId));

        var command = new JoinWaitingListCommand(customerId, ticketTypeId);

        //Act
        Result result = await Sender.Send(command);

        //Assert
        result.IsSuccess.Should().BeTrue();

        WaitingListEntry? entry = await DbContext.Set<WaitingListEntry>()
            .SingleOrDefaultAsync(e => e.TicketTypeId == ticketTypeId, CancellationToken.None);

        entry.Should().NotBeNull();
        entry!.Status.Should().Be(WaitingListEntryStatus.Waiting);
    }

    [Fact]
    public async Task Should_ReturnFailure_WhenCustomerJoinsTwice()
    {
        //Arrange
        await CleanDatabaseAsync();

        Guid customerId = await Sender.CreateCustomerAsync(Guid.CreateVersion7());
        var eventId = Guid.CreateVersion7();
        var ticketTypeId = Guid.CreateVersion7();

        await Sender.CreateEventWithTicketTypeAsync(eventId, ticketTypeId, 1m);

        await Sender.Send(new AddItemToCartCommand(customerId, ticketTypeId, 1m));
        await Sender.Send(new CreateOrderCommand(customerId));

        await Sender.Send(new JoinWaitingListCommand(customerId, ticketTypeId));

        //Act
        Result result = await Sender.Send(new JoinWaitingListCommand(customerId, ticketTypeId));

        //Assert
        result.Error.Should().Be(WaitingListErrors.AlreadyJoined);
    }

    [Fact]
    public async Task Should_RemoveEntry_WhenCustomerLeavesTheWaitingList()
    {
        //Arrange
        await CleanDatabaseAsync();

        Guid customerId = await Sender.CreateCustomerAsync(Guid.CreateVersion7());
        var eventId = Guid.CreateVersion7();
        var ticketTypeId = Guid.CreateVersion7();

        await Sender.CreateEventWithTicketTypeAsync(eventId, ticketTypeId, 1m);

        await Sender.Send(new AddItemToCartCommand(customerId, ticketTypeId, 1m));
        await Sender.Send(new CreateOrderCommand(customerId));

        await Sender.Send(new JoinWaitingListCommand(customerId, ticketTypeId));

        //Act
        Result result = await Sender.Send(new LeaveWaitingListCommand(customerId, ticketTypeId));

        //Assert
        result.IsSuccess.Should().BeTrue();

        WaitingListEntry? entry = await DbContext.Set<WaitingListEntry>()
            .SingleOrDefaultAsync(e => e.TicketTypeId == ticketTypeId, CancellationToken.None);

        entry.Should().BeNull();

        //Act — leaving again fails
        Result secondResult = await Sender.Send(new LeaveWaitingListCommand(customerId, ticketTypeId));

        //Assert
        secondResult.Error.Should().Be(WaitingListErrors.NotJoined);
    }

    [Fact]
    public async Task Should_NotifyEarliestWaitingCustomers_WhenInventoryIsRestocked()
    {
        //Arrange
        await CleanDatabaseAsync();

        var ticketTypeId = Guid.CreateVersion7();
        var eventId = Guid.CreateVersion7();

        await Sender.CreateEventWithTicketTypeAsync(eventId, ticketTypeId, 1m);

        // Sell out the ticket type so that customers can join the waiting list.
        Guid buyerId = await Sender.CreateCustomerAsync(Guid.CreateVersion7());
        await Sender.Send(new AddItemToCartCommand(buyerId, ticketTypeId, 1m));
        Result createOrderResult = await Sender.Send(new CreateOrderCommand(buyerId));
        createOrderResult.IsSuccess.Should().BeTrue();

        // Two customers join the sold out waiting list.
        foreach (Guid customerId in new[] { Guid.CreateVersion7(), Guid.CreateVersion7() })
        {
            await Sender.CreateCustomerAsync(customerId);

            Result joinResult = await Sender.Send(new JoinWaitingListCommand(customerId, ticketTypeId));
            joinResult.IsSuccess.Should().BeTrue();
        }

        var command = new NotifyWaitingListCommand(ticketTypeId, 1m);

        //Act — only one spot became available
        Result result = await Sender.Send(command);

        //Assert
        result.IsSuccess.Should().BeTrue();

        List<WaitingListEntry> entries = await DbContext.Set<WaitingListEntry>()
            .Where(e => e.TicketTypeId == ticketTypeId)
            .OrderBy(e => e.CreatedAtUtc)
            .ToListAsync(CancellationToken.None);

        entries.Should().HaveCount(2);
        entries[0].Status.Should().Be(WaitingListEntryStatus.Notified);
        entries[0].NotifiedAtUtc.Should().NotBeNull();
        entries[1].Status.Should().Be(WaitingListEntryStatus.Waiting);
        entries[1].NotifiedAtUtc.Should().BeNull();
    }
}
