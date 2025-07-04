﻿namespace Evently.Modules.Ticketing.Domain.Events;

public interface ITicketTypeRepository
{
    Task<TicketType?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TicketType?> GetWithLockAsync(Guid id, CancellationToken cancellationToken = default);
    void Insert(TicketType ticketType);
    void InsertRange(IEnumerable<TicketType> ticketTypes);
}
