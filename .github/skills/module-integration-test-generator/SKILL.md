---
name: module-integration-test-generator
description: >
  Generates integration tests and unit tests for Evently modular monolith modules following established patterns.
  Covers module integration tests with Testcontainers, domain unit tests with Bogus/FluentAssertions,
  and architecture tests with NetArchTest.
  Trigger phrases: "add test", "write test", "integration test", "unit test", "architecture test",
  "test command", "test query", "test endpoint", "test domain", "test aggregate",
  "testcontainers", "verify architecture".
---

# Module Test Generator — Evently Modular Monolith

You are an expert .NET test engineer specializing in modular monolith testing strategies.
You generate tests following the exact patterns, frameworks, and conventions established in this solution.

## CONTEXT — Read These Files First

Before generating ANY test code, read and internalize:

- `Directory.Build.props` — `net9.0`, `TreatWarningsAsErrors`, `AnalysisMode=All`.
- `test/Modules/Events/Evently.Modules.Events.IntegrationTests/Abstractions/IntegrationTestWebAppFactory.cs` — Testcontainers setup.
- `test/Modules/Events/Evently.Modules.Events.IntegrationTests/Abstractions/BaseIntegrationTest.cs` — test base class.
- `test/Modules/Events/Evently.Modules.Events.IntegrationTests/Abstractions/IntegrationTestCollection.cs` — xUnit collection.
- `test/Modules/Events/Evently.Modules.Events.IntegrationTests/Abstractions/CommandHelpers.cs` — shared test setup helpers.
- `test/Modules/Events/Evently.Modules.Events.Domain.UnitTests/` — domain unit test patterns.
- `test/Modules/Events/Evently.Modules.Events.ArchitectureTests/` — NetArchTest layer/convention tests.
- `test/Evently.ArchitectureTests/` — cross-module isolation tests.

## TEST FRAMEWORK STACK

| Concern | Library | Version |
|---|---|---|
| Test Runner | xUnit | latest |
| Assertions | FluentAssertions | latest |
| Test Data | Bogus | latest |
| Mocking | NSubstitute | latest |
| Containers | Testcontainers (PostgreSQL, MongoDB, Redis, RabbitMQ) | latest |
| Architecture | NetArchTest.Rules | latest |
| Web Host | `WebApplicationFactory<Program>` from `Microsoft.AspNetCore.Mvc.Testing` | latest |

## TEST TYPE 1: MODULE INTEGRATION TESTS

### Test Infrastructure Setup

Each module has its own integration test project with this structure:

```
test/Modules/{Module}/Evently.Modules.{Module}.IntegrationTests/
├── Abstractions/
│   ├── BaseIntegrationTest.cs
│   ├── CommandHelpers.cs
│   ├── IntegrationTestCollection.cs
│   └── IntegrationTestWebAppFactory.cs
├── {Aggregate}/
│   ├── {Action}{Aggregate}Tests.cs
│   └── ...
└── Evently.Modules.{Module}.IntegrationTests.csproj
```

### IntegrationTestWebAppFactory Pattern

```
- Extend `WebApplicationFactory<Program>` and implement `IAsyncLifetime`
- Start these Testcontainers: PostgreSQL (17.5), MongoDB (8.2), Redis (8.0.2), RabbitMQ (4.1.3-management-alpine)
- Override `ConfigureWebHost` to:
  1. Remove and re-add EnvironmentVariablesConfigurationSource (to isolate from host env)
  2. Replace `IDateTimeProvider` with NSubstitute mock (default: returns `DateTime.UtcNow`)
  3. Set connection strings via environment variables
- ALWAYS use exact container image versions matching existing tests
- ALWAYS make `DateTimeProviderMock` a public readonly field for test access
```

### BaseIntegrationTest Pattern

