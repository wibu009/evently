---
name: module-feature-scaffolder
description: >
  Scaffolds a complete CQRS feature slice (Command or Query) inside an existing Evently modular monolith module.
  Generates all required artifacts across Domain, Application, and Presentation layers following established patterns.
  Trigger phrases: "add feature", "add command", "add query", "add endpoint", "scaffold use case", "new CQRS slice",
  "create command handler", "create query handler", "add API endpoint".
---

# Module Feature Scaffolder — Evently Modular Monolith

You are an expert .NET 9 modular monolith developer. You scaffold complete vertical feature slices
following the CQRS + DDD + Clean Architecture patterns established in this solution.

## CONTEXT — Read These Files First

Before generating ANY code, read and internalize:

- `Directory.Build.props` — enforces `net9.0`, `TreatWarningsAsErrors`, `AnalysisMode=All`, `EnforceCodeStyleInBuild`.
- `src/Common/Evently.Common.Domain/Result.cs` — the `Result` / `Result<T>` monad used everywhere.
- `src/Common/Evently.Common.Domain/Error.cs` — the `Error` record with factory methods (`NotFound`, `Problem`, `Conflict`, `Failure`).
- `src/Common/Evently.Common.Domain/Entity.cs` — base `Entity` class with `RaiseDomainEvent()`.
- `src/Common/Evently.Common.Domain/DomainEvent.cs` — base `DomainEvent` using `Guid.CreateVersion7()`.
- `src/Common/Evently.Common.Application/Messaging/ICommand.cs` — `ICommand` / `ICommand<TResponse>` interfaces.
- `src/Common/Evently.Common.Application/Messaging/IQuery.cs` — `IQuery<TResponse>` interface.
- `src/Common/Evently.Common.Application/Messaging/ICommandHandler.cs` — handler interface.
- `src/Common/Evently.Common.Application/Messaging/IQueryHandler.cs` — handler interface.
- `src/Common/Evently.Common.Presentation/Endpoints/IEndpoint.cs` — the `IEndpoint` contract.
- `src/Common/Evently.Common.Presentation/Results/ApiResults.cs` — `ApiResults.Problem()` helper.
- `src/Common/Evently.Common.Presentation/Results/ResultExtensions.cs` — `result.Match()` extension.

Reference existing features for patterns (pick the module the user targets):
- `src/Modules/Events/Evently.Modules.Events.Application/Events/CreateEvent/` — command pattern exemplar.
- `src/Modules/Events/Evently.Modules.Events.Application/Events/GetEvent/` — query + Dapper pattern exemplar.
- `src/Modules/Events/Evently.Modules.Events.Presentation/Events/CreateEventEndpoint.cs` — endpoint exemplar.

## ARCHITECTURE RULES — NEVER VIOLATE

1. **Layer Dependencies** (enforced by NetArchTest in `test/`):
   - `Domain` → ONLY depends on `Evently.Common.Domain`. NEVER reference Application, Infrastructure, or Presentation.
   - `Application` → depends on `Common.Application` + own `Domain` + own `IntegrationEvents`. NEVER reference Infrastructure or Presentation.
   - `Presentation` → depends on `Common.Presentation` + own `Application`. NEVER reference Infrastructure.
   - `Infrastructure` is the composition root — you do NOT create files there for feature slices (only for repository implementations or DB config).

2. **Module Isolation** (enforced by cross-module architecture tests):
   - A module MUST NEVER reference another module's Domain, Application, Infrastructure, or Presentation.
   - Cross-module communication is ONLY allowed through `IntegrationEvents` projects.

## COMMAND SLICE — Full Artifact List

When the user asks to add a command (write operation), generate these files:

### 1. Command Record — `{Module}.Application/{Aggregate}/{Action}/{Action}{Aggregate}Command.cs`

```
- ALWAYS `public sealed record`
- Implement `ICommand` (void result) or `ICommand<TResponse>` (e.g., `ICommand<Guid>`)
- Import from `Evently.Common.Application.Messaging`
- Namespace: `Evently.Modules.{Module}.Application.{Aggregate}.{Action}`
```

### 2. Command Handler — `{Module}.Application/{Aggregate}/{Action}/{Action}{Aggregate}CommandHandler.cs`

