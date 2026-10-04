using Evently.Common.Domain;
using Evently.Modules.Events.Application.Events.GetEvent;
using Evently.Modules.Events.Application.Events.UpdateEvent;
using Evently.Modules.Events.Domain.Events;
using Evently.Modules.Events.IntegrationTests.Abstractions;
using FluentAssertions;

namespace Evently.Modules.Events.IntegrationTests.Events;

public class UpdateEventTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Should_ReturnFailure_WhenEventDoesNotExist()
    {
        // Arrange
        var command = new UpdateEventCommand(
            Guid.CreateVersion7(),
            Faker.Music.Genre(),
            Faker.Music.Genre(),
            Faker.Address.StreetAddress());

        // Act
        Result result = await Sender.Send(command);

        // Assert
        result.Error.Should().Be(EventErrors.NotFound(command.EventId));
    }

    [Fact]
    public async Task Should_UpdateEventDetails_WhenCommandIsValid()
    {
        // Arrange
        await CleanDatabaseAsync();

        Guid categoryId = await Sender.CreateCategoryAsync(Faker.Music.Genre());
        Guid eventId = await Sender.CreateEventAsync(categoryId);

        string newTitle = Faker.Music.Genre();
        string newDescription = Faker.Lorem.Sentence();
        string newLocation = Faker.Address.StreetAddress();

        var command = new UpdateEventCommand(eventId, newTitle, newDescription, newLocation);

        // Act
        Result result = await Sender.Send(command);

        // Assert
        result.IsSuccess.Should().BeTrue();

        Result<EventResponse> eventResult = await Sender.Send(new GetEventQuery(eventId));
        eventResult.Value.Title.Should().Be(newTitle);
        eventResult.Value.Description.Should().Be(newDescription);
        eventResult.Value.Location.Should().Be(newLocation);
    }
}
