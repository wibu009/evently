using Evently.Common.Domain;
using Evently.Modules.Ticketing.Domain.Customers;
using Evently.Modules.Ticketing.Domain.Events;
using Evently.Modules.Ticketing.Domain.Orders;
using Evently.Modules.Ticketing.Domain.Tickets;
using Evently.Modules.Ticketing.Domain.UnitTests.Abstractions;
using FluentAssertions;

namespace Evently.Modules.Ticketing.Domain.UnitTests.Tickets;

public class TicketTests : BaseTest
{
    private static (Order Order, TicketType TicketType) CreateOrderAndTicketType()
    {
        var customer = Customer.Create(
            Guid.CreateVersion7(),
            Faker.Internet.Email(),
            Faker.Name.FirstName(),
            Faker.Name.LastName());

        var order = Order.Create(customer);

        DateTime startsAtUtc = DateTime.UtcNow;
        var @event = Event.Create(
            Guid.CreateVersion7(),
            Faker.Music.Genre(),
            Faker.Music.Genre(),
            Faker.Address.StreetAddress(),
            startsAtUtc,
            null);

        var ticketType = TicketType.Create(
            Guid.CreateVersion7(),
            @event.Id,
            Faker.Name.FirstName(),
            Faker.Random.Decimal(),
            Faker.Random.String(3),
            Faker.Random.Decimal());

        return (order, ticketType);
    }

    private static Ticket CreateTicket()
    {
        (Order order, TicketType ticketType) = CreateOrderAndTicketType();

        return Ticket.Create(order, ticketType);
    }

    [Fact]
    public void Create_ShouldRaiseDomainEvent_WhenTicketIsCreated()
    {
        //Arrange
        (Order order, TicketType ticketType) = CreateOrderAndTicketType();

        //Act
        Result<Ticket> result = Ticket.Create(
            order,
            ticketType);

        //Assert
        TicketCreatedDomainEvent domainEvent =
            AssertDomainEventWasPublished<TicketCreatedDomainEvent>(result.Value);

        domainEvent.TicketId.Should().Be(result.Value.Id);
    }

    [Fact]
    public void Archive_ShouldRaiseDomainEvent_WhenTicketIsArchived()
    {
        //Arrange
        Ticket ticket = CreateTicket();

        //Act
        ticket.Archive();

        //Assert
        TicketArchivedDomainEvent domainEvent =
            AssertDomainEventWasPublished<TicketArchivedDomainEvent>(ticket);

        domainEvent.TicketId.Should().Be(ticket.Id);
    }

    [Fact]
    public void Transfer_ShouldChangeOwner_AndRaiseDomainEvent_WhenTicketIsTransferred()
    {
        //Arrange
        (Order order, TicketType ticketType) = CreateOrderAndTicketType();

        var ticket = Ticket.Create(order, ticketType);

        // Recreate the current owner with the ticket's owner id.
        var fromCustomer = Customer.Create(
            order.CustomerId,
            Faker.Internet.Email(),
            Faker.Name.FirstName(),
            Faker.Name.LastName());

        var toCustomer = Customer.Create(
            Guid.CreateVersion7(),
            Faker.Internet.Email(),
            Faker.Name.FirstName(),
            Faker.Name.LastName());

        //Act
        Result result = ticket.Transfer(fromCustomer, toCustomer);

        //Assert
        result.IsSuccess.Should().BeTrue();
        ticket.CustomerId.Should().Be(toCustomer.Id);

        TicketTransferredDomainEvent domainEvent =
            AssertDomainEventWasPublished<TicketTransferredDomainEvent>(ticket);

        domainEvent.TicketId.Should().Be(ticket.Id);
        domainEvent.Code.Should().Be(ticket.Code);
        domainEvent.FromCustomerId.Should().Be(fromCustomer.Id);
        domainEvent.ToCustomerId.Should().Be(toCustomer.Id);
    }

    [Fact]
    public void Transfer_ShouldReturnFailure_WhenTicketIsArchived()
    {
        //Arrange
        Ticket ticket = CreateTicket();
        ticket.Archive();

        var fromCustomer = Customer.Create(
            Guid.CreateVersion7(),
            Faker.Internet.Email(),
            Faker.Name.FirstName(),
            Faker.Name.LastName());

        var toCustomer = Customer.Create(
            Guid.CreateVersion7(),
            Faker.Internet.Email(),
            Faker.Name.FirstName(),
            Faker.Name.LastName());

        //Act
        Result result = ticket.Transfer(fromCustomer, toCustomer);

        //Assert
        result.Error.Should().Be(TicketErrors.CannotTransferArchivedTicket);
    }

    [Fact]
    public void Transfer_ShouldReturnFailure_WhenTicketIsNotOwnedByTheCurrentCustomer()
    {
        //Arrange
        Ticket ticket = CreateTicket();

        var fromCustomer = Customer.Create(
            Guid.CreateVersion7(),
            Faker.Internet.Email(),
            Faker.Name.FirstName(),
            Faker.Name.LastName());

        var toCustomer = Customer.Create(
            Guid.CreateVersion7(),
            Faker.Internet.Email(),
            Faker.Name.FirstName(),
            Faker.Name.LastName());

        //Act
        Result result = ticket.Transfer(fromCustomer, toCustomer);

        //Assert
        result.Error.Should().Be(TicketErrors.NotOwnedByCustomer(fromCustomer.Id));
    }

    [Fact]
    public void Transfer_ShouldReturnFailure_WhenTicketIsTransferredToTheSameCustomer()
    {
        //Arrange
        (Order order, TicketType ticketType) = CreateOrderAndTicketType();

        var ticket = Ticket.Create(order, ticketType);

        // Recreate the customer with the ticket's owner id so both sides match.
        var customer = Customer.Create(
            order.CustomerId,
            Faker.Internet.Email(),
            Faker.Name.FirstName(),
            Faker.Name.LastName());

        //Act
        Result result = ticket.Transfer(customer, customer);

        //Assert
        result.Error.Should().Be(TicketErrors.CannotTransferToSelf);
    }
}
