using FluentValidation;

namespace Evently.Modules.Ticketing.Application.Events.RescheduleEvent;

internal sealed class RescheduleEventCommandValidator : AbstractValidator<RescheduleEventCommand>
{
    public RescheduleEventCommandValidator()
    {
        RuleFor(x => x.EventId).NotEmpty();
        RuleFor(x => x.StartAtUtc).NotEmpty();
        RuleFor(x => x.EndAtUtc)
            .Must((cmd, endAtUtc) => endAtUtc > cmd.StartAtUtc)
            .When(cmd => cmd.EndAtUtc.HasValue);
    }
}