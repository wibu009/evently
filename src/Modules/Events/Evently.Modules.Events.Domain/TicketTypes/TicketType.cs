using Evently.Modules.Events.Domain.Abstractions;
using Evently.Modules.Events.Domain.Events;

namespace Evently.Modules.Events.Domain.TicketTypes;

public sealed class TicketType :Entity
{
    private TicketType() { }
    
    public Guid Id { get; private init; }
    public Guid EventId { get; private set; }
    public string Name { get; private set; }
    public decimal Price { get; private set; }
    public string Currency { get; private set; }
    public decimal Quantity { get; private set; }
    
    public static TicketType Create(
        Event @event,
        string name,
        decimal price,
        string currency,
        decimal quantity)
    {
        var ticketType = new TicketType
        {
            Id = Guid.CreateVersion7(),
            EventId = @event.Id,
            Name = name,
            Price = price,
            Currency = currency,
            Quantity = quantity
        };
        
        ticketType.RaiseDomainEvent(new TicketTypeCreatedDomainEvent(ticketType.Id));

        return ticketType;
    }

    public void UpdatePrice(decimal price)
    {
        if (Price == price)
        {
            return;
        }
        
        Price = price;
        
        RaiseDomainEvent(new TicketTypePriceChangedDomainEvent(Id, price));
    }
}
