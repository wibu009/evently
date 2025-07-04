﻿using Evently.Common.Domain;
using Evently.Modules.Attendance.Domain.Attendees;
using Evently.Modules.Attendance.Domain.Events;
using Evently.Modules.Attendance.Domain.Tickets;
using Evently.Modules.Attendance.Domain.UnitTests.Abstractions;
using FluentAssertions;

namespace Evently.Modules.Attendance.Domain.UnitTests.Tickets;

public class TicketTests : BaseTest
{
    [Fact]
    public void Create_ShouldRaiseDomainEvent_WhenTicketIsCreated()
    {
        //Arrange
        var attendee = Attendee.Create(
            Guid.CreateVersion7(), 
            Faker.Internet.Email(),
            Faker.Person.FirstName, 
            Faker.Person.LastName);

        DateTime startsAtUtc = DateTime.UtcNow;

        var @event = Event.Create(
            Guid.CreateVersion7(), 
            Faker.Music.Genre(), 
            Faker.Music.Genre(), 
            Faker.Address.StreetName(), 
            startsAtUtc, null);

        //Act
        Result<Ticket> result = Ticket.Create(
            Guid.CreateVersion7(),
            attendee,
            @event,
            Faker.Random.String());

        //Assert
        TicketCreatedDomainEvent domainEvent =
            AssertDomainEventWasPublished<TicketCreatedDomainEvent>(result.Value);

        domainEvent.TicketId.Should().Be(result.Value.Id);
    }
    
    [Fact]
    public void MarkAsUsed_ShouldRaiseDomainEvent_WhenTicketIsUsed()
    {
        //Arrange
        var attendee = Attendee.Create(
            Guid.CreateVersion7(), 
            Faker.Internet.Email(), 
            Faker.Person.FirstName, 
            Faker.Person.LastName);

        DateTime startsAtUtc = DateTime.UtcNow;

        var @event = Event.Create(
            Guid.CreateVersion7(), 
            Faker.Music.Genre(), 
            Faker.Music.Genre(), 
            Faker.Address.StreetName(),
            startsAtUtc, null);

        var ticket = Ticket.Create(
            Guid.CreateVersion7(),
            attendee,
            @event,
            Faker.Random.String());

        //Act
        ticket.MarkAsUsed();

        //Assert
        TicketUsedDomainEvent domainEvent =
            AssertDomainEventWasPublished<TicketUsedDomainEvent>(ticket);

        domainEvent.TicketId.Should().Be(ticket.Id);
    }
}
