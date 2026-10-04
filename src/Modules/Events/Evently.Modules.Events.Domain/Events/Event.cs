using Evently.Common.Domain;
using Evently.Modules.Events.Domain.Categories;

#pragma warning disable CA1054 // Image/banner URLs are stored as validated strings by design (EF-mapped value, validated as absolute http(s) at the application boundary).

namespace Evently.Modules.Events.Domain.Events;

public sealed class Event : Entity
{
    private Event()
    {
    }
    
    public Guid Id { get; private init; }
    public Guid CategoryId { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public string Location { get; private set; }
    public DateTime StartAtUtc { get; private set; }
    public DateTime? EndAtUtc { get; private set; }
    public EventStatus Status { get; private set; }

    /// <summary>
    /// Optional hero banner shown on the event detail page. Falls back to the
    /// cover gallery image, then to the generated gradient, when not set.
    /// </summary>
    public string? HeroBannerUrl { get; private set; }

    /// <summary>
    /// Optional accent color (hex, e.g. #7C3AED) used to theme the event detail
    /// page and its tickets. Falls back to the brand color when not set.
    /// </summary>
    public string? AccentColor { get; private set; }

    private readonly List<EventImage> _images = [];
    public IReadOnlyCollection<EventImage> Images => [.. _images];

    public static Result<Event> Create(
        Category category,
        string title,
        string description,
        string location,
        DateTime startAtUtc,
        DateTime? endAtUtc)
    {
        if (endAtUtc.HasValue && endAtUtc < startAtUtc)
        {
            return Result.Failure<Event>(EventErrors.EndDatePrecedesStartDate);
        }

        var @event = new Event
        {
            Id = Guid.CreateVersion7(),
            CategoryId = category.Id,
            Title = title,
            Description = description,
            Location = location,
            StartAtUtc = startAtUtc,
            EndAtUtc = endAtUtc,
            Status = EventStatus.Draft
        };

        @event.RaiseDomainEvent(new EventCreatedDomainEvent(@event.Id));

        return @event;
    }

    public Result Publish()
    {
        if (Status != EventStatus.Draft)
        {
            return Result.Failure(EventErrors.NotDraft);
        }
        
        Status = EventStatus.Published;
        
        RaiseDomainEvent(new EventPublishedDomainEvent(Id));
        
        return Result.Success();
    }
    
    public Result Reschedule(DateTime startAtUtc, DateTime? endAtUtc)
    {
        if (Status is EventStatus.Completed or EventStatus.Canceled)
        {
            return Result.Failure(EventErrors.CannotReschedule);
        }

        if (StartAtUtc == startAtUtc && EndAtUtc == endAtUtc)
        {
            return Result.Success();
        }

        if (endAtUtc.HasValue && endAtUtc < startAtUtc)
        {
            return Result.Failure(EventErrors.EndDatePrecedesStartDate);
        }

        StartAtUtc = startAtUtc;
        EndAtUtc = endAtUtc;

        RaiseDomainEvent(new EventRescheduledDomainEvent(Id, startAtUtc, endAtUtc));

        return Result.Success();
    }

    public Result UpdateDetails(string title, string description, string location)
    {
        if (Status == EventStatus.Canceled)
        {
            return Result.Failure(EventErrors.CannotUpdate);
        }

        if (Title == title && Description == description && Location == location)
        {
            return Result.Success();
        }

        Title = title;
        Description = description;
        Location = location;

        RaiseDomainEvent(new EventUpdatedDomainEvent(Id));

        return Result.Success();
    }

    /// <summary>
    /// Completes a published event after it has taken place.
    /// Completed events disappear from the public search catalog.
    /// </summary>
    public Result Complete(DateTime utcNow)
    {
        if (Status != EventStatus.Published)
        {
            return Result.Failure(EventErrors.NotPublished);
        }

        if (StartAtUtc > utcNow)
        {
            return Result.Failure(EventErrors.NotStarted);
        }

        Status = EventStatus.Completed;

        RaiseDomainEvent(new EventCompletedDomainEvent(Id));

        return Result.Success();
    }

    public Result Cancel(DateTime utcNow)
    {
        if (Status == EventStatus.Canceled)
        {
            return Result.Failure(EventErrors.AlreadyCanceled);
        }

        if (StartAtUtc < utcNow)
        {
            return Result.Failure(EventErrors.AlreadyStarted);
        }
        
        Status = EventStatus.Canceled;
        
        RaiseDomainEvent(new EventCanceledDomainEvent(Id));
        
        return Result.Success();
    }

    /// <summary>
    /// The gallery is capped so a single event cannot bloat the catalog queries.
    /// </summary>
    public const int MaxImagesPerEvent = 8;

    public Result<EventImage> AddImage(string imageUrl)
    {
        if (Status == EventStatus.Canceled)
        {
            return Result.Failure<EventImage>(EventErrors.CannotUpdate);
        }

        if (_images.Count >= MaxImagesPerEvent)
        {
            return Result.Failure<EventImage>(EventErrors.ImageLimitReached(MaxImagesPerEvent));
        }

        bool isCover = _images.Count == 0;

        Result<EventImage> result = EventImage.Create(this, imageUrl, isCover, _images.Count);
        if (result.IsFailure)
        {
            return result;
        }

        _images.Add(result.Value);

        return result.Value;
    }

    public Result RemoveImage(Guid imageId)
    {
        EventImage? image = _images.Find(i => i.Id == imageId);
        if (image is null)
        {
            return Result.Failure(EventErrors.ImageNotFound(imageId));
        }

        _images.Remove(image);

        if (image.IsCover)
        {
            EventImage? next = _images.MinBy(i => i.DisplayOrder);
            next?.PromoteToCover();
        }

        return Result.Success();
    }

    public Result SetCoverImage(Guid imageId)
    {
        EventImage? image = _images.Find(i => i.Id == imageId);
        if (image is null)
        {
            return Result.Failure(EventErrors.ImageNotFound(imageId));
        }

        foreach (EventImage other in _images)
        {
            other.DemoteFromCover();
        }

        image.PromoteToCover();

        return Result.Success();
    }

    /// <summary>
    /// Customizes the hero banner and accent color of the event detail page.
    /// Display-only: no domain events are raised and nothing fans out to other modules.
    /// </summary>
    public Result UpdateAppearance(string? heroBannerUrl, string? accentColor)
    {
        if (Status == EventStatus.Canceled)
        {
            return Result.Failure(EventErrors.CannotUpdate);
        }

        if (!string.IsNullOrWhiteSpace(heroBannerUrl))
        {
            if (heroBannerUrl.Trim().Length > 500)
            {
                return Result.Failure(EventErrors.InvalidImageUrl);
            }

            HeroBannerUrl = heroBannerUrl.Trim();
        }
        else
        {
            HeroBannerUrl = null;
        }

        if (!string.IsNullOrWhiteSpace(accentColor))
        {
            if (!EventAppearance.IsValidHexColor(accentColor.Trim()))
            {
                return Result.Failure(EventErrors.InvalidAccentColor);
            }

            AccentColor = accentColor.Trim().ToUpperInvariant();
        }
        else
        {
            AccentColor = null;
        }

        return Result.Success();
    }
}
