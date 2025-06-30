using Evently.Modules.Events.IntegrationEvents.Events;
using Evently.Modules.Ticketing.IntegrationEvents.Events;
using MassTransit;

namespace Evently.Modules.Events.Presentation.Events;

public sealed class CancelEventSaga : MassTransitStateMachine<CancelEventState>
{
    public State CancellationStarted { get; private set; }
    public State PaymentsRefunded { get; private set; }
    public State TicketsArchived { get; private set; }
    
    public Event<EventCanceledIntegrationEvent> EventCanceled { get; private set; }
    public Event<EventPaymentsRefundedIntegrationEvent> EventPaymentsRefunded { get; private set; }
    public Event<EventTicketsArchivedIntegrationEvent> EventTicketsArchived { get; private set; }
    public Event EventCancellationCompleted { get; private set; }

    public CancelEventSaga()
    {
        Event(() => EventCanceled, c => c.CorrelateById(m => m.Message.EventId));
        Event(() => EventPaymentsRefunded, c => c.CorrelateById(m => m.Message.EventId));
        Event(() => EventTicketsArchived, c => c.CorrelateById(m => m.Message.EventId));
        
        InstanceState(s => s.CurrentState);

        Initially(
            When(EventCanceled)
                .Publish(context => new EventCancellationStartedIntegrationEvent(
                    context.Message.Id,
                    context.Message.OccurredOnUtc,
                    context.Message.EventId))
                .TransitionTo(CancellationStarted));
        
        During(CancellationStarted, 
            When(EventPaymentsRefunded).TransitionTo(PaymentsRefunded),
            When(EventTicketsArchived).TransitionTo(TicketsArchived));
        
        During(PaymentsRefunded,
            When(EventTicketsArchived).TransitionTo(TicketsArchived));
        
        During(TicketsArchived,
            When(EventPaymentsRefunded).TransitionTo(PaymentsRefunded));

        CompositeEvent(() => EventCancellationCompleted,
            state => state.CancellationCompletedStatus,
            EventPaymentsRefunded, EventTicketsArchived);
        
        DuringAny(
            When(EventCancellationCompleted)
                .Publish(context => new EventCancellationCompletedIntegrationEvent(
                    Guid.CreateVersion7(), 
                    DateTime.UtcNow, 
                    context.Saga.CorrelationId))
                .Finalize());
    }
}

public sealed class CancelEventState : SagaStateMachineInstance, ISagaVersion
{
    public Guid CorrelationId { get; set; }
    public int Version { get; set; }
    public string CurrentState { get; set; }
    public int CancellationCompletedStatus { get; set; }
}
