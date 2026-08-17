namespace DddOrderDemo.Domain.Inventory;

/// <summary>库存仓储接口：领域层定义抽象，基础设施层实现</summary>
public interface IInventoryRepository
{
    void Add(Product product);
    Product? FindById(Guid id);
    IReadOnlyList<Product> GetAll();
}
