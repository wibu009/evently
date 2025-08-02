using Evently.Common.Domain;
using Evently.Modules.Attendance.Application.Events.CreateEvent;
using Evently.Modules.Attendance.IntegrationTests.Abstractions;
using FluentAssertions;

namespace Evently.Modules.Attendance.IntegrationTests.Events;

public class CreateEventTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    public static readonly TheoryData<Guid, string, string, string, DateTime, DateTime?> InvalidData = new()
    {
        { Guid.Empty, Faker.Music.Genre(), Faker.Music.Genre(), Faker.Address.StreetAddress(), default, null },
        { Guid.CreateVersion7(), string.Empty, Faker.Music.Genre(), Faker.Address.StreetAddress(), default, null },
        { Guid.CreateVersion7(), Faker.Music.Genre(), string.Empty, Faker.Address.StreetAddress(), default, null },
        { Guid.CreateVersion7(), Faker.Music.Genre(), Faker.Music.Genre(), string.Empty, default, null },
        { Guid.CreateVersion7(), Faker.Music.Genre(), Faker.Music.Genre(), Faker.Address.StreetAddress(), default, null }
    };

    [Theory]
    [MemberData(nameof(InvalidData))]
    public async Task Should_ReturnFailure_WhenCommandIsInvalid(
        Guid eventId,
        string title,
        string description,
        string location,
        DateTime startsAtUtc,
        DateTime? endsAtUtc)
    {
        // Arrange
        var command = new CreateEventCommand(eventId, title, description, location, startsAtUtc, endsAtUtc);

        // Act
        Result result = await Sender.Send(command);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Should_ReturnSuccess_WhenCommandIsValid()
    {
        // Arrange
        var eventId = Guid.CreateVersion7();

        var command = new CreateEventCommand(
            eventId,
            Faker.Music.Genre(),
            Faker.Music.Genre(),
            Faker.Address.StreetAddress(),
            DateTime.UtcNow.AddMinutes(10),
            null);

        // Act
        Result result = await Sender.Send(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}
