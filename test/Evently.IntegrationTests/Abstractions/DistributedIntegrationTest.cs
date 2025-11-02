using Bogus;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Evently.IntegrationTests.Abstractions;

#pragma warning disable CA1515
public abstract class DistributedIntegrationTest : IDisposable
#pragma warning restore CA1515
{
    private readonly IServiceScope _eventlyScope;
    private readonly IServiceScope _ticketingScope;

    protected readonly ISender EventlySender;
    protected readonly ISender TicketingSender;
    protected readonly Faker Faker = new();

    protected DistributedIntegrationTest(DistributedIntegrationTestFixture fixture)
    {
        _eventlyScope = fixture.EventlyFactory.Services.CreateScope();
        _ticketingScope = fixture.TicketingFactory.Services.CreateScope();

        EventlySender = _eventlyScope.ServiceProvider.GetRequiredService<ISender>();
        TicketingSender = _ticketingScope.ServiceProvider.GetRequiredService<ISender>();
    }

    public void Dispose()
    {
        _eventlyScope.Dispose();
        _ticketingScope.Dispose();
    }
}
