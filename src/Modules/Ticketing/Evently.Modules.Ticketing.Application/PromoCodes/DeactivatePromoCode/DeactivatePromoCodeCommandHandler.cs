using Evently.Common.Application.Clock;
using Evently.Common.Application.Messaging;
using Evently.Common.Domain;
using Evently.Modules.Ticketing.Application.Abstractions.Data;
using Evently.Modules.Ticketing.Domain.PromoCodes;

namespace Evently.Modules.Ticketing.Application.PromoCodes.DeactivatePromoCode;

internal sealed class DeactivatePromoCodeCommandHandler(
    IPromoCodeRepository promoCodeRepository,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeactivatePromoCodeCommand>
{
    public async Task<Result> Handle(DeactivatePromoCodeCommand request, CancellationToken cancellationToken)
    {
        PromoCode? promoCode = await promoCodeRepository.GetAsync(request.PromoCodeId, cancellationToken);
        if (promoCode is null)
        {
            return Result.Failure(PromoCodeErrors.NotFound(request.PromoCodeId));
        }

        promoCode.Deactivate(dateTimeProvider.UtcNow);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
