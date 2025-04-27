using Evently.Modules.Events.Application.Abstractions.Messaging;
using Evently.Modules.Events.Application.Categories.GetCategory;

namespace Evently.Modules.Events.Application.Categories.GetCategories;

public sealed record GetCategoriesQuery : IQuery<IReadOnlyCollection<CategoryResponse>>;
