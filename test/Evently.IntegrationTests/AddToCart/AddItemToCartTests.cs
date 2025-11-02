using Evently.Common.Domain;
using Evently.IntegrationTests.Abstractions;
using Evently.Modules.Ticketing.Application.Carts.AddItemToCart;
using Evently.Modules.Ticketing.Application.Customers.GetCustomer;
using Evently.Modules.Users.Application.Users.RegisterUser;
using FluentAssertions;

namespace Evently.IntegrationTests.AddToCart;

[Collection(nameof(DistributedIntegrationTestCollection))]
public class AddItemToCartTests(DistributedIntegrationTestFixture fixture) : DistributedIntegrationTest(fixture)
{
    private const decimal Quantity = 10;

    [Fact]
    public async Task Customer_ShouldBeAbleTo_AddItemToCart()
    {
        var register = new RegisterUserCommand(
            Faker.Internet.Email(),
            Faker.Internet.Password(6),
            Faker.Name.FirstName(),
            Faker.Name.LastName());

        Result<Guid> userResult = await EventlySender.Send(register);
        userResult.IsSuccess.Should().BeTrue();
        
        Result<CustomerResponse> customerResult = await Poller.WaitAsync(
            TimeSpan.FromSeconds(15),
            async () =>
            {
                var query = new GetCustomerQuery(userResult.Value);
                return await TicketingSender.Send(query);
            });

        customerResult.IsSuccess.Should().BeTrue();
        CustomerResponse customer = customerResult.Value;

        var ticketTypeId = Guid.CreateVersion7();
        var eventId = Guid.CreateVersion7();
        
        await TicketingSender.CreateEventAsync(eventId, ticketTypeId, Quantity);
        
        var addCommand = new AddItemToCartCommand(customer.Id, ticketTypeId, Quantity);
        Result addResult = await TicketingSender.Send(addCommand);

        addResult.IsSuccess.Should().BeTrue();
    }
}
