using Microsoft.Extensions.Configuration;

namespace Evently.Common.Infrastructure.Configuration;

public static class ConfigurationExtensions
{
    public static string GetConnectionStringOrThrow(this IConfiguration configuration, string name) =>
        configuration.GetConnectionString(name) ?? throw new InvalidOperationException($"The connection string {name} was not found");

    public static T GetValueOrThrow<T>(this IConfiguration configuration, string name) =>
        configuration.GetValue<T?>(name) ?? throw new InvalidOperationException($"The value {name} was not found");
}
