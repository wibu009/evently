using FluentValidation;

namespace Evently.Modules.Ticketing.Application.Events.CreateEvent;

internal sealed class CreateEventCommandValidator : AbstractValidator<CreateEventCommand>
{
    public CreateEventCommandValidator()
    {
        RuleFor(x => x.EventId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty();
        RuleFor(x => x.Description).NotEmpty();
        RuleFor(x => x.Location).NotEmpty();
        RuleFor(x => x.StartAtUtc).NotEmpty();
        RuleFor(x => x.EndAtUtc)
            .Must((cmd, endAtUtc) => endAtUtc > cmd.StartAtUtc)
            .When(cmd => cmd.EndAtUtc.HasValue);

        RuleForEach(x => x.TicketTypes)
            .ChildRules(i =>
            {
                i.RuleFor(x => x.TicketTypeId).NotEmpty();
                i.RuleFor(x => x.Name).NotEmpty();
                i.RuleFor(x => x.Price).GreaterThan(decimal.Zero);
                i.RuleFor(x => x.Currency).NotEmpty();
                i.RuleFor(x => x.Quantity).GreaterThan(decimal.Zero);
            });
    }
}