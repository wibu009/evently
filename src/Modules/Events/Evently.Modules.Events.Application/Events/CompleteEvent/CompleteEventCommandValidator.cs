using FluentValidation;

namespace Evently.Modules.Events.Application.Events.CompleteEvent;

internal sealed class CompleteEventCommandValidator : AbstractValidator<CompleteEventCommand>
{
    public CompleteEventCommandValidator()
    {
        RuleFor(x => x.EventId).NotEmpty();
    }
}
