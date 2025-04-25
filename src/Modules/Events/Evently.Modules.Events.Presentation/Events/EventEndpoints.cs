using Microsoft.AspNetCore.Routing;

namespace Evently.Modules.Events.Presentation.Events;

public sealed class EventEndpoints
{
    public static void MapEndpoints(IEndpointRouteBuilder app)
    {
        CreateEvent.MapEndpoints(app);
        GetEvent.MapEndpoints(app);
    }
}
