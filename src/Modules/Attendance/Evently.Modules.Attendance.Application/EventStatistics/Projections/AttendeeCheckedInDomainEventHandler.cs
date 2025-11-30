using System.Data.Common;
using Dapper;
using Evently.Common.Application.Data;
using Evently.Common.Application.Messaging;
using Evently.Modules.Attendance.Domain.Attendees;

namespace Evently.Modules.Attendance.Application.EventStatistics.Projections;

internal sealed class AttendeeCheckedInDomainEventHandler(
    IDbConnectionFactory dbConnectionFactory,
    IEventStatisticsRepository eventStatisticsRepository)
    : DomainEventHandler<AttendeeCheckedInDomainEvent>
{
    public override async Task Handle(AttendeeCheckedInDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync(cancellationToken);
        
        const string sql =
            """
            SELECT COUNT(*)
            FROM attendance.tickets t
            WHERE
                t.event_id = @EventId AND
                t.used_at_utc IS NOT NULL
            """;
        
        int attendeeCount = await connection.ExecuteScalarAsync<int>(sql, domainEvent);
        
        EventStatistics eventStatistics = await eventStatisticsRepository.GetAsync(domainEvent.EventId, cancellationToken);
        if (eventStatistics is null)
        {
            throw new InvalidOperationException($"EventStatistics with id {domainEvent.EventId} not found");
        }
        
        eventStatistics.AttendeesCheckedIn = attendeeCount;
        
        await eventStatisticsRepository.ReplaceAsync(eventStatistics, cancellationToken);
    }
}
