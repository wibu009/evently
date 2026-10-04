using Evently.Common.Application.Messaging;
using Evently.Common.Domain;
using Evently.Modules.Events.Application.Abstractions.Data;
using Evently.Modules.Events.Domain.Events;

namespace Evently.Modules.Events.Application.Events.UpdateEvent;

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

        Result result = @event.UpdateDetails(request.Title, request.Description, request.Location);
        if (result.IsFailure)
        {
            return Result.Failure(result.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
