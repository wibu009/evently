using Evently.Common.Domain;

namespace Evently.Modules.Ticketing.Domain.PromoCodes;

/// <summary>
/// A discount code that customers can apply to an order at checkout.
/// Redemptions are only counted for orders that are actually paid, so abandoned
/// checkouts never burn a redemption.
/// </summary>
public sealed class PromoCode : Entity
{
    private PromoCode() { }

    public Guid Id { get; private set; }
    public string Code { get; private set; }
    public DiscountType DiscountType { get; private set; }
    public decimal DiscountValue { get; private set; }
    public string Currency { get; private set; }
    public int? MaxRedemptions { get; private set; }
    public int TimesRedeemed { get; private set; }
    public DateTime ValidFromUtc { get; private set; }
    public DateTime? ValidUntilUtc { get; private set; }

    public static Result<PromoCode> Create(
        string code,
        DiscountType discountType,
        decimal discountValue,
        string currency,
        int? maxRedemptions,
        DateTime validFromUtc,
        DateTime? validUntilUtc)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return Result.Failure<PromoCode>(PromoCodeErrors.NotFoundByCode(code));
        }

        if (discountValue <= decimal.Zero)
        {
            return Result.Failure<PromoCode>(PromoCodeErrors.InvalidDiscountValue);
        }

        if (discountType == DiscountType.Percentage && discountValue > 100m)
        {
            return Result.Failure<PromoCode>(PromoCodeErrors.InvalidPercentageDiscount);
        }

        if (validUntilUtc.HasValue && validUntilUtc.Value <= validFromUtc)
        {
            return Result.Failure<PromoCode>(PromoCodeErrors.InvalidValidityPeriod);
        }

        var promoCode = new PromoCode
        {
            Id = Guid.CreateVersion7(),
            Code = code.Trim().ToUpperInvariant(),
            DiscountType = discountType,
            DiscountValue = discountValue,
            Currency = currency,
            MaxRedemptions = maxRedemptions,
            TimesRedeemed = 0,
            ValidFromUtc = validFromUtc,
            ValidUntilUtc = validUntilUtc
        };

        return promoCode;
    }

    public bool IsCurrentlyValid(DateTime utcNow) =>
        ValidFromUtc <= utcNow && (!ValidUntilUtc.HasValue || ValidUntilUtc.Value > utcNow);

    /// <summary>
    /// Counts one redemption of the promo code.
    /// </summary>
    public Result Redeem(DateTime utcNow)
    {
        if (!IsCurrentlyValid(utcNow))
        {
            return Result.Failure(PromoCodeErrors.NotCurrentlyValid);
        }

        if (MaxRedemptions.HasValue && TimesRedeemed >= MaxRedemptions.Value)
        {
            return Result.Failure(PromoCodeErrors.MaxRedemptionsReached);
        }

        TimesRedeemed++;

        RaiseDomainEvent(new PromoCodeRedeemedDomainEvent(Id, Code, TimesRedeemed));

        return Result.Success();
    }

    /// <summary>
    /// Ends the validity of the promo code immediately.
    /// </summary>
    public void Deactivate(DateTime utcNow)
    {
        ValidUntilUtc = utcNow;
    }

    /// <summary>
    /// Calculates the discount for the given gross amount.
    /// </summary>
    public Result<decimal> CalculateDiscount(decimal amount, DateTime utcNow)
    {
        if (!IsCurrentlyValid(utcNow))
        {
            return Result.Failure<decimal>(PromoCodeErrors.NotCurrentlyValid);
        }

        if (amount <= decimal.Zero)
        {
            return Result.Failure<decimal>(PromoCodeErrors.DiscountExceedsOrderAmount);
        }

        decimal discount = DiscountType switch
        {
            DiscountType.Percentage => Math.Round(amount * DiscountValue / 100m, 2, MidpointRounding.AwayFromZero),
            DiscountType.FixedAmount => DiscountValue,
            _ => decimal.Zero
        };

        if (discount > amount)
        {
            discount = amount;
        }

        return discount;
    }
}
