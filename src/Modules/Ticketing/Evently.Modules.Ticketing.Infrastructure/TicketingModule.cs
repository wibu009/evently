using Evently.Modules.Ticketing.Application.Carts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Evently.Modules.Ticketing.Infrastructure;

public static class TicketingModule
{
    public static void AddTicketingModule( this IServiceCollection services, IConfiguration configuration)
    {
        services.AddInfrastructure(configuration);
    }
    
#pragma warning disable S1172
#pragma warning disable IDE0060
    private static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
#pragma warning restore IDE0060
#pragma warning restore S1172
    {
        services.AddSingleton<CartService>();
    }
}
