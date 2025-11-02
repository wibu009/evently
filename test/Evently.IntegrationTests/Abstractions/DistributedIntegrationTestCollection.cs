namespace Evently.IntegrationTests.Abstractions;

[CollectionDefinition(nameof(DistributedIntegrationTestCollection))]
#pragma warning disable CA1515
public sealed class DistributedIntegrationTestCollection : ICollectionFixture<DistributedIntegrationTestFixture>;
#pragma warning restore CA1515
