using FluentValidation;

namespace Evently.Modules.Ticketing.Application.Orders.CancelOrder;

internal sealed class CancelOrderCommandValidator : AbstractValidator<CancelOrderCommand>
{
    public CancelOrderCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
    }
}
