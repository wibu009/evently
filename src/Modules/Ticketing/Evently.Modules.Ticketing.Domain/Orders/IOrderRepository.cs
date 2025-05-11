namespace Evently.Modules.Ticketing.Domain.Orders;

public interface IOrderRepository
{
    Task<Order?> GetAsync(Guid orderId, CancellationToken cancellationToken);
    void Insert(Order order);
}