namespace Evently.Modules.Ticketing.Application.PromoCodes.GetPromoCodes;

public sealed record GetPromoCodesResponse(
    int Page,
    int PageSize,
    int TotalCount,
    IReadOnlyList<PromoCodeResponse> PromoCodes);
