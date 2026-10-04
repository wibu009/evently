using FluentValidation;

namespace Evently.Modules.Events.Application.Events.RemoveEventImage;

internal sealed class RemoveEventImageCommandValidator : AbstractValidator<RemoveEventImageCommand>
{
    public RemoveEventImageCommandValidator()
    {
        RuleFor(x => x.EventId).NotEmpty();

        RuleFor(x => x.ImageId).NotEmpty();
    }
}