```
- ALWAYS `internal sealed class` with primary constructor for DI
- Implement `ICommandHandler<TCommand>` or `ICommandHandler<TCommand, TResponse>`
- Return `Result` or `Result<TResponse>` — NEVER throw exceptions for business rule violations
- Use repository interfaces from Domain (e.g., `IEventRepository`)
- Use `IUnitOfWork` from Application Abstractions for `SaveChangesAsync`
- Use `IDateTimeProvider` from `Evently.Common.Application.Clock` for time — NEVER use `DateTime.UtcNow` directly
- Pattern: validate → load aggregates → call domain method → check result → persist → return
```

### 3. Command Validator — `{Module}.Application/{Aggregate}/{Action}/{Action}{Aggregate}CommandValidator.cs`

```
- ALWAYS `internal sealed class`
- Extend `AbstractValidator<TCommand>` from FluentValidation
- Define rules in the constructor
- ALWAYS validate: `.NotEmpty()` for required Guids and strings
- For optional fields, use `.Must().When()` pattern
```

### 4. Endpoint — `{Module}.Presentation/{Aggregate}/{Action}{Aggregate}Endpoint.cs`

```
- ALWAYS `internal sealed class` implementing `IEndpoint`
- Define `MapEndpoint(IEndpointRouteBuilder app)` method
- Use `app.MapPost(...)`, `app.MapPut(...)`, or `app.MapDelete(...)` as appropriate
- Chain: `.RequireAuthorization(Permissions.Xxx)` → `.WithTags(Tags.Xxx)` → `.WithName("...")` → `.Produces<T>()` → `.ProducesProblem()` → `.WithSummary("...")` → `.WithDescription("...")`
- For commands returning data: `result.Match(Results.Ok, ApiResults.Problem)`
- For commands returning void: `result.Match(Results.NoContent, ApiResults.Problem)`
- Define a `private sealed record Request(...)` inside the endpoint class for request binding
- Import: `Evently.Common.Domain`, `Evently.Common.Presentation.Endpoints`, `Evently.Common.Presentation.Results`, `MediatR`, `Microsoft.AspNetCore.Builder`, `Microsoft.AspNetCore.Http`, `Microsoft.AspNetCore.Routing`
```

### 5. (If new aggregate) Domain Entity — `{Module}.Domain/{Aggregate}/{Aggregate}.cs`

```
- ALWAYS `public sealed class` extending `Entity`
- Private parameterless constructor for EF Core
- Properties with `private set` or `private init`
- Static factory method returning `Result<T>` — validate invariants, raise domain event
- Use `Guid.CreateVersion7()` for IDs
- Mutation methods return `Result` for fallible operations
```

### 6. (If new aggregate) Errors — `{Module}.Domain/{Aggregate}/{Aggregate}Errors.cs`

```
- ALWAYS `public static class`
- Parameterized `NotFound` method: `public static Error NotFound(Guid id) => Error.NotFound("...", $"... with id {id} not found")`
- Static `readonly` fields for business rule errors using `Error.Problem("Code", "Description")`
- Error code format: `{Aggregate}.{ErrorName}` (e.g., `Events.NotDraft`)
```

### 7. (If new aggregate) Repository Interface — `{Module}.Domain/{Aggregate}/I{Aggregate}Repository.cs`

```
- Defined in Domain layer
- Async methods with CancellationToken
- Common: `GetAsync(Guid id, CancellationToken)`, `Insert(T entity)`
```

### 8. Permissions — Update `{Module}.Presentation/Permissions.cs`

```
- Add new `internal const string` entries
- Format: `"{resource}:{action}"` (e.g., `"events:update"`, `"events:read"`)
```

### 9. Tags — Update `{Module}.Presentation/Tags.cs` (if new aggregate)

```
- Add new `internal const string` entry matching the aggregate name
```

## QUERY SLICE — Full Artifact List

When the user asks to add a query (read operation), generate:

### 1. Query Record — `{Module}.Application/{Aggregate}/{Action}/{Action}Query.cs`

```
- ALWAYS `public sealed record`
- Implement `IQuery<TResponse>`
- Include filter/parameter properties as constructor args
```

### 2. Response Record — `{Module}.Application/{Aggregate}/{Action}/{Aggregate}Response.cs`

```
- ALWAYS `public sealed record`
- Flat DTO — NO domain entities in the response
- For nested collections, use `List<TChildResponse>` with `{ get; } = []`
```

### 3. Query Handler — `{Module}.Application/{Aggregate}/{Action}/{Action}QueryHandler.cs`

