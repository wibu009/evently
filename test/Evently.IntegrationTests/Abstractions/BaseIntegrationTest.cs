using Bogus;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Evently.IntegrationTests.Abstractions;

[Collection(nameof(IntegrationTestCollection))]
#pragma warning disable CA1515
public abstract class BaseIntegrationTest : IDisposable
#pragma warning restore CA1515
{
    private readonly IServiceScope _scope;
    protected readonly ISender Sender;
    protected readonly Faker Faker = new();

    protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
    {
        _scope = factory.Services.CreateScope();
        Sender = _scope.ServiceProvider.GetRequiredService<ISender>();
    }

    public void Dispose()
    {
        _scope.Dispose();
    }
}
