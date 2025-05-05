using Evently.Common.Application.Messaging;

namespace Evently.Modules.Events.Application.Events.RescheduleEvent;

public sealed record RescheduleEventCommand(Guid EventId, DateTime StartAtUtc, DateTime? EndAtUtc) : ICommand;
