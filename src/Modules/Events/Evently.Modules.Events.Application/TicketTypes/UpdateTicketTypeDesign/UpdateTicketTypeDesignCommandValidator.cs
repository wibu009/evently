using FluentValidation;

namespace Evently.Modules.Events.Application.TicketTypes.UpdateTicketTypeDesign;

internal sealed class UpdateTicketTypeDesignCommandValidator : AbstractValidator<UpdateTicketTypeDesignCommand>
{
    public UpdateTicketTypeDesignCommandValidator()
    {
        RuleFor(x => x.TicketTypeId).NotEmpty();

        RuleFor(x => x.Color)
            .Matches("^#[0-9a-fA-F]{6}$")
            .WithMessage("The ticket color must be a hex color like #7C3AED")
            .When(x => !string.IsNullOrWhiteSpace(x.Color));

        RuleFor(x => x.BackgroundImageUrl)
            .MaximumLength(500)
            .Must(url => string.IsNullOrWhiteSpace(url) ||
                         (Uri.TryCreate(url, UriKind.Absolute, out Uri? uri) &&
                          (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)))
            .WithMessage("The background image URL must be a valid absolute http(s) URL")
            .When(x => !string.IsNullOrWhiteSpace(x.BackgroundImageUrl));
    }
}
