using Evently.Common.Domain;
using Evently.Modules.Ticketing.Domain.PromoCodes;
using Evently.Modules.Ticketing.Domain.UnitTests.Abstractions;
using FluentAssertions;

namespace Evently.Modules.Ticketing.Domain.UnitTests.PromoCodes;

public class PromoCodeTests : BaseTest
{
    private static readonly DateTime ValidFrom = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime ValidUntil = new(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Now = new(2026, 6, 15, 0, 0, 0, DateTimeKind.Utc);

    private static PromoCode CreatePercentagePromoCode(decimal percentage = 10m, int? maxRedemptions = null) =>
        PromoCode.Create("SUMMER10", DiscountType.Percentage, percentage, "USD", maxRedemptions, ValidFrom, ValidUntil).Value;

    private static PromoCode CreateFixedPromoCode(decimal amount = 25m) =>
        PromoCode.Create("SAVE25", DiscountType.FixedAmount, amount, "USD", null, ValidFrom, ValidUntil).Value;

    [Fact]
    public void Create_ShouldNormalizeCode_ToUpperCase()
    {
        // Arrange + Act
        PromoCode promoCode = PromoCode.Create(" summer10 ", DiscountType.Percentage, 10m, "USD", null, ValidFrom, ValidUntil).Value;

        // Assert
        promoCode.Code.Should().Be("SUMMER10");
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenDiscountValueIsNotPositive()
    {
        // Arrange + Act
        Result<PromoCode> result = PromoCode.Create("X", DiscountType.Percentage, decimal.Zero, "USD", null, ValidFrom, ValidUntil);

        // Assert
        result.Error.Should().Be(PromoCodeErrors.InvalidDiscountValue);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenPercentageDiscountExceeds100()
    {
        // Arrange + Act
        Result<PromoCode> result = PromoCode.Create("X", DiscountType.Percentage, 150m, "USD", null, ValidFrom, ValidUntil);

        // Assert
        result.Error.Should().Be(PromoCodeErrors.InvalidPercentageDiscount);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenValidityPeriodIsInvalid()
    {
        // Arrange + Act
        Result<PromoCode> result = PromoCode.Create(
            "X",
            DiscountType.Percentage,
            10m,
            "USD",
            null,
            ValidUntil,
            ValidFrom);

        // Assert
        result.Error.Should().Be(PromoCodeErrors.InvalidValidityPeriod);
    }

    [Fact]
    public void CalculateDiscount_ShouldReturnPercentageOfAmount()
    {
        // Arrange
        PromoCode promoCode = CreatePercentagePromoCode(10m);

        // Act
        Result<decimal> result = promoCode.CalculateDiscount(199.99m, Now);

        // Assert
        result.Value.Should().Be(20.00m);
    }

    [Fact]
    public void CalculateDiscount_ShouldReturnFixedAmount()
    {
        // Arrange
        PromoCode promoCode = CreateFixedPromoCode(25m);

        // Act
        Result<decimal> result = promoCode.CalculateDiscount(100m, Now);

        // Assert
        result.Value.Should().Be(25m);
    }

    [Fact]
    public void CalculateDiscount_ShouldCapAtTheOrderAmount()
    {
        // Arrange
        PromoCode promoCode = CreateFixedPromoCode(25m);

        // Act
        Result<decimal> result = promoCode.CalculateDiscount(10m, Now);

        // Assert
        result.Value.Should().Be(10m);
    }

    [Fact]
    public void CalculateDiscount_ShouldReturnFailure_WhenPromoCodeIsExpired()
    {
        // Arrange
        PromoCode promoCode = CreatePercentagePromoCode();

        // Act
        Result<decimal> result = promoCode.CalculateDiscount(100m, ValidUntil);

        // Assert
        result.Error.Should().Be(PromoCodeErrors.NotCurrentlyValid);
    }

    [Fact]
    public void Redeem_ShouldIncreaseTimesRedeemed_AndRaiseDomainEvent_WhenPromoCodeIsValid()
    {
        // Arrange
        PromoCode promoCode = CreatePercentagePromoCode(maxRedemptions: 2);

        // Act
        Result result = promoCode.Redeem(Now);

        // Assert
        result.IsSuccess.Should().BeTrue();
        promoCode.TimesRedeemed.Should().Be(1);

        PromoCodeRedeemedDomainEvent domainEvent =
            AssertDomainEventWasPublished<PromoCodeRedeemedDomainEvent>(promoCode);

        domainEvent.PromoCodeId.Should().Be(promoCode.Id);
        domainEvent.Code.Should().Be(promoCode.Code);
    }

    [Fact]
    public void Redeem_ShouldReturnFailure_WhenMaxRedemptionsReached()
    {
        // Arrange
        PromoCode promoCode = CreatePercentagePromoCode(maxRedemptions: 1);

        promoCode.Redeem(Now);

        // Act
        Result result = promoCode.Redeem(Now);

        // Assert
        result.Error.Should().Be(PromoCodeErrors.MaxRedemptionsReached);
    }

    [Fact]
    public void Redeem_ShouldReturnFailure_WhenPromoCodeIsExpired()
    {
        // Arrange
        PromoCode promoCode = CreatePercentagePromoCode();

        // Act
        Result result = promoCode.Redeem(ValidUntil);

        // Assert
        result.Error.Should().Be(PromoCodeErrors.NotCurrentlyValid);
    }

    [Fact]
    public void Deactivate_ShouldEndValidityImmediately()
    {
        // Arrange
        PromoCode promoCode = CreatePercentagePromoCode();

        // Act
        promoCode.Deactivate(Now);

        // Assert
        promoCode.IsCurrentlyValid(Now).Should().BeFalse();
    }
}
