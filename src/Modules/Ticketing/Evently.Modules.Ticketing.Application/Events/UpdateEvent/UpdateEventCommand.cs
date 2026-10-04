using Evently.Common.Application.Messaging;

namespace Evently.Modules.Ticketing.Application.Events.UpdateEvent;

public sealed record UpdateEventCommand(
    Guid EventId,
    string Title,
    string Description,
    string Location) : ICommand;
