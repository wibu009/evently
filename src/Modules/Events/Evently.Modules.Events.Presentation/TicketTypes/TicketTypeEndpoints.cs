using Microsoft.AspNetCore.Routing;

namespace Evently.Modules.Events.Presentation.TicketTypes;

public static class TicketTypeEndpoints
{
    public static void MapEndpoints(IEndpointRouteBuilder app)
    {
        GetTicketType.MapEndpoint(app);
        GetTicketTypes.MapEndpoint(app);
        CreateTicketType.MapEndpoint(app);
        ChangeTicketTypePrice.MapEndpoint(app);
    }
}
