using Evently.Common.Domain;

namespace Evently.Modules.Attendance.Domain.Attendees;

public static class AttendeeErrors
{
    public static Error NotFound(Guid attendeeId) => Error.NotFound("Attendees.NotFound", $"Attendee with id {attendeeId} not found");
}