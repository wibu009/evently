using Evently.Common.Domain;
using Evently.Modules.Events.Domain.Events;

#pragma warning disable CA1054 // Background image URLs are stored as validated strings by design (EF-mapped value, validated as absolute http(s) at the application boundary).

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

    /// <summary>
    /// Optional ticket face color (hex, e.g. #7C3AED). Falls back to the event
    /// accent color, then to the brand color, when not set.
    /// </summary>
    public string? Color { get; private set; }

    /// <summary>
    /// Optional ticket face background image. Falls back to a gradient when not set.
    /// </summary>
    public string? BackgroundImageUrl { get; private set; }
    
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

    /// <summary>
    /// Customizes the ticket face design. Display-only: no domain events are
    /// raised and nothing fans out to other modules.
    /// </summary>
    public Result UpdateDesign(string? color, string? backgroundImageUrl)
    {
        if (!string.IsNullOrWhiteSpace(color))
        {
            if (!TicketTypeDesign.IsValidHexColor(color.Trim()))
            {
                return Result.Failure(TicketTypeErrors.InvalidColor);
            }

            Color = color.Trim().ToUpperInvariant();
        }
        else
        {
            Color = null;
        }

        if (!string.IsNullOrWhiteSpace(backgroundImageUrl))
        {
            if (backgroundImageUrl.Trim().Length > 500)
            {
                return Result.Failure(TicketTypeErrors.InvalidBackgroundImageUrl);
            }

            BackgroundImageUrl = backgroundImageUrl.Trim();
        }
        else
        {
            BackgroundImageUrl = null;
        }

        return Result.Success();
    }
}
