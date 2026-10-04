using Evently.Common.Application.Messaging;

namespace Evently.Modules.Attendance.Application.Attendees.CheckInTicketByCode;

public sealed record CheckInTicketByCodeCommand(string TicketCode) : ICommand<CheckInTicketByCodeResponse>;
