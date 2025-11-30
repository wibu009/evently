using System.Data.Common;
using Dapper;
using Evently.Common.Application.Data;
using Evently.Common.Application.Messaging;
using Evently.Modules.Attendance.Domain.Tickets;

namespace Evently.Modules.Attendance.Application.EventStatistics.Projections;

internal sealed class TicketCreatedDomainEventHandler(
    IDbConnectionFactory dbConnectionFactory,
    IEventStatisticsRepository eventStatisticsRepository)
    : DomainEventHandler<TicketCreatedDomainEvent>
{
    public override async Task Handle(TicketCreatedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync(cancellationToken);
        
        const string sql =
            """
            SELECT COUNT(*)
            FROM attendance.tickets t
            WHERE t.event_id = @EventId
            """;
        
        int ticketCount = await connection.ExecuteScalarAsync<int>(sql, domainEvent);
        
        EventStatistics eventStatistics = await eventStatisticsRepository.GetAsync(domainEvent.EventId, cancellationToken);
        
        if (eventStatistics is null)
        {
            throw new InvalidOperationException($"EventStatistics with id {domainEvent.EventId} not found");
        }
        
        eventStatistics.TicketsSold = ticketCount;
        
        await eventStatisticsRepository.ReplaceAsync(eventStatistics, cancellationToken);
    }
}
