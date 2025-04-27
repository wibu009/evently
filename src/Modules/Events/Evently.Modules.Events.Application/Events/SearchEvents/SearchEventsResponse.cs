using Evently.Modules.Events.Application.Events.GetEvents;

namespace Evently.Modules.Events.Application.Events.SearchEvents;

public record SearchEventsResponse(int Page, int PageSize, int TotalCount, IReadOnlyCollection<EventResponse> Events);
