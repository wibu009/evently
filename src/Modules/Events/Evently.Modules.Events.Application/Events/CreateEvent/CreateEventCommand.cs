using FluentValidation;
using MediatR;

namespace Evently.Modules.Events.Application.Events.CreateEvent;

public sealed record CreateEventCommand(
    string Title, 
    string Description,
    string Location,
    DateTime StartAtUtc,
    DateTime? EndAtUtc) : IRequest<Guid>;

internal sealed class CreateEventCommandValidator : AbstractValidator<CreateEventCommand>
{
    public CreateEventCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty();
        RuleFor(x => x.Description).NotEmpty();
        RuleFor(x => x.Location).NotEmpty();
        RuleFor(x => x.StartAtUtc).NotEmpty();
        RuleFor(x => x.EndAtUtc)
            .Must((cmd, endAtUtc) => endAtUtc > cmd.StartAtUtc)
            .When(cmd => cmd.EndAtUtc.HasValue);
    }
}
