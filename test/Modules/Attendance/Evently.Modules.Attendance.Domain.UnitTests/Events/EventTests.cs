using Evently.Common.Domain;
using Evently.Modules.Attendance.Domain.Events;
using Evently.Modules.Attendance.Domain.UnitTests.Abstractions;
using FluentAssertions;

namespace Evently.Modules.Attendance.Domain.UnitTests.Events;

public class EventTests : BaseTest
{
    [Fact]
    public void Should_RaiseDomainEvent_WhenEventCreated()
    {
        //Arrange
        var eventId = Guid.CreateVersion7();
        DateTime startAtUtc = DateTime.UtcNow;
        
        //Act
        Result<Event> result = Event.Create(
            eventId,
            Faker.Music.Genre(),
            Faker.Music.Genre(),
            Faker.Address.StreetAddress(),
            startAtUtc, 
            null);
        
        //Assert
        EventCreatedDomainEvent domainEvent = AssertDomainEventWasPublished<EventCreatedDomainEvent>(result.Value);
        
        domainEvent.EventId.Should().Be(result.Value.Id);
    }
}
