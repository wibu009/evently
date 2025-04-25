namespace Evently.Modules.Events.Application.Events.GetEvent;

public sealed record EventResponse(
    Guid Id,
    string Title,
    string Description,
    string Location,
    DateTimeOffset StartAtUtc,
    DateTimeOffset? EndAtUtc);
