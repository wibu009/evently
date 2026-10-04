using Evently.Common.Application.Messaging;
using Evently.Common.Domain;
using Evently.Modules.Events.Application.Abstractions.Data;
using Evently.Modules.Events.Domain.Events;

namespace Evently.Modules.Events.Application.Events.RemoveEventImage;

internal sealed class RemoveEventImageCommandHandler(
    IEventRepository eventRepository,
    IEventImageRepository imageRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<RemoveEventImageCommand>
{
    public async Task<Result> Handle(RemoveEventImageCommand request, CancellationToken cancellationToken)
    {
        Event? @event = await eventRepository.GetWithImagesAsync(request.EventId, cancellationToken);
        if (@event is null)
        {
            return Result.Failure(EventErrors.NotFound(request.EventId));
        }

        EventImage? image = await imageRepository.GetAsync(request.ImageId, cancellationToken);
        if (image is null || image.EventId != request.EventId)
        {
            return Result.Failure(EventErrors.ImageNotFound(request.ImageId));
        }

        Result result = @event.RemoveImage(request.ImageId);
        if (result.IsFailure)
        {
            return Result.Failure(result.Error);
        }

        imageRepository.Remove(image);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
