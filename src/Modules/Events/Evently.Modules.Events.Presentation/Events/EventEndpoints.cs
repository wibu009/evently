using Microsoft.AspNetCore.Routing;

namespace Evently.Modules.Events.Presentation.Events;

public sealed class EventEndpoints
{
    public static void MapEndpoints(IEndpointRouteBuilder app)
    {
        CancelEvent.MapEndpoint(app);
        CreateEvent.MapEndpoint(app);
        GetEvent.MapEndpoint(app);
        GetEvents.MapEndpoint(app);
        PublishEvent.MapEndpoint(app);
        RescheduleEvent.MapEndpoint(app);
        SearchEvents.MapEndpoint(app);
    }
}
