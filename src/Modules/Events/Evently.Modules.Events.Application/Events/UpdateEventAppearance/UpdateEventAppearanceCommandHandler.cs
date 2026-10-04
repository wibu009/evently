using Evently.Common.Application.Messaging;
using Evently.Common.Domain;
using Evently.Modules.Events.Application.Abstractions.Data;
using Evently.Modules.Events.Domain.Events;

namespace Evently.Modules.Events.Application.Events.UpdateEventAppearance;

internal sealed class UpdateEventAppearanceCommandHandler(
    IEventRepository eventRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateEventAppearanceCommand>
{
    public async Task<Result> Handle(UpdateEventAppearanceCommand request, CancellationToken cancellationToken)
    {
        Event? @event = await eventRepository.GetAsync(request.EventId, cancellationToken);
        if (@event is null)
        {
            return Result.Failure(EventErrors.NotFound(request.EventId));
        }

        Result result = @event.UpdateAppearance(request.HeroBannerUrl, request.AccentColor);
        if (result.IsFailure)
        {
            return Result.Failure(result.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
