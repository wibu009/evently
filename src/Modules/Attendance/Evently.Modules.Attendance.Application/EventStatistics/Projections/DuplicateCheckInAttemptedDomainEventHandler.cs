using Evently.Common.Application.Messaging;
using Evently.Modules.Attendance.Domain.Attendees;

namespace Evently.Modules.Attendance.Application.EventStatistics.Projections;

internal sealed class DuplicateCheckInAttemptedDomainEventHandler(
    IEventStatisticsRepository eventStatisticsRepository)
    : DomainEventHandler<DuplicateCheckInAttemptedDomainEvent>
{
    public override async Task Handle(DuplicateCheckInAttemptedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        EventStatistics eventStatistics =
            await eventStatisticsRepository.GetAsync(domainEvent.EventId, cancellationToken);
        
        if (eventStatistics is null)
        {
            throw new InvalidOperationException($"EventStatistics with id {domainEvent.EventId} not found");
        }

        eventStatistics.DuplicateCheckInTickets.Add(new TicketModel
        {
            AttendeeId = domainEvent.AttendeeId,
            EventId = domainEvent.EventId,
            TicketId = domainEvent.TicketId,
            TicketCode = domainEvent.TicketCode
        });

        await eventStatisticsRepository.ReplaceAsync(eventStatistics, cancellationToken);
    }
}
