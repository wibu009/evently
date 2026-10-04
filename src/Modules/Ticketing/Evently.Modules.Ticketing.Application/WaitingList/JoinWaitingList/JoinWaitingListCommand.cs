using Evently.Common.Application.Messaging;

namespace Evently.Modules.Ticketing.Application.WaitingList.JoinWaitingList;

public sealed record JoinWaitingListCommand(Guid CustomerId, Guid TicketTypeId) : ICommand;
