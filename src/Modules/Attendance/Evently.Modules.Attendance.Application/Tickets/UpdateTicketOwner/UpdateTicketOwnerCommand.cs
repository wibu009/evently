using Evently.Common.Application.Messaging;

namespace Evently.Modules.Attendance.Application.Tickets.UpdateTicketOwner;

public sealed record UpdateTicketOwnerCommand(Guid TicketId, Guid AttendeeId) : ICommand;
