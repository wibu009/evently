using Evently.Common.Domain;
using Evently.Modules.Attendance.Application.Tickets.CreateTicket;
using Evently.Modules.Attendance.Domain.Attendees;
using Evently.Modules.Attendance.Domain.Events;
using Evently.Modules.Attendance.IntegrationTests.Abstractions;
using FluentAssertions;

namespace Evently.Modules.Attendance.IntegrationTests.Tickets;

public class CreateTicketsTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Should_ReturnFailure_WhenAttendeeDoesNotExist()
    {
        // Arrange
        var command = new CreateTicketCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Faker.Random.String());

        // Act
        Result result = await Sender.Send(command);

        // Assert
        result.Error.Should().Be(AttendeeErrors.NotFound(command.AttendeeId));
    }

    [Fact]
    public async Task Should_ReturnFailure_WhenEventDoesNotExist()
    {
        // Arrange
        Guid attendeeId = await Sender.CreateAttendeeAsync(Guid.CreateVersion7());

        var command = new CreateTicketCommand(
            Guid.CreateVersion7(),
            attendeeId,
            Guid.CreateVersion7(),
            Faker.Random.String());

        // Act
        Result result = await Sender.Send(command);

        // Assert
        result.Error.Should().Be(EventErrors.NotFound(command.EventId));
    }

    [Fact]
    public async Task Should_ReturnSuccess_WhenTicketIsCreated()
    {
        //Arrange
        Guid attendeeId = await Sender.CreateAttendeeAsync(Guid.CreateVersion7());
        Guid eventId = await Sender.CreateEventAsync(Guid.CreateVersion7());

        var command = new CreateTicketCommand(
            Guid.CreateVersion7(),
            attendeeId,
            eventId,
            Ulid.NewUlid().ToString());

        //Act
        Result result = await Sender.Send(command);

        //Assert
        result.IsSuccess.Should().BeTrue();
    }
}
