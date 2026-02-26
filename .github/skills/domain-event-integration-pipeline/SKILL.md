---
name: domain-event-integration-pipeline
description: >
  Designs and implements the full domain event to integration event pipeline in the Evently modular monolith.
  Covers domain events, domain event handlers, integration events, integration event handlers, Outbox/Inbox wiring,
  MassTransit consumer registration, and idempotent handler decoration.
  Trigger phrases: "add domain event", "add integration event", "publish event across modules",
  "cross-module communication", "event-driven", "outbox pattern", "inbox pattern",
  "MassTransit consumer", "saga", "domain event handler", "integration event handler".
---

# Domain Event & Integration Pipeline — Evently Modular Monolith

You are an expert in event-driven architecture within .NET modular monoliths. You implement the full
domain event → outbox → integration event → inbox → cross-module handler pipeline used in this solution.

## CONTEXT — Read These Files First

Before generating ANY code, read and internalize:

- `src/Common/Evently.Common.Domain/IDomainEvent.cs` — marker interface.
- `src/Common/Evently.Common.Domain/DomainEvent.cs` — base class with `Guid.CreateVersion7()` and `DateTime.UtcNow`.
- `src/Common/Evently.Common.Domain/Entity.cs` — `RaiseDomainEvent()` method.
- `src/Common/Evently.Common.Application/Messaging/IDomainEventHandler.cs` — handler contract.
- `src/Common/Evently.Common.Application/Messaging/DomainEventHandler.cs` — abstract base handler.
- `src/Common/Evently.Common.Application/EventBus/IEventBus.cs` — `PublishAsync<T>()` contract.
- `src/Common/Evently.Common.Application/EventBus/IIntegrationEvent.cs` — marker interface.
- `src/Common/Evently.Common.Application/EventBus/IntegrationEvent.cs` — base class.
- `src/Common/Evently.Common.Application/EventBus/IntegrationEventHandler.cs` — abstract base handler.
- `src/Common/Evently.Common.Infrastructure/Outbox/` — `InsertOutboxMessagesInterceptor`, `OutboxOptions`, `ConfigureProcessOutboxJob`.
- `src/Common/Evently.Common.Infrastructure/Inbox/` — `InboxOptions`, `ConfigureProcessInboxJob`.

Reference existing implementations:
- `src/Modules/Events/Evently.Modules.Events.Domain/Events/EventPublishedDomainEvent.cs`
- `src/Modules/Events/Evently.Modules.Events.Application/Events/PublishEvent/EventPublishedDomainEventHandler.cs`
- `src/Modules/Events/Evently.Modules.Events.IntegrationEvents/Events/EventPublishedIntegrationEvent.cs`
- `src/Modules/Ticketing/Evently.Modules.Ticketing.Presentation/Events/EventPublishedIntegrationEventHandler.cs`
- `src/Modules/Events/Evently.Modules.Events.Infrastructure/EventsModule.cs` — MassTransit + idempotent handler registration.

## THE FULL EVENT PIPELINE

```
Entity.RaiseDomainEvent()
    ↓
InsertOutboxMessagesInterceptor (EF SaveChanges) — serializes to outbox_messages table
    ↓
Quartz ProcessOutboxJob — dispatches via MediatR
    ↓
DomainEventHandler (in Application layer of SAME module)
    ↓
IEventBus.PublishAsync() — publishes IntegrationEvent via MassTransit/RabbitMQ
    ↓
IntegrationEventConsumer<T> (MassTransit consumer wrapper)
    ↓
Inbox — stores for idempotent processing
    ↓
IntegrationEventHandler (in Presentation layer of CONSUMING module)
    ↓
ISender.Send(new XxxCommand(...)) — triggers command in consuming module
```

## DOMAIN EVENT — Generation Rules

File: `{Module}.Domain/{Aggregate}/{Aggregate}{Action}DomainEvent.cs`

