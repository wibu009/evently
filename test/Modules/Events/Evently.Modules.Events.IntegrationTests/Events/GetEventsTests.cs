using Evently.Common.Domain;
using Evently.Modules.Events.Application.Events.GetEvents;
using Evently.Modules.Events.IntegrationTests.Abstractions;
using FluentAssertions;

namespace Evently.Modules.Events.IntegrationTests.Events;

public class GetEventsTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Should_ReturnEmptyResult_WhenEventsDoNotExist()
    {
        // Arrange
        await CleanDatabaseAsync();

        var query = new GetEventsQuery(1, 10);

        // Act
        Result<GetEventsResponse> result = await Sender.Send(query);

        // Assert
        result.Value.Events.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Should_ReturnEvents_WhenEventsExist()
    {
        // Arrange
        await CleanDatabaseAsync();

        Guid categoryId = await Sender.CreateCategoryAsync(Faker.Music.Genre());

        await Sender.CreateEventAsync(categoryId);
        await Sender.CreateEventAsync(categoryId);

        var query = new GetEventsQuery(1, 10);

        // Act
        Result<GetEventsResponse> result = await Sender.Send(query);

        // Assert
        result.Value.Events.Should().HaveCount(2);
        result.Value.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Should_ReturnPagedResult_WhenPageSizeIsSmallerThanTotalCount()
    {
        // Arrange
        await CleanDatabaseAsync();

        Guid categoryId = await Sender.CreateCategoryAsync(Faker.Music.Genre());

        await Sender.CreateEventAsync(categoryId);
        await Sender.CreateEventAsync(categoryId);

        var query = new GetEventsQuery(1, 1);

        // Act
        Result<GetEventsResponse> result = await Sender.Send(query);

        // Assert
        result.Value.Events.Should().HaveCount(1);
        result.Value.TotalCount.Should().Be(2);
    }
}
