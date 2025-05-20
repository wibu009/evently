using Evently.Common.Application.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace Evently.Common.Infrastructure.Authentication;

internal static class AuthenticationExtensions
{
    internal static void AddAuthenticationInternal(this IServiceCollection services)
    {
        services.AddAuthorization();
        services.AddAuthentication().AddJwtBearer();
        services.AddHttpContextAccessor();

        services.ConfigureOptions<JwtBearerConfigureOptions>();

        services.AddScoped<ICurrentActor, CurrentActor>();
    }
}
