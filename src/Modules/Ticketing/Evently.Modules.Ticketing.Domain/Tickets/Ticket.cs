using Evently.Common.Domain;
using Evently.Modules.Ticketing.Domain.Customers;
using Evently.Modules.Ticketing.Domain.Events;
using Evently.Modules.Ticketing.Domain.Orders;

namespace Evently.Modules.Ticketing.Domain.Tickets;

public sealed class Ticket : Entity
{
    private Ticket() { }

    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid EventId { get; private set; }
    public Guid TicketTypeId { get; private set; }
    public string Code { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public bool Archived { get; private set; }
    
    public static Ticket Create(Order order, TicketType ticketType)
    {
        var ticket = new Ticket
        {
            Id = Guid.CreateVersion7(),
            CustomerId = order.CustomerId,
            OrderId = order.Id,
            EventId = ticketType.EventId,
            TicketTypeId = ticketType.Id,
            Code = $"tc_{Ulid.NewUlid()}",
            CreatedAtUtc = DateTime.UtcNow
        };

        ticket.RaiseDomainEvent(new TicketCreatedDomainEvent(ticket.Id));

        return ticket;
    }
    
    public void Archive()
    {
        if (Archived)
        {
            return;
        }

        Archived = true;
        RaiseDomainEvent(new TicketArchivedDomainEvent(Id, Code));
    }

    /// <summary>
    /// Transfers the ticket to another customer. The previous owner loses access
    /// and the new owner can use the ticket for check-in.
    /// </summary>
    public Result Transfer(Customer fromCustomer, Customer toCustomer)
    {
        if (Archived)
        {
            return Result.Failure(TicketErrors.CannotTransferArchivedTicket);
        }

        if (CustomerId != fromCustomer.Id)
        {
            return Result.Failure(TicketErrors.NotOwnedByCustomer(fromCustomer.Id));
        }

        if (toCustomer.Id == fromCustomer.Id)
        {
            return Result.Failure(TicketErrors.CannotTransferToSelf);
        }

        CustomerId = toCustomer.Id;

        RaiseDomainEvent(new TicketTransferredDomainEvent(Id, Code, fromCustomer.Id, toCustomer.Id));

        return Result.Success();
    }
}
