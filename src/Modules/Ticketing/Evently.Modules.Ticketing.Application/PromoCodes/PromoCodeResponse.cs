namespace Evently.Modules.Ticketing.Application.PromoCodes;

public sealed record PromoCodeResponse(
    Guid Id,
    string Code,
    string DiscountType,
    decimal DiscountValue,
    string Currency,
    int? MaxRedemptions,
    int TimesRedeemed,
    DateTime ValidFromUtc,
    DateTime? ValidUntilUtc);
