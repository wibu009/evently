using Evently.Common.Application.Messaging;

namespace Evently.Modules.Events.Application.Events.SetCoverImage;

public sealed record SetCoverImageCommand(Guid EventId, Guid ImageId) : ICommand;
