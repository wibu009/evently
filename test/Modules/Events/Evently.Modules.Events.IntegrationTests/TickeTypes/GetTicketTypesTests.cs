using Evently.Common.Domain;
using Evently.Modules.Events.Application.TicketTypes.GetTicketType;
using Evently.Modules.Events.Application.TicketTypes.GetTicketTypes;
using Evently.Modules.Events.IntegrationTests.Abstractions;
using FluentAssertions;

namespace Evently.Modules.Events.IntegrationTests.TickeTypes;

public class GetTicketTypesTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Should_ReturnFailure_WhenTicketTypesDoNotExist()
    {
        // Arrange
        await CleanDatabaseAsync();

        var query = new GetTicketTypesQuery(Guid.CreateVersion7());

        // Act
        Result<IReadOnlyCollection<TicketTypeResponse>> result = await Sender.Send(query);

        // Assert
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Should_ReturnTicketTypes_WhenTicketTypesExists()
    {
        // Arrange
        await CleanDatabaseAsync();

        Guid categoryId = await Sender.CreateCategoryAsync(Faker.Music.Genre());
        Guid eventId = await Sender.CreateEventAsync(categoryId);

        await Sender.CreateTicketTypeAsync(eventId);
        await Sender.CreateTicketTypeAsync(eventId);

        var query = new GetTicketTypesQuery(eventId);

        // Act
        Result<IReadOnlyCollection<TicketTypeResponse>> result = await Sender.Send(query);

        // Assert
        result.Value.Should().HaveCount(2);
    }
}
