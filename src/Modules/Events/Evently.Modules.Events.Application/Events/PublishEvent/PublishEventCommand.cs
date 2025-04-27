using Evently.Modules.Events.Application.Abstractions.Messaging;

namespace Evently.Modules.Events.Application.Events.PublishEvent;

public sealed record PublishEventCommand(Guid EventId) : ICommand;
