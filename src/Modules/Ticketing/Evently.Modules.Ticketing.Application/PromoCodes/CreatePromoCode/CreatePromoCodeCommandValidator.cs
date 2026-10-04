using FluentValidation;

namespace Evently.Modules.Ticketing.Application.PromoCodes.CreatePromoCode;

internal sealed class CreatePromoCodeCommandValidator : AbstractValidator<CreatePromoCodeCommand>
{
    public CreatePromoCodeCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);

        RuleFor(x => x.DiscountType)
            .Must(type => type is 0 or 1)
            .WithMessage("The discount type must be either 0 (percentage) or 1 (fixed amount)");

        RuleFor(x => x.DiscountValue).GreaterThan(decimal.Zero);

        RuleFor(x => x.Currency).NotEmpty().MaximumLength(3);

        RuleFor(x => x.MaxRedemptions).GreaterThan(0).When(x => x.MaxRedemptions.HasValue);

        RuleFor(x => x.ValidUntilUtc)
            .Must((command, validUntilUtc) => validUntilUtc > command.ValidFromUtc)
            .When(command => command.ValidFromUtc.HasValue && command.ValidUntilUtc.HasValue)
            .WithMessage("The validity end must be after the validity start");
    }
}
