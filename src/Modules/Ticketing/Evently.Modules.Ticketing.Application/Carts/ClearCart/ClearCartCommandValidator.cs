using FluentValidation;

namespace Evently.Modules.Ticketing.Application.Carts.ClearCart;

internal sealed class ClearCartCommandValidator : AbstractValidator<ClearCartCommand>
{
    public ClearCartCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
    }
}