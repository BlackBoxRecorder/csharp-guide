using DddOrderDemo.Domain.Common;

namespace DddOrderDemo.Domain.Inventory;

/// <summary>
/// 商品聚合根：库存数量是商品聚合的不变量，只能通过本聚合的方法修改，
/// 外部代码无法绕过校验直接改库存。
/// </summary>
public class Product
{
    public Product(string name, Money unitPrice, int stock)
    {
        Id = Guid.NewGuid();
        Name = name;
        UnitPrice = unitPrice;
        _stock = stock;
    }

    public Guid Id { get; }
    public string Name { get; }
    public Money UnitPrice { get; }

    private int _stock;
    public int Stock => _stock;

    /// <summary>预占库存：下单时扣减，库存不足抛异常</summary>
    public void Reserve(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("预占数量必须大于 0", nameof(quantity));
        if (quantity > _stock)
            throw new InvalidOperationException($"库存不足：{Name} 仅剩 {_stock} 件");

        _stock -= quantity;
    }

    /// <summary>释放库存：取消订单时归还</summary>
    public void Restore(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("释放数量必须大于 0", nameof(quantity));
        _stock += quantity;
    }
}
