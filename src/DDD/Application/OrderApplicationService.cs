using DddOrderDemo.Domain.Inventory;
using DddOrderDemo.Domain.Ordering;
using DddOrderDemo.Domain.Payment;

namespace DddOrderDemo.Application;

/// <summary>
/// 应用服务（用例层）：编排「下单」「支付」用例，协调多个限界上下文。
/// 它不包含业务规则——规则在领域层，应用服务只负责翻译请求、调用领域对象、保存结果。
/// </summary>
public class OrderApplicationService
{
    private readonly IOrderRepository _orders;
    private readonly IInventoryRepository _inventory;
    private readonly IPaymentRepository _payments;

    public OrderApplicationService(IOrderRepository orders, IInventoryRepository inventory, IPaymentRepository payments)
    {
        _orders = orders;
        _inventory = inventory;
        _payments = payments;
    }

    /// <summary>下单用例：校验库存 → 创建订单聚合 → 扣减库存</summary>
    public Order PlaceOrder(Guid customerId, params (Guid ProductId, int Quantity)[] requests)
    {
        // 1. 先整体校验库存，避免只扣减了部分商品
        foreach (var (productId, quantity) in requests)
        {
            var product = _inventory.FindById(productId)
                ?? throw new InvalidOperationException($"商品 {productId} 不存在");
            if (product.Stock < quantity)
                throw new InvalidOperationException($"库存不足：{product.Name} 仅剩 {product.Stock} 件");
        }

        // 2. 创建订单聚合，逐行添加商品（数量为正的不变量由 Order 保证）
        var order = Order.Create(customerId);
        foreach (var (productId, quantity) in requests)
        {
            var product = _inventory.FindById(productId)!;
            order.AddItem(product.Id, product.Name, product.UnitPrice, quantity);
            product.Reserve(quantity); // 3. 预占库存
        }

        _orders.Add(order);
        return order;
    }

    /// <summary>支付用例：计算折扣 → 订单状态流转 → 记录支付</summary>
    public Payment PayOrder(Guid orderId)
    {
        var order = _orders.FindById(orderId)
            ?? throw new InvalidOperationException($"订单 {orderId} 不存在");

        var discount = OrderDomainService.CalculateDiscount(order);
        order.Pay(discount); // 状态流转 + 发布领域事件

        var payment = Payment.Pay(order.Id, order.TotalAmount - discount);
        _payments.Add(payment);
        return payment;
    }
}
