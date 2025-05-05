using Evently.Common.Application.Messaging;

namespace Evently.Modules.Events.Application.Events.SearchEvents;

public record SearchEventsQuery(
    Guid? CategoryId,
    DateTime? StartDate,
    DateTime? EndDate,
    int Page,
    int PageSize) : IQuery<SearchEventsResponse>;
