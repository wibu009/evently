using Evently.Common.Application.Messaging;

namespace Evently.Modules.Events.Application.Events.CompleteEvent;

public sealed record CompleteEventCommand(Guid EventId) : ICommand;