```
- ALWAYS `public sealed class` extending `DomainEvent`
- Use primary constructor with relevant data (typically just the aggregate ID)
- Properties with `{ get; init; }` pattern
- Namespace: `Evently.Modules.{Module}.Domain.{Aggregate}`
- NEVER include large payloads — the handler will query for full data
```

**Template:**
```csharp
using Evently.Common.Domain;

namespace Evently.Modules.{Module}.Domain.{Aggregate};

public sealed class {Aggregate}{Action}DomainEvent(Guid {aggregate}Id) : DomainEvent
{
    public Guid {Aggregate}Id { get; init; } = {aggregate}Id;
}
```

## DOMAIN EVENT HANDLER — Generation Rules

File: `{Module}.Application/{Aggregate}/{Action}{Aggregate}/{Aggregate}{Action}DomainEventHandler.cs`

```
- ALWAYS `internal sealed class` with primary constructor
- Extend `DomainEventHandler<TDomainEvent>`
- Override `Handle(TDomainEvent domainEvent, CancellationToken cancellationToken = default)`
- Pattern A (simple relay): Directly publish an IntegrationEvent via `IEventBus`
- Pattern B (enriched relay): Query for full data via `ISender.Send(new GetXxxQuery(...))`, then publish enriched IntegrationEvent
- For Pattern B: throw `EventlyException` if the query fails (this is infrastructure-level, not business logic)
- NEVER catch and swallow exceptions — let the Outbox retry mechanism handle transient failures
```

**Pattern A — Simple Relay:**
```csharp
internal sealed class {Aggregate}{Action}DomainEventHandler(IEventBus eventBus)
    : DomainEventHandler<{Aggregate}{Action}DomainEvent>
{
    public override async Task Handle(
        {Aggregate}{Action}DomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        await eventBus.PublishAsync(
            new {Aggregate}{Action}IntegrationEvent(
                domainEvent.Id,
                domainEvent.OccurredOnUtc,
                domainEvent.{Aggregate}Id),
            cancellationToken);
    }
}
```

**Pattern B — Enriched Relay:**
```csharp
internal sealed class {Aggregate}{Action}DomainEventHandler(ISender sender, IEventBus eventBus)
    : DomainEventHandler<{Aggregate}{Action}DomainEvent>
{
    public override async Task Handle(
        {Aggregate}{Action}DomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        Result<{Aggregate}Response> result = await sender.Send(
            new Get{Aggregate}Query(domainEvent.{Aggregate}Id), cancellationToken);

        if (result.IsFailure)
        {
            throw new EventlyException(nameof(Get{Aggregate}Query), result.Error);
        }

        await eventBus.PublishAsync(
            new {Aggregate}{Action}IntegrationEvent(
                domainEvent.Id,
                domainEvent.OccurredOnUtc,
                result.Value.Id,
                /* ... mapped properties ... */),
            cancellationToken);
    }
}
```

## INTEGRATION EVENT — Generation Rules

File: `{Module}.IntegrationEvents/{Aggregate}/{Aggregate}{Action}IntegrationEvent.cs`

```
- ALWAYS `public sealed class` extending `IntegrationEvent`
- Use primary constructor with ALL data the consuming module needs (it cannot query back)
- Properties with `{ get; init; }` pattern
- Project: `Evently.Modules.{Module}.IntegrationEvents` — this is a SHARED CONTRACT project
- This project has MINIMAL dependencies — only `Evently.Common.Application`
- NEVER reference Domain, Infrastructure, or Presentation from this project
- For complex nested data, define model classes in the IntegrationEvents project (e.g., `TicketTypeModel`)
```

## INTEGRATION EVENT HANDLER — Generation Rules

File: `{ConsumingModule}.Presentation/{SourceAggregate}/{Aggregate}{Action}IntegrationEventHandler.cs`

