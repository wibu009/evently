using Evently.Common.Infrastructure.Data;
using Evently.Modules.Attendance.Infrastructure.Events;
using MongoDB.Driver;

namespace Evently.Modules.Attendance.Infrastructure.Database;

public sealed class AttendanceDocumentDataStore : DocumentDataStore
{
    public AttendanceDocumentDataStore(IMongoClient client, string databaseName)
        : base(client, databaseName)
    {
        RegisterConfiguration(new EventStatisticsConfiguration());
    }
}
