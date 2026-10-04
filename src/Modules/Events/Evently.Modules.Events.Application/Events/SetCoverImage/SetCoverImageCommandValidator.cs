using FluentValidation;

namespace Evently.Modules.Events.Application.Events.SetCoverImage;

internal sealed class SetCoverImageCommandValidator : AbstractValidator<SetCoverImageCommand>
{
    public SetCoverImageCommandValidator()
    {
        RuleFor(x => x.EventId).NotEmpty();

        RuleFor(x => x.ImageId).NotEmpty();
    }
}
