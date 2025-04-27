using FluentValidation;

namespace Evently.Modules.Events.Application.TicketTypes.CreateTicketType;

internal sealed class CreateTicketTypeCommandValidator : AbstractValidator<CreateTicketTypeCommand>
{
    public CreateTicketTypeCommandValidator()
    {
        RuleFor(x => x.EventId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Price).GreaterThan(decimal.Zero);
        RuleFor(x => x.Currency).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(decimal.Zero);
    }
}
