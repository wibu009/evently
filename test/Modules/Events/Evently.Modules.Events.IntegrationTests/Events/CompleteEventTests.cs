using Evently.Common.Domain;
using Evently.Modules.Events.Application.Events.CompleteEvent;
using Evently.Modules.Events.Application.Events.GetEvent;
using Evently.Modules.Events.Application.Events.PublishEvent;
using Evently.Modules.Events.Domain.Events;
using Evently.Modules.Events.IntegrationTests.Abstractions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Evently.Modules.Events.IntegrationTests.Events;

public class CompleteEventTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Should_ReturnFailure_WhenEventDoesNotExist()
    {
        // Arrange
        var command = new CompleteEventCommand(Guid.CreateVersion7());

        // Act
        Result result = await Sender.Send(command);

        // Assert
        result.Error.Should().Be(EventErrors.NotFound(command.EventId));
    }

    [Fact]
    public async Task Should_ReturnFailure_WhenEventIsStillDraft()
    {
        // Arrange
        await CleanDatabaseAsync();

        Guid categoryId = await Sender.CreateCategoryAsync(Faker.Music.Genre());
        Guid eventId = await Sender.CreateEventAsync(categoryId);

        var command = new CompleteEventCommand(eventId);

        // Act
        Result result = await Sender.Send(command);

        // Assert
        result.Error.Should().Be(EventErrors.NotPublished);
    }

    [Fact]
    public async Task Should_ReturnFailure_WhenEventHasNotStartedYet()
    {
        // Arrange
        await CleanDatabaseAsync();

        Guid categoryId = await Sender.CreateCategoryAsync(Faker.Music.Genre());
        Guid eventId = await Sender.CreateEventAsync(categoryId, DateTime.UtcNow.AddMinutes(10));
        await Sender.CreateTicketTypeAsync(eventId);

        Result publishResult = await Sender.Send(new PublishEventCommand(eventId));
        publishResult.IsSuccess.Should().BeTrue();

        var command = new CompleteEventCommand(eventId);

        // Act
        Result result = await Sender.Send(command);

        // Assert
        result.Error.Should().Be(EventErrors.NotStarted);
    }

    [Fact]
    public async Task Should_CompleteEvent_WhenEventIsPublishedAndStarted()
    {
        // Arrange
        await CleanDatabaseAsync();

        Guid categoryId = await Sender.CreateCategoryAsync(Faker.Music.Genre());
        Guid eventId = await Sender.CreateEventAsync(categoryId);
        await Sender.CreateTicketTypeAsync(eventId);

        Result publishResult = await Sender.Send(new PublishEventCommand(eventId));
        publishResult.IsSuccess.Should().BeTrue();

        // Simulate the event having started.
        await DbContext.Database.ExecuteSqlRawAsync(
            "UPDATE events.events SET start_at_utc = now() - interval '1 minute' WHERE id = {0}",
            eventId);

        // The raw SQL bypassed the change tracker, so drop the tracked entity
        // before the command re-reads it.
        DbContext.ChangeTracker.Clear();

        Result<EventResponse> beforeResult = await Sender.Send(new GetEventQuery(eventId));
        beforeResult.Value.StartAtUtc.Should().BeBefore(
            DateTime.UtcNow,
            $"the event start should be in the past but was {beforeResult.Value.StartAtUtc:O} (kind {beforeResult.Value.StartAtUtc.Kind}) vs {DateTime.UtcNow:O}");

        var command = new CompleteEventCommand(eventId);

        // Act
        Result result = await Sender.Send(command);

        // Assert
        result.IsSuccess.Should().BeTrue($"expected the event to be completed but got {result.Error?.Code}: {result.Error?.Description}");

        Result<EventResponse> eventResult = await Sender.Send(new GetEventQuery(eventId));
        eventResult.IsSuccess.Should().BeTrue();
    }
}
