namespace Evently.Modules.Events.Domain.Events;

public interface IEventRepository
{
    Task<Event?> GetAsync(Guid id, CancellationToken cancellationToken);

    Task<Event?> GetWithImagesAsync(Guid id, CancellationToken cancellationToken);

    void Insert(Event @event);
}
