namespace Evently.IntegrationTests.Abstractions;

#pragma warning disable CA1515
public sealed class DistributedIntegrationTestFixture : IAsyncLifetime
#pragma warning restore CA1515
{
    public TestEnvironment Environment { get; private set; } = null!;
    public EventlyWebAppFactory EventlyFactory { get; private set; } = null!;
    public TicketingWebAppFactory TicketingFactory { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        Environment = new TestEnvironment();
        await Environment.InitializeAsync();

        EventlyFactory = new EventlyWebAppFactory(Environment);
        TicketingFactory = new TicketingWebAppFactory(Environment);
    }

    public async Task DisposeAsync()
    {
        await EventlyFactory.DisposeAsync();
        await TicketingFactory.DisposeAsync();
        await Environment.DisposeAsync();
    }
}
