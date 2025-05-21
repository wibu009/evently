using Evently.Common.Application.Messaging;

namespace Evently.Modules.Attendance.Application.Events.CreateEvent;

public sealed record CreateEventCommand(
    Guid EventId,
    string Title,
    string Description,
    string Location,
    DateTime StartAtUtc,
    DateTime? EndAtUtc) : ICommand;
