using Evently.Common.Domain;

namespace Evently.Modules.Events.Domain.Events;

public static class EventErrors
{
    public static Error NotFound(Guid eventId)
        => Error.NotFound("Events.NotFound", $"Event with id {eventId} not found");
    
    public static readonly Error StartDateInPast = Error.Problem("Events.StartDateInPast", "The event start date is in the past");
    
    public static readonly Error EndDatePrecedesStartDate = Error.Problem("Events.EndDatePrecedesStartDate", "The event end date precedes the start date");
    
    public static readonly Error NoTicketsFound = Error.Problem("Events.NoTicketsFound", "The event does not have any ticket types defined");
    
    public static readonly Error NotDraft = Error.Problem("Events.NotDraft", "The event is not in draft status");
    
    public static readonly Error AlreadyCanceled = Error.Problem("Events.AlreadyCanceled", "The event was already canceled");
    
    public static readonly Error AlreadyStarted = Error.Problem("Events.AlreadyStarted", "The event has already started");

    public static readonly Error NotPublished = Error.Problem("Events.NotPublished", "The event is not in published status");

    public static readonly Error NotStarted = Error.Problem("Events.NotStarted", "The event has not started yet");

    public static readonly Error CannotReschedule = Error.Problem("Events.CannotReschedule", "Canceled or completed events cannot be rescheduled");

    public static readonly Error CannotUpdate = Error.Problem("Events.CannotUpdate", "Canceled events cannot be updated");

    public static Error ImageNotFound(Guid imageId)
        => Error.NotFound("Events.ImageNotFound", $"Event image with id {imageId} not found");

    public static readonly Error InvalidImageUrl = Error.Problem("Events.InvalidImageUrl", "The image URL must be a non-empty value of at most 500 characters");

    public static readonly Error InvalidAccentColor = Error.Problem("Events.InvalidAccentColor", "The accent color must be a hex color like #7C3AED");

    public static Error ImageLimitReached(int maxImages)
        => Error.Problem("Events.ImageLimitReached", $"An event can have at most {maxImages} images");
}
