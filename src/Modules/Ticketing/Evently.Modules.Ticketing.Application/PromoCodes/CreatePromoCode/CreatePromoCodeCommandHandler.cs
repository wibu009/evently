using Evently.Common.Application.Clock;
using Evently.Common.Application.Messaging;
using Evently.Common.Domain;
using Evently.Modules.Ticketing.Application.Abstractions.Data;
using Evently.Modules.Ticketing.Domain.PromoCodes;

namespace Evently.Modules.Ticketing.Application.PromoCodes.CreatePromoCode;

internal sealed class CreatePromoCodeCommandHandler(
    IPromoCodeRepository promoCodeRepository,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreatePromoCodeCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreatePromoCodeCommand request, CancellationToken cancellationToken)
    {
        string normalizedCode = request.Code.Trim().ToUpperInvariant();

        PromoCode? existingPromoCode = await promoCodeRepository.GetByCodeAsync(normalizedCode, cancellationToken);
        if (existingPromoCode is not null)
        {
            return Result.Failure<Guid>(PromoCodeErrors.AlreadyExists);
        }

        if (!Enum.IsDefined(typeof(DiscountType), request.DiscountType))
        {
            return Result.Failure<Guid>(PromoCodeErrors.InvalidDiscountValue);
        }

        var discountType = (DiscountType)request.DiscountType;

        Result<PromoCode> result = PromoCode.Create(
            normalizedCode,
            discountType,
            request.DiscountValue,
            request.Currency.ToUpperInvariant(),
            request.MaxRedemptions,
            request.ValidFromUtc ?? dateTimeProvider.UtcNow,
            request.ValidUntilUtc);

        if (result.IsFailure)
        {
            return Result.Failure<Guid>(result.Error);
        }

        PromoCode promoCode = result.Value;

        promoCodeRepository.Insert(promoCode);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return promoCode.Id;
    }
}
