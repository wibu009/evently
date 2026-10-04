using Evently.Common.Application.Messaging;

namespace Evently.Modules.Ticketing.Application.WaitingList.LeaveWaitingList;

public sealed record LeaveWaitingListCommand(Guid CustomerId, Guid TicketTypeId) : ICommand;
