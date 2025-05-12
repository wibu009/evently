using Evently.Common.Application.Caching;
using Evently.Common.Application.Messaging;

namespace Evently.Modules.Events.Application.Categories.GetCategory;

public sealed record GetCategoryQuery(Guid CategoryId) : ICacheQuery<CategoryResponse>
{
    public string CacheKey => $"category:{CategoryId}";
}
