using Evently.Common.Domain;

namespace Evently.Modules.Events.Domain.Categories;

public static class CategoryErrors
{
    public static Error NotFound(Guid categoryId)
        => Error.NotFound("Categories.NotFound", $"Category with id {categoryId} not found");

    public static readonly Error AlreadyArchived =
        Error.Conflict("Categories.AlreadyArchived", "The category was already archived");
}
