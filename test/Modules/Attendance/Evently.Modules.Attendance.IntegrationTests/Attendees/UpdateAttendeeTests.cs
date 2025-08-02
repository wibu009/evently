using Evently.Common.Domain;
using Evently.Modules.Attendance.Application.Attendees.UpdateAttendee;
using Evently.Modules.Attendance.Domain.Attendees;
using Evently.Modules.Attendance.IntegrationTests.Abstractions;
using FluentAssertions;

namespace Evently.Modules.Attendance.IntegrationTests.Attendees;

public class UpdateAttendeeTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Should_ReturnFailure_WhenAttendeeDoesNotExist()
    {
        // Arrange
        var command = new UpdateAttendeeCommand(
            Guid.CreateVersion7(),
            Faker.Name.FirstName(),
            Faker.Name.LastName());

        // Act
        Result result = await Sender.Send(command);

        // Assert
        result.Error.Should().Be(AttendeeErrors.NotFound(command.AttendeeId));
    }

    [Fact]
    public async Task Should_ReturnSuccess_WhenAttendeeExists()
    {
        // Arrange
        Guid attendeeId = await Sender.CreateAttendeeAsync(Guid.CreateVersion7());

        var command = new UpdateAttendeeCommand(
            attendeeId,
            Faker.Name.FirstName(),
            Faker.Name.LastName());

        // Act
        Result result = await Sender.Send(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}
