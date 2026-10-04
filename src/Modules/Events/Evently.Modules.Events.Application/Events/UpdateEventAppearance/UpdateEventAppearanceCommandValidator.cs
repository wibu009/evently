using System.Text.RegularExpressions;
using FluentValidation;

namespace Evently.Modules.Events.Application.Events.UpdateEventAppearance;

internal sealed class UpdateEventAppearanceCommandValidator : AbstractValidator<UpdateEventAppearanceCommand>
{
    public UpdateEventAppearanceCommandValidator()
    {
        RuleFor(x => x.EventId).NotEmpty();

        RuleFor(x => x.HeroBannerUrl)
            .MaximumLength(500)
            .Must(url => string.IsNullOrWhiteSpace(url) ||
                         (Uri.TryCreate(url, UriKind.Absolute, out Uri? uri) &&
                          (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)))
            .WithMessage("The hero banner URL must be a valid absolute http(s) URL")
            .When(x => !string.IsNullOrWhiteSpace(x.HeroBannerUrl));

        RuleFor(x => x.AccentColor)
            .Matches("^#[0-9a-fA-F]{6}$")
            .WithMessage("The accent color must be a hex color like #7C3AED")
            .When(x => !string.IsNullOrWhiteSpace(x.AccentColor));
    }
}
