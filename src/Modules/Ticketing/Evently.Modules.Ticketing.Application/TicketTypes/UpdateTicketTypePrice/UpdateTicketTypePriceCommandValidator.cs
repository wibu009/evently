using FluentValidation;

namespace Evently.Modules.Ticketing.Application.TicketTypes.UpdateTicketTypePrice;

internal sealed class UpdateTicketTypePriceCommandValidator : AbstractValidator<UpdateTicketTypePriceCommand>
{
    public UpdateTicketTypePriceCommandValidator()
    {
        RuleFor(x => x.TicketTypeId).NotEmpty();
        RuleFor(x => x.Price).GreaterThan(decimal.Zero);
    }
}