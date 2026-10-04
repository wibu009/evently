using Evently.Common.Application.Clock;
using Evently.Common.Application.Messaging;
using Evently.Common.Domain;
using Evently.Modules.Ticketing.Application.Abstractions.Data;
using Evently.Modules.Ticketing.Domain.Orders;
using Evently.Modules.Ticketing.Domain.PromoCodes;
using Microsoft.Extensions.Logging;

namespace Evently.Modules.Ticketing.Application.PromoCodes.RedeemPromoCode;

/// <summary>
/// Counts the redemption of the promo code that was applied to the given order.
/// Executed through the outbox once the order is paid, so abandoned checkouts
/// never burn a redemption of a limited promo code.
/// </summary>
internal sealed class RedeemPromoCodeCommandHandler(
    IOrderRepository orderRepository,
    IPromoCodeRepository promoCodeRepository,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork,
    ILogger<RedeemPromoCodeCommandHandler> logger)
    : ICommandHandler<RedeemPromoCodeCommand>
{
    public async Task<Result> Handle(RedeemPromoCodeCommand request, CancellationToken cancellationToken)
    {
        Order? order = await orderRepository.GetAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            return Result.Failure(OrderErrors.NotFound(request.OrderId));
        }

        if (!order.PromoCodeId.HasValue)
        {
            return Result.Success();
        }

        PromoCode? promoCode = await promoCodeRepository.GetAsync(order.PromoCodeId.Value, cancellationToken);
        if (promoCode is null)
        {
            return Result.Failure(PromoCodeErrors.NotFound(order.PromoCodeId.Value));
        }

        Result result = promoCode.Redeem(dateTimeProvider.UtcNow);
        if (result.IsFailure)
        {
            // The promo code expired or ran out of redemptions between checkout and payment.
            // The order stays valid; only the redemption counter is not increased.
            logger.LogWarning(
                "The promo code {PromoCode} of paid order {OrderId} could not be redeemed: {Error}",
                promoCode.Code,
                order.Id,
                result.Error.Description);

            return Result.Success();
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
