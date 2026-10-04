using FluentValidation;

namespace Evently.Modules.Events.Application.Events.AddEventImage;

internal sealed class AddEventImageCommandValidator : AbstractValidator<AddEventImageCommand>
{
    public AddEventImageCommandValidator()
    {
        RuleFor(x => x.EventId).NotEmpty();

        RuleFor(x => x.ImageUrl)
            .NotEmpty()
            .MaximumLength(500)
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out Uri? uri) &&
                         (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            .WithMessage("The image URL must be a valid absolute http(s) URL");
    }
}
