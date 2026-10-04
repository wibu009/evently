using FluentValidation;

namespace Evently.Modules.Ticketing.Application.Orders.CreateOrder;

internal sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();

        RuleFor(x => x.PromoCode)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.PromoCode));
    }
}
