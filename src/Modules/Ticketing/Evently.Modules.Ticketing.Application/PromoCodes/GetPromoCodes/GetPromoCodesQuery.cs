using Evently.Common.Application.Messaging;

namespace Evently.Modules.Ticketing.Application.PromoCodes.GetPromoCodes;

public sealed record GetPromoCodesQuery(int Page, int PageSize) : IQuery<GetPromoCodesResponse>;
