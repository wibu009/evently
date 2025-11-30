namespace Evently.Modules.Attendance.Application.EventStatistics;

public interface IEventStatisticsRepository
{
    Task<EventStatistics?> GetAsync(Guid eventId, CancellationToken cancellationToken = default);

    Task InsertAsync(EventStatistics eventStatistics, CancellationToken cancellationToken = default);

    Task ReplaceAsync(EventStatistics eventStatistics, CancellationToken cancellationToken = default);
}
