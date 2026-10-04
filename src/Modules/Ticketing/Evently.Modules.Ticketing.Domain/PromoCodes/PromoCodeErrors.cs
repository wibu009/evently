using Evently.Common.Domain;

namespace Evently.Modules.Ticketing.Domain.PromoCodes;

public static class PromoCodeErrors
{
    public static Error NotFound(Guid promoCodeId) =>
        Error.NotFound("PromoCodes.NotFound", $"Promo code with id {promoCodeId} not found");

    public static Error NotFoundByCode(string code) =>
        Error.NotFound("PromoCodes.NotFoundByCode", $"Promo code '{code}' not found");

    public static readonly Error AlreadyExists =
        Error.Conflict("PromoCodes.AlreadyExists", "A promo code with the same code already exists");

    public static readonly Error InvalidDiscountValue =
        Error.Problem("PromoCodes.InvalidDiscountValue", "The discount value must be greater than zero");

    public static readonly Error InvalidPercentageDiscount =
        Error.Problem("PromoCodes.InvalidPercentageDiscount", "A percentage discount cannot exceed 100 percent");

    public static readonly Error InvalidValidityPeriod =
        Error.Problem("PromoCodes.InvalidValidityPeriod", "The validity end must be after the validity start");

    public static readonly Error NotCurrentlyValid =
        Error.Problem("PromoCodes.NotCurrentlyValid", "The promo code is not within its validity period");

    public static readonly Error MaxRedemptionsReached =
        Error.Problem("PromoCodes.MaxRedemptionsReached", "The promo code has reached its maximum number of redemptions");

    public static Error CurrencyMismatch(string promoCodeCurrency, string orderCurrency) =>
        Error.Problem("PromoCodes.CurrencyMismatch", $"The promo code currency {promoCodeCurrency} does not match the order currency {orderCurrency}");

    public static readonly Error DiscountExceedsOrderAmount =
        Error.Problem("PromoCodes.DiscountExceedsOrderAmount", "The discount cannot exceed the order amount");
}
