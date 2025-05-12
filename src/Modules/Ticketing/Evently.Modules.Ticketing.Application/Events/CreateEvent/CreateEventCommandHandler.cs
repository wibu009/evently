using Evently.Common.Application.Messaging;
using Evently.Common.Domain;
using Evently.Modules.Ticketing.Application.Abstractions.Data;
using Evently.Modules.Ticketing.Domain.Events;

namespace Evently.Modules.Ticketing.Application.Events.CreateEvent;

internal sealed class CreateEventCommandHandler(
    IEventRepository eventRepository,
    ITicketTypeRepository ticketTypeRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateEventCommand>
{
    public async Task<Result> Handle(CreateEventCommand request, CancellationToken cancellationToken)
    {
        var @event = Event.Create(
            request.EventId,
            request.Title,
            request.Description,
            request.Location,
            request.StartAtUtc,
            request.EndAtUtc);
        
        eventRepository.Insert(@event);
        
        IEnumerable<TicketType> ticketTypes = request.TicketTypes
            .Select(x => TicketType.Create(x.TicketTypeId, x.EventId, x.Name, x.Price, x.Currency, x.Quantity));
        
        ticketTypeRepository.InsertRange(ticketTypes);
        
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        return Result.Success();
    }
}