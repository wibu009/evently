using Evently.Common.Application.Messaging;
using Evently.Modules.Ticketing.Application.Tickets.GetTicket;

namespace Evently.Modules.Ticketing.Application.Tickets.GetTicketsForOrder;

public sealed record GetTicketsForOrderQuery(Guid OrderId): IQuery<IReadOnlyList<TicketResponse>>;
