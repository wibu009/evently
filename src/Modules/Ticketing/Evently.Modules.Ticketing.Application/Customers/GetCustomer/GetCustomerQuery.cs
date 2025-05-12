using Evently.Common.Application.Caching;

namespace Evently.Modules.Ticketing.Application.Customers.GetCustomer;

public sealed record GetCustomerQuery(Guid CustomerId) : ICacheQuery<CustomerResponse>
{
    public string CacheKey => $"customer:{CustomerId}";
}
