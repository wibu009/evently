namespace Evently.Modules.Ticketing.Application.WaitingList.GetMyWaitingList;

public sealed record WaitingListEntryResponse(
    Guid Id,
    Guid TicketTypeId,
    string Status,
    DateTime CreatedAtUtc,
    DateTime? NotifiedAtUtc);
