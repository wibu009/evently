using Evently.Common.Domain;
using Evently.Modules.Ticketing.Application.Carts;
using Evently.Modules.Ticketing.Application.Carts.AddItemToCart;
using Evently.Modules.Ticketing.Application.Orders.CreateOrder;
using Evently.Modules.Ticketing.Application.PromoCodes.CreatePromoCode;
using Evently.Modules.Ticketing.Application.PromoCodes.RedeemPromoCode;
using Evently.Modules.Ticketing.Domain.PromoCodes;
using Evently.Modules.Ticketing.IntegrationTests.Abstractions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Evently.Modules.Ticketing.IntegrationTests.Payments;

public class PromoCodeTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Should_ReturnFailure_WhenPromoCodeDoesNotExist()
    {
        //Arrange
        await CleanDatabaseAsync();

        Guid customerId = await Sender.CreateCustomerAsync(Guid.CreateVersion7());
        var eventId = Guid.CreateVersion7();
        var ticketTypeId = Guid.CreateVersion7();

        await Sender.CreateEventWithTicketTypeAsync(eventId, ticketTypeId, 5m);

        await Sender.Send(new AddItemToCartCommand(customerId, ticketTypeId, 1m));

        var command = new CreateOrderCommand(customerId, "DOES_NOT_EXIST");

        //Act
        Result<Guid> result = await Sender.Send(command);

        //Assert
        result.Error.Should().Be(PromoCodeErrors.NotFoundByCode("DOES_NOT_EXIST"));
    }

    [Fact]
    public async Task Should_ReturnFailure_WhenPromoCodeAlreadyExists()
    {
        //Arrange
        await CleanDatabaseAsync();

        var createCommand = new CreatePromoCodeCommand(
            "SUMMER10",
            (int)DiscountType.Percentage,
            10m,
            "USD",
            100,
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow.AddDays(30));

        await Sender.Send(createCommand);

        //Act
        Result<Guid> result = await Sender.Send(createCommand);

        //Assert
        result.Error.Should().Be(PromoCodeErrors.AlreadyExists);
    }

    [Fact]
    public async Task Should_ApplyDiscount_AndChargeNetPrice_WhenPromoCodeIsUsedAtCheckout()
    {
        //Arrange
        await CleanDatabaseAsync();

        Guid customerId = await Sender.CreateCustomerAsync(Guid.CreateVersion7());
        var eventId = Guid.CreateVersion7();
        var ticketTypeId = Guid.CreateVersion7();

        await Sender.CreateEventWithTicketTypeAsync(eventId, ticketTypeId, 5m);

        Result<Guid> promoCodeResult = await Sender.Send(new CreatePromoCodeCommand(
            "SUMMER10",
            (int)DiscountType.Percentage,
            10m,
            "USD",
            100,
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow.AddDays(30)));
        promoCodeResult.IsSuccess.Should().BeTrue();

        await Sender.Send(new AddItemToCartCommand(customerId, ticketTypeId, 1m));

        var command = new CreateOrderCommand(customerId, "summer10");

        //Act
        Result<Guid> result = await Sender.Send(command);

        //Assert
        result.IsSuccess.Should().BeTrue();

        Domain.Orders.Order? order = await DbContext.Set<Domain.Orders.Order>()
            .SingleOrDefaultAsync(o => o.Id == result.Value, CancellationToken.None);

        order!.PromoCodeId.Should().Be(promoCodeResult.Value);
        order.DiscountAmount.Should().BeGreaterThan(decimal.Zero);
        order.NetPrice.Should().Be(order.TotalPrice - order.DiscountAmount);

        Domain.Payments.Payment? payment = await DbContext.Set<Domain.Payments.Payment>()
            .SingleOrDefaultAsync(p => p.OrderId == result.Value, CancellationToken.None);

        payment!.Amount.Should().Be(order.NetPrice);
    }

    [Fact]
    public async Task Should_CountRedemption_WhenPaidOrderWithPromoCodeIsRedeemed()
    {
        //Arrange
        await CleanDatabaseAsync();

        Guid customerId = await Sender.CreateCustomerAsync(Guid.CreateVersion7());
        var eventId = Guid.CreateVersion7();
        var ticketTypeId = Guid.CreateVersion7();

        await Sender.CreateEventWithTicketTypeAsync(eventId, ticketTypeId, 5m);

        Guid promoCodeId = (await Sender.Send(new CreatePromoCodeCommand(
            "SUMMER10",
            (int)DiscountType.Percentage,
            10m,
            "USD",
            100,
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow.AddDays(30)))).Value;

        await Sender.Send(new AddItemToCartCommand(customerId, ticketTypeId, 1m));

        Result<Guid> orderResult = await Sender.Send(new CreateOrderCommand(customerId, "SUMMER10"));
        orderResult.IsSuccess.Should().BeTrue();

        //Act — the redemption is counted only after the order is paid
        Result result = await Sender.Send(new RedeemPromoCodeCommand(orderResult.Value));

        //Assert
        result.IsSuccess.Should().BeTrue();

        Domain.PromoCodes.PromoCode? promoCode = await DbContext.Set<Domain.PromoCodes.PromoCode>()
            .SingleOrDefaultAsync(p => p.Id == promoCodeId, CancellationToken.None);

        promoCode!.TimesRedeemed.Should().Be(1);
    }
}
