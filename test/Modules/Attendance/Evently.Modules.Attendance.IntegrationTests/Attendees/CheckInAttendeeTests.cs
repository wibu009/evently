using Evently.Common.Domain;
using Evently.Modules.Attendance.Application.Attendees.CheckInAttendee;
using Evently.Modules.Attendance.Domain.Attendees;
using Evently.Modules.Attendance.Domain.Tickets;
using Evently.Modules.Attendance.IntegrationTests.Abstractions;
using FluentAssertions;

namespace Evently.Modules.Attendance.IntegrationTests.Attendees;

public class CheckInAttendeeTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Should_ReturnFailure_WhenAttendeeDoesNotExist()
    {
        // Arrange
        var command = new CheckInAttendeeCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7());

        // Act
        Result result = await Sender.Send(command);

        // Assert
        result.Error.Should().Be(AttendeeErrors.NotFound(command.AttendeeId));
    }

    [Fact]
    public async Task Should_ReturnFailure_WhenTicketDoesNotExist()
    {
        // Arrange
        Guid attendeeId = await Sender.CreateAttendeeAsync(Guid.CreateVersion7());

        var command = new CheckInAttendeeCommand(
            attendeeId,
            Guid.CreateVersion7());

        // Act
        Result result = await Sender.Send(command);

        // Assert
        result.Error.Should().Be(TicketErrors.NotFound);
    }

    [Fact]
    public async Task Should_ReturnSuccess_WhenAttendeeCheckedIn()
    {
        //Arrange
        Guid attendeeId = await Sender.CreateAttendeeAsync(Guid.CreateVersion7());
        Guid eventId = await Sender.CreateEventAsync(Guid.CreateVersion7());
        Guid ticketId = await Sender.CreateTicketAsync(Guid.CreateVersion7(), attendeeId, eventId);

        var command = new CheckInAttendeeCommand(
            attendeeId,
            ticketId);

        //Act
        Result result = await Sender.Send(command);

        //Assert
        result.IsSuccess.Should().BeTrue();
    }
}
