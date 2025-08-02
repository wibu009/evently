using Evently.Common.Domain;
using Evently.Modules.Events.Application.Events.PublishEvent;
using Evently.Modules.Events.Domain.Events;
using Evently.Modules.Events.IntegrationTests.Abstractions;
using FluentAssertions;

namespace Evently.Modules.Events.IntegrationTests.Events;

public class PublishEventTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Should_ReturnFailure_WhenEventDoesNotExist()
    {
        // Arrange
        var eventId = Guid.CreateVersion7();

        var command = new PublishEventCommand(eventId);

        // Act
        Result result = await Sender.Send(command);

        // Assert
        result.Error.Should().Be(EventErrors.NotFound(eventId));
    }

    [Fact]
    public async Task Should_ReturnFailure_WhenEventDoesNotHaveAnyTicketTypes()
    {
        // Arrange
        Guid categoryId = await Sender.CreateCategoryAsync(Faker.Music.Genre());
        Guid eventId = await Sender.CreateEventAsync(categoryId);

        var command = new PublishEventCommand(eventId);

        // Act
        Result result = await Sender.Send(command);

        //Assert
        result.Error.Should().Be(EventErrors.NoTicketsFound);
    }

    [Fact]
    public async Task Should_ReturnSuccess_WhenEventIsPublished()
    {
        // Arrange
        Guid categoryId = await Sender.CreateCategoryAsync(Faker.Music.Genre());
        Guid eventId = await Sender.CreateEventAsync(categoryId);
        await Sender.CreateTicketTypeAsync(eventId);

        var command = new PublishEventCommand(eventId);

        // Act
        Result result = await Sender.Send(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}