```
- ALWAYS `internal sealed class` with primary constructor
- Implement `IQueryHandler<TQuery, TResponse>`
- Inject `IDbConnectionFactory` — NEVER use DbContext for reads
- Use Dapper raw SQL with `connection.QueryAsync<T>(...)`
- Use snake_case column names (e.g., `e.start_at_utc`)
- Use schema-qualified table names (e.g., `events.events`, `ticketing.orders`)
- Map columns using `nameof(TResponse.Property)` with `AS` aliases
- For multi-table joins with collections, use the Dapper multi-map + dictionary pattern
- Return `Result.Failure<T>(XxxErrors.NotFound(id))` for missing entities — NEVER return null
```

### 4. Endpoint — same pattern as command endpoints, but use `app.MapGet(...)` and `Results.Ok`

## CODING STYLE — ABSOLUTE CONSTRAINTS

- ALWAYS use `sealed` on classes (commands, handlers, endpoints, validators, entities, domain events).
- ALWAYS use `internal` visibility for handlers, validators, and endpoints.
- ALWAYS use primary constructors for dependency injection in handlers.
- ALWAYS use `record` types for commands, queries, responses, errors, and domain events.
- ALWAYS use collection expressions: `[]` for empty, `[.. items]` for spread.
- ALWAYS use raw string literals (`"""..."""`) for multi-line SQL.
- ALWAYS use `Guid.CreateVersion7()` for new entity IDs.
- NEVER use `DateTime.UtcNow` directly — inject `IDateTimeProvider`.
- NEVER throw exceptions for business rule violations — return `Result.Failure(...)`.
- NEVER put `using` statements inside namespaces — use file-scoped namespaces.
- NEVER use `#nullable disable` — nullable reference types are always enabled.
- ALWAYS add the `#pragma warning disable CA1515` / `#pragma warning restore CA1515` pair around non-internal public test base classes only.
- OpenAPI metadata is MANDATORY on every endpoint: `.WithName()`, `.WithSummary()`, `.WithDescription()`, `.Produces<>()`, `.ProducesProblem()`.

## NAMING CONVENTIONS

| Artifact | Pattern | Example |
|---|---|---|
| Command | `{Action}{Aggregate}Command` | `CreateEventCommand` |
| Command Handler | `{Action}{Aggregate}CommandHandler` | `CreateEventCommandHandler` |
| Command Validator | `{Action}{Aggregate}CommandValidator` | `CreateEventCommandValidator` |
| Query | `{Action}{Aggregate}Query` or `{Action}{Aggregates}Query` | `GetEventQuery`, `GetEventsQuery` |
| Query Handler | `{Action}{Aggregate}QueryHandler` | `GetEventQueryHandler` |
| Response DTO | `{Aggregate}Response` | `EventResponse` |
| Endpoint | `{Action}{Aggregate}Endpoint` | `CreateEventEndpoint` |
| Domain Event | `{Aggregate}{Action}DomainEvent` | `EventCreatedDomainEvent` |
| Integration Event | `{Aggregate}{Action}IntegrationEvent` | `EventPublishedIntegrationEvent` |
| Errors class | `{Aggregate}Errors` | `EventErrors` |
| Repository | `I{Aggregate}Repository` | `IEventRepository` |
| Folder | `{Action}{Aggregate}` (under Application) | `CreateEvent/` |

## EXAMPLE INTERACTION

User: "Add a feature to update an event's description in the Events module"

You generate:
1. `src/Modules/Events/Evently.Modules.Events.Application/Events/UpdateEventDescription/UpdateEventDescriptionCommand.cs`
2. `src/Modules/Events/Evently.Modules.Events.Application/Events/UpdateEventDescription/UpdateEventDescriptionCommandHandler.cs`
3. `src/Modules/Events/Evently.Modules.Events.Application/Events/UpdateEventDescription/UpdateEventDescriptionCommandValidator.cs`
4. `src/Modules/Events/Evently.Modules.Events.Presentation/Events/UpdateEventDescriptionEndpoint.cs`
5. Update `Event.cs` domain entity with a new `UpdateDescription(string description)` method returning `Result`.
6. Add `EventDescriptionUpdatedDomainEvent.cs` if the event is significant.
7. Update `Permissions.cs` if a new permission is needed (reuse `ModifyEvents` if appropriate).

