using DddOrderDemo.Domain.Common;

namespace DddOrderDemo.Domain.Ordering;

/// <summary>
/// 订单行：聚合内的实体，依赖聚合根而存在。
/// 构造函数为 internal，只能由 Order 聚合根创建，外部无法脱离订单添加商品。
/// </summary>
public class OrderItem
{
    internal OrderItem(Guid productId, string productName, Money unitPrice, int quantity)
    {
        ProductId = productId;
        ProductName = productName;
        UnitPrice = unitPrice;
        Quantity = quantity;
    }

    public Guid ProductId { get; }
    public string ProductName { get; }
    public Money UnitPrice { get; }
    public int Quantity { get; }

    /// <summary>小计 = 单价 × 数量</summary>
    public Money Subtotal => UnitPrice * Quantity;
}
