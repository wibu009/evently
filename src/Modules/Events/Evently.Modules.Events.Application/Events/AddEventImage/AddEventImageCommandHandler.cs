using Evently.Common.Application.Messaging;
using Evently.Common.Domain;
using Evently.Modules.Events.Application.Abstractions.Data;
using Evently.Modules.Events.Domain.Events;

namespace Evently.Modules.Events.Application.Events.AddEventImage;

internal sealed class AddEventImageCommandHandler(
    IEventRepository eventRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<AddEventImageCommand, Guid>
{
    public async Task<Result<Guid>> Handle(AddEventImageCommand request, CancellationToken cancellationToken)
    {
        Event? @event = await eventRepository.GetWithImagesAsync(request.EventId, cancellationToken);
        if (@event is null)
        {
            return Result.Failure<Guid>(EventErrors.NotFound(request.EventId));
        }

        Result<EventImage> result = @event.AddImage(request.ImageUrl);
        if (result.IsFailure)
        {
            return Result.Failure<Guid>(result.Error);
        }

        if (request.SetAsCover)
        {
            Result coverResult = @event.SetCoverImage(result.Value.Id);
            if (coverResult.IsFailure)
            {
                return Result.Failure<Guid>(coverResult.Error);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return result.Value.Id;
    }
}
