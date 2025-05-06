using FluentValidation;

namespace Evently.Modules.Ticketing.Application.Carts.AddItemToCart;

internal sealed class AddItemToCartCommandValidator : AbstractValidator<AddItemToCartCommand>
{
    public AddItemToCartCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.TicketTypeId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(decimal.Zero);
    }
}