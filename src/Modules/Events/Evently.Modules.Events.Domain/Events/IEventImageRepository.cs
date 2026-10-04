namespace Evently.Modules.Events.Domain.Events;

public interface IEventImageRepository
{
    Task<EventImage?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EventImage>> GetForEventAsync(Guid eventId, CancellationToken cancellationToken = default);

    void Insert(EventImage image);

    void Remove(EventImage image);
}
