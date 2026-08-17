using DddOrderDemo.Domain.Inventory;

namespace DddOrderDemo.Infrastructure;

/// <summary>内存库存仓储：演示用，仅存在于进程内</summary>
public class InMemoryInventoryRepository : IInventoryRepository
{
    private readonly Dictionary<Guid, Product> _products = [];

    public void Add(Product product) => _products[product.Id] = product;

    public Product? FindById(Guid id) => _products.GetValueOrDefault(id);

    public IReadOnlyList<Product> GetAll() => _products.Values.ToList();
}
