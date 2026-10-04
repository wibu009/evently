using Evently.Common.Domain;

namespace Evently.Modules.Events.Domain.Events;

/// <summary>
/// A gallery image attached to an event. Images are display-only metadata:
/// adding, removing or promoting them never raises domain events, so the
/// catalog stays fast and no integration events fan out to other modules.
/// </summary>
public sealed class EventImage : Entity
{
    private EventImage()
    {
    }

    public Guid Id { get; private init; }
    public Guid EventId { get; private init; }
    public string ImageUrl { get; private set; }
    public bool IsCover { get; private set; }
    public int DisplayOrder { get; private set; }
    public DateTime CreatedAtUtc { get; private init; }

    internal static Result<EventImage> Create(Event @event, string imageUrl, bool isCover, int displayOrder)
    {
        if (string.IsNullOrWhiteSpace(imageUrl) || imageUrl.Length > 500)
        {
            return Result.Failure<EventImage>(EventErrors.InvalidImageUrl);
        }

        var image = new EventImage
        {
            Id = Guid.CreateVersion7(),
            EventId = @event.Id,
            ImageUrl = imageUrl.Trim(),
            IsCover = isCover,
            DisplayOrder = displayOrder,
            CreatedAtUtc = DateTime.UtcNow
        };

        return image;
    }

    internal void PromoteToCover()
    {
        IsCover = true;
    }

    internal void DemoteFromCover()
    {
        IsCover = false;
    }
}
