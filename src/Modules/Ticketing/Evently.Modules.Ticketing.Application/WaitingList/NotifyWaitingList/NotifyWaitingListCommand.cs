using Evently.Common.Application.Messaging;

namespace Evently.Modules.Ticketing.Application.WaitingList.NotifyWaitingList;

public sealed record NotifyWaitingListCommand(Guid TicketTypeId, decimal Quantity) : ICommand;
