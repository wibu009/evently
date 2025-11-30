using Evently.Common.Infrastructure.Data;
using Evently.Modules.Attendance.Application.EventStatistics;
using Evently.Modules.Attendance.Domain.Events;

namespace Evently.Modules.Attendance.Infrastructure.Events;

internal sealed class EventStatisticsConfiguration : IDocumentConfiguration<EventStatistics>
{
    public void Configure(DocumentDataBuilder<EventStatistics> dataBuilder)
    {
        dataBuilder.ToCollection("event-statistics");
        dataBuilder.MapId(e => e.EventId);

        dataBuilder.IndexAscending(e => e.EventId); 
    }
}
