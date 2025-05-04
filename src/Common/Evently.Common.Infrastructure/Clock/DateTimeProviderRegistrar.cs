using Evently.Common.Application.Clock;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Evently.Common.Infrastructure.Clock;

internal static class DateTimeProviderRegistrar
{
    public static void AddDateTimeProvider(this IServiceCollection services)
    {
        services.TryAddSingleton<IDateTimeProvider, DateTimeProvider>();
    }
}
