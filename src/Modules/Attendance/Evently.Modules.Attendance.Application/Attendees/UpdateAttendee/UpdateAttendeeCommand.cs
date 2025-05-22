using Evently.Common.Application.Messaging;

namespace Evently.Modules.Attendance.Application.Attendees.UpdateAttendee;

public sealed record UpdateAttendeeCommand(Guid Id, string FirstName, string LastName) : ICommand;
