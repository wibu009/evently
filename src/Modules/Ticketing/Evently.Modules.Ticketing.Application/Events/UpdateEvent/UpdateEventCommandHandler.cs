using Evently.Common.Application.Messaging;
using Evently.Common.Domain;
using Evently.Modules.Ticketing.Application.Abstractions.Data;
using Evently.Modules.Ticketing.Domain.Events;

namespace Evently.Modules.Ticketing.Application.Events.UpdateEvent;

/// <summary>
/// Keeps the local event replica in sync with the Events module when an event is updated.
/// </summary>
internal sealed class UpdateEventCommandHandler(
    IEventRepository eventRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateEventCommand>
{
    public async Task<Result> Handle(UpdateEventCommand request, CancellationToken cancellationToken)
    {
        Event? @event = await eventRepository.GetAsync(request.EventId, cancellationToken);
        if (@event is null)
        {
            return Result.Failure(EventErrors.NotFound(request.EventId));
        }

        @event.UpdateDetails(request.Title, request.Description, request.Location);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
