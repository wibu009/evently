using FluentValidation;

namespace Evently.Modules.Ticketing.Application.Orders.ProcessPayment;

internal sealed class ProcessPaymentCommandValidator : AbstractValidator<ProcessPaymentCommand>
{
    public ProcessPaymentCommandValidator()
    {
        RuleFor(x => x.PaymentId).NotEmpty();
    }
}
