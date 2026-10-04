using FluentValidation;

namespace Evently.Modules.Ticketing.Application.PromoCodes.RedeemPromoCode;

internal sealed class RedeemPromoCodeCommandValidator : AbstractValidator<RedeemPromoCodeCommand>
{
    public RedeemPromoCodeCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
    }
}
