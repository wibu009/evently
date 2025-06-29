namespace Evently.Modules.Attendance.Application.EventStatistics.GetEventStatistics;

public sealed record EventStatisticsResponse(
    Guid EventId,
    string Title,
    string Description,
    string Location,
    DateTime StartAtUtc,
    DateTime? EndAtUtc,
    int TicketsSold,
    int AttendeesCheckedIn,
    string[] DuplicateCheckInTickets,
    string[] InvalidCheckInTickets);
