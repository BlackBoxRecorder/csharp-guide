namespace DddOrderDemo.Domain.Ordering;

/// <summary>
/// 订单仓储接口：定义在领域层，由基础设施层实现。
/// 领域层只依赖抽象，不关心数据库细节。
/// </summary>
public interface IOrderRepository
{
    void Add(Order order);
    Order? FindById(Guid id);
    IReadOnlyList<Order> GetAll();
}
