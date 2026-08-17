namespace OrderDomain;

/// <summary>订单聚合根（TDD 演示版）：总额由行实时累加，状态受状态机约束</summary>
public class Order
{
    private readonly List<OrderItem> _items = [];

    public Guid Id { get; } = Guid.NewGuid();
    public OrderStatus Status { get; private set; } = OrderStatus.Pending;

    /// <summary>订单行：只读视图，只能通过 AddItem 添加</summary>
    public IReadOnlyList<OrderItem> Items => _items;

    /// <summary>订单总额：各行小计之和</summary>
    public Money TotalAmount => _items.Aggregate(Money.Zero(), (sum, item) => sum + item.Subtotal);

    /// <summary>添加订单行：数量必须为正，且只有待支付订单允许修改</summary>
    public void AddItem(string productName, Money unitPrice, int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("商品数量必须大于 0", nameof(quantity));
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("只有待支付的订单才能添加商品");

        _items.Add(new OrderItem(productName, unitPrice, quantity));
    }

    /// <summary>支付：只有待支付订单能完成支付，杜绝重复支付</summary>
    public void Pay()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("订单当前状态不可支付");
        Status = OrderStatus.Paid;
    }

    /// <summary>取消：只有待支付订单可取消</summary>
    public void Cancel()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("订单当前状态不可取消");
        Status = OrderStatus.Cancelled;
    }
}
