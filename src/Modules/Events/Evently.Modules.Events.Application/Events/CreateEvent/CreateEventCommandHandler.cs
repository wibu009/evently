using Evently.Common.Application.Clock;
using Evently.Common.Application.Messaging;
using Evently.Common.Domain;
using Evently.Modules.Events.Application.Abstractions.Data;
using Evently.Modules.Events.Domain.Categories;
using Evently.Modules.Events.Domain.Events;
using MediatR;

namespace Evently.Modules.Events.Application.Events.CreateEvent;

internal sealed class CreateEventCommandHandler(
    IDateTimeProvider dateTimeProvider,
    IEventRepository eventRepository,
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateEventCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateEventCommand request, CancellationToken cancellationToken)
    {
        if (request.StartAtUtc < dateTimeProvider.UtcNow)
        {
            return Result.Failure<Guid>(EventErrors.StartDateInPast);
        }
        
        Category? category = await categoryRepository.GetAsync(request.CategoryId, cancellationToken);
        
        if (category is null)
        {
            return Result.Failure<Guid>(CategoryErrors.NotFound(request.CategoryId));
        }

        Result<Event> @event = Event.Create(
            category,
            request.Title,
            request.Description,
            request.Location,
            request.StartAtUtc,
            request.EndAtUtc);
        
        if (@event.IsFailure)
        {
            return Result.Failure<Guid>(@event.Error);
        }
        
        eventRepository.Insert(@event.Value);
        
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        return @event.Value.Id;
    }
}
