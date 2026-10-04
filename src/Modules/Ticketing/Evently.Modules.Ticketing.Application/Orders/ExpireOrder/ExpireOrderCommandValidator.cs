using FluentValidation;

namespace Evently.Modules.Ticketing.Application.Orders.ExpireOrder;

internal sealed class ExpireOrderCommandValidator : AbstractValidator<ExpireOrderCommand>
{
    public ExpireOrderCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
    }
}
