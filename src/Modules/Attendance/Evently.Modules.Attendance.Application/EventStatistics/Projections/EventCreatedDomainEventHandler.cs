using System.Data.Common;
using Dapper;
using Evently.Common.Application.Data;
using Evently.Common.Application.Messaging;
using Evently.Modules.Attendance.Domain.Events;

namespace Evently.Modules.Attendance.Application.EventStatistics.Projections;

internal sealed class EventCreatedDomainEventHandler(
    IEventStatisticsRepository eventStatisticsRepository)
    : DomainEventHandler<EventCreatedDomainEvent>
{
    public override async Task Handle(EventCreatedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await eventStatisticsRepository.InsertAsync(
            EventStatistics.Create(
                domainEvent.EventId,
                domainEvent.Title,
                domainEvent.Description,
                domainEvent.Location,
                domainEvent.StartAtUtc,
                domainEvent.EndAtUtc),
            cancellationToken);
    }
}
