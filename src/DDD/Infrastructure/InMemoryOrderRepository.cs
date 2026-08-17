using DddOrderDemo.Domain.Ordering;

namespace DddOrderDemo.Infrastructure;

/// <summary>内存订单仓储：演示用，仅存在于进程内</summary>
public class InMemoryOrderRepository : IOrderRepository
{
    private readonly Dictionary<Guid, Order> _orders = [];

    public void Add(Order order) => _orders[order.Id] = order;

    public Order? FindById(Guid id) => _orders.GetValueOrDefault(id);

    public IReadOnlyList<Order> GetAll() => _orders.Values.ToList();
}
