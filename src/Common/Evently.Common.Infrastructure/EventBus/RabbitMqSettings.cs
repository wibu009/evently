namespace Evently.Common.Infrastructure.EventBus;

public sealed class RabbitMqSettings(
    string host,
    string username = "guest",
    string password = "guest")
{
    public string Host { get; init; } = host;
    public string Username { get; init; } = username;
    public string Password { get; init; } = password;
}
