using Evently.Common.Application.Messaging;

namespace Evently.Modules.Events.Application.Events.RemoveEventImage;

public sealed record RemoveEventImageCommand(Guid EventId, Guid ImageId) : ICommand;