```
- ALWAYS `internal sealed class` with primary constructor
- Extend `IntegrationEventHandler<TIntegrationEvent>`
- Lives in the CONSUMING module's Presentation layer (NOT the publishing module)
- Override `Handle(TIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)`
- Translate the integration event into a local command: `sender.Send(new LocalCommand(...))`
- Throw `EventlyException` if the command fails
- NEVER directly access the publishing module's domain — always go through your own Application layer
```

**Template:**
```csharp
using Evently.Common.Application.EventBus;
using Evently.Common.Application.Exceptions;
using Evently.Common.Domain;
using Evently.Modules.{SourceModule}.IntegrationEvents.{Aggregate};
using Evently.Modules.{ConsumingModule}.Application.{LocalAggregate}.{LocalAction};
using MediatR;

namespace Evently.Modules.{ConsumingModule}.Presentation.{SourceAggregate};

internal sealed class {Aggregate}{Action}IntegrationEventHandler(ISender sender)
    : IntegrationEventHandler<{Aggregate}{Action}IntegrationEvent>
{
    public override async Task Handle(
        {Aggregate}{Action}IntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        Result result = await sender.Send(
            new {LocalAction}Command(
                integrationEvent.{Aggregate}Id,
                /* mapped properties */),
            cancellationToken);

        if (result.IsFailure)
        {
            throw new EventlyException(nameof({LocalAction}Command), result.Error);
        }
    }
}
```

## DI WIRING — What Gets Registered Automatically

You do NOT need to manually register most things. Understand the automatic registration flow in `{Module}Module.cs`:

### Domain Event Handlers (in `AddApplication()`):
- Scanned from the Application assembly via `IDomainEventHandler` interface
- Each handler is registered as `Scoped`
- Each handler is decorated with `IdempotentDomainEventHandler<>` via Scrutor — ensures at-least-once + idempotent processing

### Integration Event Handlers (in `AddPresentation()`):
- Scanned from the Presentation assembly via `IIntegrationEventHandler` interface
- For each handler, the corresponding `IntegrationEventConsumer<>` is registered with MassTransit
- Each handler is registered as `Scoped`
- Each handler is decorated with `IdempotentIntegrationEventHandler<>` via Scrutor

### What YOU Must Do:
1. Create the domain event class in Domain
2. Raise it from the entity via `RaiseDomainEvent()`
3. Create the domain event handler in Application
4. Create the integration event class in IntegrationEvents project
5. Create the integration event handler in the consuming module's Presentation layer
6. VERIFY: The consuming module's `.csproj` has a `ProjectReference` to the source module's IntegrationEvents project

## MODULE ISOLATION RULES — NEVER VIOLATE

- The publishing module's IntegrationEvents project is the ONLY shared contract.
- The consuming module references ONLY `{SourceModule}.IntegrationEvents` — NEVER `{SourceModule}.Domain`, `.Application`, `.Infrastructure`, or `.Presentation`.
- Integration event handlers live in the CONSUMING module's `Presentation` layer.
- Domain event handlers live in the PUBLISHING module's `Application` layer.

## SAGA PATTERN (Advanced)

For orchestrated multi-step cross-module workflows, follow the `CancelEventSaga` pattern:

- File: `{Module}.Presentation/{Aggregate}/{Saga}Saga.cs` + `{Saga}State.cs`
- Use MassTransit `MassTransitStateMachine<TState>`
- Register in `AddPresentation()` via `cfg.AddSagaStateMachine<TSaga, TState>()`
- State persistence: Redis (primary) with InMemory fallback
- Saga reacts to integration events and publishes new integration events to coordinate

## CHECKLIST — Before Completing

- [ ] Domain event raised inside entity mutation method
- [ ] Domain event handler in Application layer publishes integration event
- [ ] Integration event class in IntegrationEvents project with full data payload
- [ ] Integration event handler in consuming module's Presentation layer
- [ ] Consuming module `.csproj` references source module's IntegrationEvents project
- [ ] All classes are `internal sealed` (except domain events and integration events which are `public sealed`)
- [ ] No cross-module references beyond IntegrationEvents

