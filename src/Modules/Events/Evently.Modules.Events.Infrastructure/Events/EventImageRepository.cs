using Evently.Modules.Events.Domain.Events;
using Evently.Modules.Events.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Evently.Modules.Events.Infrastructure.Events;

internal sealed class EventImageRepository(EventsDbContext context) : IEventImageRepository
{
    public async Task<EventImage?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.EventImages.SingleOrDefaultAsync(i => i.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<EventImage>> GetForEventAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        return await context.EventImages
            .Where(i => i.EventId == eventId)
            .OrderBy(i => i.DisplayOrder)
            .ToListAsync(cancellationToken);
    }

    public void Insert(EventImage image)
    {
        context.EventImages.Add(image);
    }

    public void Remove(EventImage image)
    {
        context.EventImages.Remove(image);
    }
}
