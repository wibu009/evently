using Evently.Common.Domain;
using Evently.Modules.Ticketing.Application.Events.RescheduleEvent;
using Evently.Modules.Ticketing.Domain.Events;
using Evently.Modules.Ticketing.IntegrationTests.Abstractions;
using FluentAssertions;

namespace Evently.Modules.Ticketing.IntegrationTests.Events;

public class RescheduleEventTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    private const decimal Quantity = 10;
    
    [Fact]
    public async Task Should_ReturnFailure_WhenEventDoesNotExist()
    {
        //Arrange
        var eventId = Guid.CreateVersion7();
        DateTime startsAtUtc = DateTime.UtcNow;
        DateTime endsAtUtc = startsAtUtc.AddHours(1);

        var command = new RescheduleEventCommand(eventId, startsAtUtc, endsAtUtc);

        //Act
        Result result = await Sender.Send(command);

        //Assert
        result.Error.Should().Be(EventErrors.NotFound(command.EventId));
    }

    [Fact]
    public async Task Should_ReturnFailure_WhenEventAlreadyStarted()
    {
        //Arrange
        var eventId = Guid.CreateVersion7();
        var ticketTypeId = Guid.CreateVersion7();
        DateTime startsAtUtc = DateTime.UtcNow.AddMinutes(-5);
        DateTime endsAtUtc = startsAtUtc.AddHours(1);

        await Sender.CreateEventWithTicketTypeAsync(eventId, ticketTypeId, Quantity);

        var command = new RescheduleEventCommand(eventId, startsAtUtc, endsAtUtc);

        //Act
        Result result = await Sender.Send(command);

        //Assert
        result.Error.Should().Be(EventErrors.StartDateInPast);
    }

    [Fact]
    public async Task Should_ReturnSuccess_WhenEventRescheduled()
    {
        //Arrange
        var eventId = Guid.CreateVersion7();
        var ticketTypeId = Guid.CreateVersion7();
        DateTime startsAtUtc = DateTime.UtcNow;
        DateTime endsAtUtc = startsAtUtc.AddHours(1);

        await Sender.CreateEventWithTicketTypeAsync(eventId, ticketTypeId, Quantity);

        var command = new RescheduleEventCommand(eventId, startsAtUtc, endsAtUtc);

        //Act
        Result result = await Sender.Send(command);

        //Assert
        result.Error.Should().Be(EventErrors.StartDateInPast);
    }
}
