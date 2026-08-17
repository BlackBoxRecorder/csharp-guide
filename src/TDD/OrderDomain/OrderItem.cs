namespace OrderDomain;

/// <summary>订单行：单价与数量在创建时固定，不可修改</summary>
public class OrderItem
{
    internal OrderItem(string productName, Money unitPrice, int quantity)
    {
        ProductName = productName;
        UnitPrice = unitPrice;
        Quantity = quantity;
    }

    public string ProductName { get; }
    public Money UnitPrice { get; }
    public int Quantity { get; }

    /// <summary>小计 = 单价 × 数量</summary>
    public Money Subtotal => UnitPrice * Quantity;
}