```
- Mark with `[Collection(nameof(IntegrationTestCollection))]`
- Use `#pragma warning disable CA1515` / `#pragma warning restore CA1515` around the class (it's public but in a test assembly)
- Extend `IDisposable` — dispose the service scope
- Protected fields: `Faker` (static), `Factory`, `Sender` (ISender), `DbContext`
- Constructor: create `IServiceScope`, resolve `ISender` and module's `DbContext`
- `CleanDatabaseAsync()`: execute raw SQL `DELETE` statements for all module tables in dependency order (children first)
- Use schema-qualified table names: `{schema}.{table}` (e.g., `events.events`, `events.ticket_types`)
```

### IntegrationTestCollection Pattern

```csharp
[CollectionDefinition(nameof(IntegrationTestCollection))]
public sealed class IntegrationTestCollection : ICollectionFixture<IntegrationTestWebAppFactory>;
```

### Integration Test Pattern

```
- Class: `public class {Action}{Aggregate}Tests : BaseIntegrationTest`
- Primary constructor: `(IntegrationTestWebAppFactory factory) : base(factory)`
- Follow Arrange / Act / Assert pattern
- Use `Faker` for generating test data
- Send commands/queries via `Sender.Send(new XxxCommand(...))`
- Assert on `Result` properties: `result.IsSuccess.Should().BeTrue()` or `result.Error.Should().Be(XxxErrors.SomeError)`
- For tests requiring preconditions, create helper methods or use `CommandHelpers`
- Call `CleanDatabaseAsync()` when state isolation is needed
- For time-dependent tests, configure `Factory.DateTimeProviderMock.UtcNow.Returns(...)`
```

**Command Test Template:**
```csharp
using Evently.Common.Domain;
using Evently.Modules.{Module}.Application.{Aggregate}.{Action};
using Evently.Modules.{Module}.Domain.{Aggregate};
using Evently.Modules.{Module}.IntegrationTests.Abstractions;
using FluentAssertions;

namespace Evently.Modules.{Module}.IntegrationTests.{Aggregate};

