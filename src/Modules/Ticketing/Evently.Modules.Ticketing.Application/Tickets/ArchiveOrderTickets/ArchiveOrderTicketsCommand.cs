using Evently.Common.Application.Messaging;

namespace Evently.Modules.Ticketing.Application.Tickets.ArchiveOrderTickets;

public sealed record ArchiveOrderTicketsCommand(Guid OrderId) : ICommand;
