using FluentValidation;

namespace Evently.Modules.Ticketing.Application.PromoCodes.DeactivatePromoCode;

internal sealed class DeactivatePromoCodeCommandValidator : AbstractValidator<DeactivatePromoCodeCommand>
{
    public DeactivatePromoCodeCommandValidator()
    {
        RuleFor(x => x.PromoCodeId).NotEmpty();
    }
}
