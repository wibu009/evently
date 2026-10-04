namespace Evently.Modules.Attendance.Application.Attendees.CheckInTicketByCode;

/// <summary>
/// The gate result of a QR scan. Duplicates and invalid codes are explicit
/// outcomes (not errors) so scanners can keep flowing without retry storms.
/// </summary>
public sealed record CheckInTicketByCodeResponse(
    string Outcome,
    Guid? AttendeeId,
    Guid? EventId);
