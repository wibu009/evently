using Evently.Common.Application.Messaging;

namespace Evently.Modules.Ticketing.Application.WaitingList.GetMyWaitingList;

public sealed record GetMyWaitingListQuery(Guid CustomerId) : IQuery<IReadOnlyList<WaitingListEntryResponse>>;
