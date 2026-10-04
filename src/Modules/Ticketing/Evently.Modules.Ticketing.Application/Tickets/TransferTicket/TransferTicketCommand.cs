using Evently.Common.Application.Messaging;

namespace Evently.Modules.Ticketing.Application.Tickets.TransferTicket;

public sealed record TransferTicketCommand(Guid TicketId, Guid ToCustomerId) : ICommand;
