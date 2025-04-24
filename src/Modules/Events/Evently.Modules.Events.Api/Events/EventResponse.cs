namespace Evently.Modules.Events.Api.Events;

public sealed record EventResponse(
    Guid Id,
    string Title,
    string Description,
    string Location,
    DateTimeOffset StartAtUtc,
    DateTimeOffset? EndAtUtc);
