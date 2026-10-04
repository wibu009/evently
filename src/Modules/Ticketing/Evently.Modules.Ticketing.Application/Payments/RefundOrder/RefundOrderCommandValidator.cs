using FluentValidation;

namespace Evently.Modules.Ticketing.Application.Payments.RefundOrder;

internal sealed class RefundOrderCommandValidator : AbstractValidator<RefundOrderCommand>
{
    public RefundOrderCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
    }
}
