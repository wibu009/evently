namespace Evently.Modules.Attendance.IntegrationTests.Abstractions;

[CollectionDefinition(nameof(IntegrationTestCollection))]
#pragma warning disable CA1515
public sealed class IntegrationTestCollection : ICollectionFixture<IntegrationTestWebAppFactory>;
