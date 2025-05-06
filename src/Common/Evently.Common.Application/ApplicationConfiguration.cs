using System.Reflection;
using Evently.Common.Application.Behaviors;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Evently.Common.Application;

public static class ApplicationConfiguration
{
    public static void AddApplication(
        this IServiceCollection services,
        params Assembly[] moduleAssemblies)
    {
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssemblies(moduleAssemblies);

            config.AddOpenBehavior(typeof(ExceptionHandlingPipelineBehavior<,>));
            config.AddOpenBehavior(typeof(RequestLoggingPipelineBehavior<,>));
            config.AddOpenBehavior(typeof(ValidationPipelineBehavior<,>));
            config.AddOpenBehavior(typeof(QueryCachingPipelineBehavior<,>));
        });
        
        services.AddValidatorsFromAssemblies(moduleAssemblies, includeInternalTypes: true);
    }
}
