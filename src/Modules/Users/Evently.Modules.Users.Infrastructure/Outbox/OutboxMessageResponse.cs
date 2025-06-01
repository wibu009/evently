namespace Evently.Modules.Users.Infrastructure.Outbox;

internal sealed record OutboxMessageResponse(Guid Id, string Content);