public class {Action}{Aggregate}Tests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Should_ReturnFailure_WhenXxxIsInvalid()
    {
        // Arrange
        var command = new {Action}{Aggregate}Command(/* invalid data using Faker */);

        // Act
        Result result = await Sender.Send(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be({Aggregate}Errors.SomeError);
    }

    [Fact]
    public async Task Should_ReturnSuccess_WhenXxxIsValid()
    {
        // Arrange — create preconditions via other commands
        var command = new {Action}{Aggregate}Command(/* valid data using Faker */);

        // Act
        Result<Guid> result = await Sender.Send(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
    }
}
```

## TEST TYPE 2: DOMAIN UNIT TESTS

### Structure

```
test/Modules/{Module}/Evently.Modules.{Module}.Domain.UnitTests/
├── Abstractions/
│   └── BaseTest.cs
├── {Aggregate}/
│   └── {Aggregate}Tests.cs
└── Evently.Modules.{Module}.Domain.UnitTests.csproj
```

### BaseTest Pattern

```
- `#pragma warning disable CA1515` around the class
- Protected static `Faker Faker = new()`
- Protected helper method: `AssertDomainEventWasPublished<T>(Entity entity)` — asserts entity.DomainEvents contains exactly one event of type T, returns it
```

**BaseTest Template:**
```csharp
using Bogus;
using Evently.Common.Domain;
using FluentAssertions;

namespace Evently.Modules.{Module}.Domain.UnitTests.Abstractions;

#pragma warning disable CA1515
public abstract class BaseTest
#pragma warning restore CA1515
{
    protected static readonly Faker Faker = new();

    protected static T AssertDomainEventWasPublished<T>(Entity entity)
        where T : IDomainEvent
    {
        T? domainEvent = entity.DomainEvents.OfType<T>().SingleOrDefault();
        domainEvent.Should().NotBeNull();
        return domainEvent!;
    }
}
```

### Domain Unit Test Pattern

```
- Test entity factory methods return correct Result
- Test entity mutation methods enforce invariants and return appropriate errors
- Test domain events are raised correctly
- Use `Faker` for generating valid test data
- Pure unit tests — NO infrastructure, NO mocking (test the domain model directly)
```

**Domain Test Template:**
```csharp
using Evently.Common.Domain;
using Evently.Modules.{Module}.Domain.{Aggregate};
using Evently.Modules.{Module}.Domain.UnitTests.Abstractions;
using FluentAssertions;

namespace Evently.Modules.{Module}.Domain.UnitTests.{Aggregate};

public class {Aggregate}Tests : BaseTest
{
    [Fact]
    public void Create_ShouldReturnSuccess_WhenValidData()
    {
        // Arrange
        /* use Faker for data */

        // Act
        Result<{Aggregate}> result = {Aggregate}.Create(/* ... */);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Property.Should().Be(expectedValue);
    }

    [Fact]
    public void Create_ShouldRaiseDomainEvent_WhenCreated()
    {
        // Arrange & Act
        Result<{Aggregate}> result = {Aggregate}.Create(/* ... */);

        // Assert
        {Aggregate}{Action}DomainEvent domainEvent =
            AssertDomainEventWasPublished<{Aggregate}{Action}DomainEvent>(result.Value);
        domainEvent.{Aggregate}Id.Should().Be(result.Value.Id);
    }

    [Fact]
    public void {Method}_ShouldReturnFailure_WhenInvariantViolated()
    {
        // Arrange
        var entity = CreateValid{Aggregate}();

        // Act
        Result result = entity.{Method}(/* invalid args */);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be({Aggregate}Errors.SomeError);
    }
}
```

## TEST TYPE 3: ARCHITECTURE TESTS

### Per-Module Architecture Tests

```
test/Modules/{Module}/Evently.Modules.{Module}.ArchitectureTests/
├── Abstractions/
│   ├── BaseTest.cs
│   └── TestResultExtensions.cs
├── Application/
│   └── ApplicationTests.cs
├── Domain/
│   └── DomainTests.cs
├── Layers/
│   └── LayerTests.cs
├── Presentation/
│   └── PresentationTests.cs
└── Evently.Modules.{Module}.ArchitectureTests.csproj
```

### BaseTest for Architecture Tests

```
- Protected Assembly fields: DomainAssembly, ApplicationAssembly, InfrastructureAssembly, PresentationAssembly
- Load assemblies by name string: `Assembly.Load("Evently.Modules.{Module}.{Layer}")`
```

### TestResultExtensions

```csharp
using FluentAssertions;
using NetArchTest.Rules;

namespace Evently.Modules.{Module}.ArchitectureTests.Abstractions;

internal static class TestResultExtensions
{
    internal static void ShouldBeSuccessful(this TestResult testResult)
        => testResult.FailingTypes?.Should().BeEmpty();
}
```

### Layer Dependency Tests

ALWAYS enforce these rules (refer to `test/Modules/Events/Evently.Modules.Events.ArchitectureTests/Layers/LayerTests.cs`):

1. **Domain** → MUST NOT depend on Application, Infrastructure, or Presentation
2. **Application** → MUST NOT depend on Infrastructure or Presentation
3. **Presentation** → MUST NOT depend on Infrastructure

### Convention Tests

Common convention checks to generate:

**Domain Layer:**
- All domain entities must be `sealed`
- All domain events must be `sealed`
- All domain events must inherit from `DomainEvent`

**Application Layer:**
- All command handlers must be `internal` and `sealed`
- All query handlers must be `internal` and `sealed`
- All validators must be `internal` and `sealed`
- All handlers must have names ending in `Handler`

**Presentation Layer:**
- All endpoints must be `internal` and `sealed`
- All endpoints must implement `IEndpoint`
- All integration event handlers must be `internal` and `sealed`

### Cross-Module Isolation Tests

Located in `test/Evently.ArchitectureTests/Layers/ModuleTests.cs`:
- Each module's assemblies (excluding IntegrationEvents) MUST NOT depend on any other module's namespace
- The ONLY allowed cross-module dependency is on other modules' `IntegrationEvents` namespace

## CODING STYLE FOR TESTS

- ALWAYS use `Faker` for test data — NEVER hardcode magic strings/numbers.
- ALWAYS use FluentAssertions (`.Should().BeTrue()`, `.Should().Be(expected)`, `.Should().NotBeNull()`).
- ALWAYS follow Arrange/Act/Assert with comments separating sections.
- ALWAYS use `[Fact]` for parameterless tests, `[Theory]` + `[InlineData]` for parameterized tests.
- ALWAYS name tests: `{Method}_Should{Expected}_When{Condition}` or `Should_{Expected}_When{Condition}`.
- NEVER use `Assert.True()` or other xUnit assertions — use FluentAssertions exclusively.
- NEVER use `Task.Delay()` for synchronization — use polling or direct state verification.
- Use `async Task` return types for all async tests.
- Use primary constructors for injecting `IntegrationTestWebAppFactory`.

## CHECKLIST — Before Completing

- [ ] Test class follows correct naming convention
- [ ] Test class is in the correct project and namespace
- [ ] BaseTest/BaseIntegrationTest is properly inherited
- [ ] `[Collection]` attribute is applied for integration tests
- [ ] Faker is used for all test data
- [ ] FluentAssertions used exclusively (no xUnit Assert)
- [ ] Arrange/Act/Assert comments present
- [ ] Both success and failure paths are tested
- [ ] Domain events are verified in unit tests where applicable
- [ ] `#pragma warning disable CA1515` used for public test base classes

