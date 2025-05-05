using Evently.Common.Application.Caching;
using Evently.Modules.Events.Application.Categories.GetCategory;

namespace Evently.Modules.Events.Application.Categories.GetCategories;

public sealed record GetCategoriesQuery : ICacheQuery<IReadOnlyCollection<CategoryResponse>>
{
    public string CacheKey => "categories";
}
