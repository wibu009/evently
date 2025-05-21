using FluentValidation;

namespace Evently.Modules.Ticketing.Application.Carts.RemoveItemFromCart;

internal sealed class RemoveItemFromCartCommandValidator : AbstractValidator<RemoveItemFromCartCommand>
{
    public RemoveItemFromCartCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.TicketTypeId).NotEmpty();
    }
}