using DddOrderDemo.Application;
using DddOrderDemo.Domain.Common;
using DddOrderDemo.Domain.Inventory;
using DddOrderDemo.Domain.Ordering;
using DddOrderDemo.Infrastructure;

// ========== 1. 准备仓储与商品 ==========
var inventory = new InMemoryInventoryRepository();
var orders = new InMemoryOrderRepository();
var payments = new InMemoryPaymentRepository();

var laptop = new Product("笔记本电脑", new Money(5999m), 10);
var keyboard = new Product("机械键盘", new Money(899m), 20);
var monitor = new Product("显示器", new Money(1299m), 8);
inventory.Add(laptop);
inventory.Add(keyboard);
inventory.Add(monitor);

Console.WriteLine("== 库存商品 ==");
PrintProducts(inventory.GetAll());

// ========== 2. 应用服务编排下单用例 ==========
var service = new OrderApplicationService(orders, inventory, payments);
var order = service.PlaceOrder(
    customerId: Guid.NewGuid(),
    (laptop.Id, 2),   // 2 台笔记本
    (keyboard.Id, 1)  // 1 把键盘（共 3 件，触发 9 折）
);

Console.WriteLine($"\n== 下单成功：订单 {order.Id} ==");
PrintOrder(order);

// ========== 3. 支付用例：计算折扣 → 状态流转 → 记录支付 ==========
var payment = service.PayOrder(order.Id);
Console.WriteLine($"\n== 支付成功 ==");
Console.WriteLine($"订单状态：{order.Status}");
Console.WriteLine($"订单总额：{order.TotalAmount}，优惠：{OrderDomainService.CalculateDiscount(order)}，实付：{payment.Amount}");

// ========== 4. 查询订单与领域事件 ==========
Console.WriteLine("\n== 订单列表 ==");
foreach (var o in orders.GetAll())
{
    Console.WriteLine($"订单 {o.Id} | 客户 {o.CustomerId} | 总额 {o.TotalAmount} | 状态 {o.Status}");
    foreach (var e in o.Events)
        Console.WriteLine($"  [领域事件] {e.GetType().Name} @ {e.OccurredAt:HH:mm:ss}");
}

// 局部函数：以表格形式输出商品
void PrintProducts(IReadOnlyList<Product> products)
{
    Console.WriteLine("名称".PadRight(10) + "单价".PadRight(12) + "库存");
    foreach (var p in products)
        Console.WriteLine(p.Name.PadRight(10) + p.UnitPrice.ToString().PadRight(12) + p.Stock);
}

// 局部函数：以表格形式输出订单明细
void PrintOrder(Order o)
{
    Console.WriteLine("商品名称".PadRight(12) + "单价".PadRight(12) + "数量".PadRight(6) + "小计");
    foreach (var item in o.Items)
        Console.WriteLine(item.ProductName.PadRight(12) + item.UnitPrice.ToString().PadRight(12) + item.Quantity.ToString().PadRight(6) + item.Subtotal);
    Console.WriteLine($"订单总额：{o.TotalAmount}");
}
