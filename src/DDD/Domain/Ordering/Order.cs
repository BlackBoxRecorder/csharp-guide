using DddOrderDemo.Domain.Common;

namespace DddOrderDemo.Domain.Ordering;

/// <summary>
/// 订单聚合根：以订单为中心，统一保证"订单行、总额、状态"之间的不变量。
/// 外部只能通过公开方法修改订单，私有构造函数 + 静态工厂阻止绕过规则的创建。
/// </summary>
public class Order
{
    private readonly List<OrderItem> _items = [];
    private readonly List<DomainEvent> _events = [];

    private Order(Guid id, Guid customerId)
    {
        Id = id;
        CustomerId = customerId;
        Status = OrderStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>工厂方法：创建订单并记录创建事件</summary>
    public static Order Create(Guid customerId)
    {
        var order = new Order(Guid.NewGuid(), customerId);
        order._events.Add(new OrderCreatedEvent(order.Id, order.CustomerId));
        return order;
    }

    public Guid Id { get; }
    public Guid CustomerId { get; }
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; }

    /// <summary>订单行：只读视图，避免外部直接修改集合</summary>
    public IReadOnlyList<OrderItem> Items => _items;

    /// <summary>聚合产生的领域事件（如订单创建、支付成功）</summary>
    public IReadOnlyList<DomainEvent> Events => _events;

    /// <summary>订单总额：由各行小计实时累加，任何时刻保持一致</summary>
    public Money TotalAmount => _items.Aggregate(Money.Zero(), (sum, item) => sum + item.Subtotal);

    /// <summary>添加订单行：数量必须为正，且只有待支付订单允许修改</summary>
    public void AddItem(Guid productId, string productName, Money unitPrice, int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("商品数量必须大于 0", nameof(quantity));
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("只有待支付的订单才能修改商品");

        _items.Add(new OrderItem(productId, productName, unitPrice, quantity));
    }

    /// <summary>支付：只有待支付订单能完成支付，杜绝重复支付</summary>
    public void Pay(Money discount)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("订单当前状态不可支付");

        Status = OrderStatus.Paid;
        _events.Add(new OrderPaidEvent(Id, TotalAmount, discount, TotalAmount - discount));
    }

    /// <summary>取消订单：同样受状态机约束</summary>
    public void Cancel()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("订单当前状态不可取消");
        Status = OrderStatus.Cancelled;
    }
}
