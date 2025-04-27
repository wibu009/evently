﻿using FluentValidation;

namespace Evently.Modules.Events.Application.Categories.ArchiveCategory;

internal sealed class ArchiveCategoryCommandValidator : AbstractValidator<ArchiveCategoryCommand>
{
    public ArchiveCategoryCommandValidator()
    {
        RuleFor(x => x.CategoryId).NotEmpty();
    }
}
