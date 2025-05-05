using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Evently.Common.Infrastructure.Interceptors;

public static class InterceptorsRegistrar
{
    public static void AddInterceptors(this IServiceCollection services)
    {
        services.TryAddSingleton<PublishDomainEventsInterceptor>();
    }
}
