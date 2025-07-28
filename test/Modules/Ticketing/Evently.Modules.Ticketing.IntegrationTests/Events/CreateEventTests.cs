using Evently.Common.Domain;
using Evently.Modules.Ticketing.Application.Events.CreateEvent;
using Evently.Modules.Ticketing.IntegrationTests.Abstractions;
using FluentAssertions;

namespace Evently.Modules.Ticketing.IntegrationTests.Events;

public class CreateEventTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Should_ReturnSuccess_WhenEventIsCreated()
    {
        //Arrange
        var eventId = Guid.CreateVersion7();
        var ticketTypeId = Guid.CreateVersion7();
        decimal quantity = Faker.Random.Decimal();

        var ticketType = new TicketTypeRequest(
            ticketTypeId,
            eventId,
            Faker.Music.Genre(),
            Faker.Random.Decimal(),
            Faker.Random.String(3),
            quantity);

        var command = new CreateEventCommand(
            eventId,
            Faker.Music.Genre(),
            Faker.Music.Genre(),
            Faker.Address.FullAddress(),
            DateTime.UtcNow,
            null,
            [ticketType]);

        //Act
        Result result = await Sender.Send(command);

        //Assert
        result.IsSuccess.Should().BeTrue();
    }
}
