using Evently.Common.Application.Messaging;

namespace Evently.Modules.Ticketing.Application.Events.RescheduleEvent;

public sealed record RescheduleEventCommand(Guid EventId, DateTime StartAtUtc, DateTime? EndAtUtc) : ICommand;
