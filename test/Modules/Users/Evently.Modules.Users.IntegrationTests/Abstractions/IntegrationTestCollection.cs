namespace Evently.Modules.Users.IntegrationTests.Abstractions;

[CollectionDefinition(nameof(IntegrationTestCollection))]
#pragma warning disable CA1515
public sealed class IntegrationTestCollection : ICollectionFixture<IntegrationTestWebAppFactory>;
#pragma warning restore CA1515
