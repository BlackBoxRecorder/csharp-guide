---
title: 领域驱动设计（DDD）
description: C# 领域驱动设计（DDD）入门：从限界上下文、通用语言等战略设计，到实体、值对象、聚合、领域事件与仓储等战术设计，配合电商订单域可运行示例。
---

# 领域驱动设计（DDD）

领域驱动设计（Domain-Driven Design）是一套**以业务为核心**的软件设计方法。它主张：软件的价值不在于技术栈多新，而在于它是否准确表达了业务规则——而要让代码准确表达业务，首先得让技术语言和业务语言保持一致。

本文先介绍 DDD 的整体思路（战略设计），再介绍建模工具（战术设计），最后用一个电商订单域的完整示例把两者串起来。示例代码位于仓库 `src/DDD`，可直接运行。

## 解决什么问题

很多系统初期开发很快，但随着业务复杂，代码会越来越难改。典型的症状：

1. **贫血模型**：类里只有属性，业务逻辑全部堆在 Service 里，判断规则散落各处，改一处坏一片。
2. **语言鸿沟**：业务方说"下单后要锁库存"，开发实现的却是"Insert 一条 Order 记录并 Update 库存表"——两套语言，沟通全靠翻译。
3. **数据库思维绑架建模**：表结构先于领域模型设计，模型被外键和表关系牵着走，而非被业务规则驱动。

DDD 的解法是：**让领域模型成为系统的核心**，用统一语言建模，把业务规则收拢进模型内部，技术细节（数据库、消息队列）退居外围。

## 战略设计：先划清业务边界

动手写代码之前，先回答一个问题：**这个系统到底要做什么业务？**

### 通用语言（Ubiquitous Language）

业务专家和开发团队必须使用**同一套术语**：业务说"订单行"，代码里就不能叫 `OrderLine` 和 `OrderRow` 混着来；业务说"预占库存"，方法名就该是 `Reserve`。

通用语言沉淀为术语表，同时体现在代码、文档和讨论中。它是 DDD 的第一步，也是成本最低、收益最大的一步。

### 限界上下文（Bounded Context）

一套通用语言只在**一个上下文**内有效。"订单"在销售上下文里指客户下单记录，在财务上下文里指对账凭证——同一个词，含义不同。限界上下文就是**语言与模型的边界**：每个上下文有独立的模型、独立的代码，甚至独立的数据库。

电商系统可拆分为三个限界上下文：

| 限界上下文 | 职责 | 核心模型 |
| :--- | :--- | :--- |
| 订单（Ordering） | 下单、改单、状态流转 | `Order`、`OrderItem` |
| 库存（Inventory） | 商品与库存管理 | `Product` |
| 支付（Payment） | 收款与支付记录 | `Payment` |

### 子域与上下文映射

业务内部按重要程度分为**核心域**（系统的立身之本，如电商的订单）、**支撑域**（如库存）和**通用域**（如权限、通知）。资源应优先投入核心域。

上下文之间通过**上下文映射**描述协作关系：订单上下文调用库存上下文（防腐层隔离），支付上下文监听订单事件（事件驱动）。映射关系决定了集成方式，也决定了团队协作方式。

## 战术设计：建模工具

战略设计划清了边界，战术设计则在边界内部**用一组建模工具把业务规则落到代码**。这些工具按职责分工：

| 工具 | 职责 | 一句话理解 |
| :--- | :--- | :--- |
| 实体（Entity） | 有唯一标识、状态可变 | "这是哪一笔订单" |
| 值对象（Value Object） | 无标识、不可变、按值比较 | "金额 100 元就是 100 元" |
| 聚合（Aggregate） | 实体 + 值对象的一致性边界 | "订单和订单行必须一起改" |
| 领域服务（Domain Service） | 跨实体的业务规则 | "折扣怎么算" |
| 领域事件（Domain Event） | 领域内发生的事实 | "订单已支付" |
| 仓储（Repository） | 聚合的存取抽象 | "把订单放进去、取出来" |

### 实体（Entity）

实体拥有**唯一标识**（如订单号、用户 ID），标识不变，属性可变。"张三改名为李四"，还是同一个人，靠 ID 识别。实体封装自己的业务方法：`Order.Cancel()`、`Product.Reserve(3)`，而不是暴露属性让外部随意修改。

### 值对象（Value Object）

值对象描述事物的**属性**，没有标识，创建后不可变，两个值对象按内容相等。金额是典型的值对象：`100 元` 换成 `50 元 + 50 元` 后依然是 `100 元`，内容即身份。把 `Money` 做成值对象后，"金额"和"币种"永远绑定，杜绝元与美元直接相加的 bug。

### 聚合（Aggregate）

聚合是 DDD 最核心也最容易用错的工具。它回答的问题是：**哪些对象必须一起修改才能保证数据一致？**

订单和订单行必须一起修改——不能存在"订单总额 1000 元，但订单行加起来只有 800 元"的状态。于是 `Order` 是**聚合根**，`OrderItem` 是聚合内实体：外部只能通过 `Order.AddItem()` 操作订单行，无法绕过聚合直接改行。

聚合的三个规则：

1. **外部只能引用聚合根**：订单行不能脱离订单被直接访问。
2. **聚合内一致性由聚合根保证**：数量为正、状态流转合法，都在聚合根方法里校验。
3. **聚合之间用 ID 引用**：订单持有 `ProductId` 而不是整个 `Product` 对象，避免跨聚合直接修改。

### 领域服务（Domain Service）

有些规则**不属于任何单一实体**，硬塞进实体反而别扭——比如折扣规则要同时看件数和总金额。这时用领域服务承载：`OrderDomainService.CalculateDiscount(order)`。领域服务是领域层的公民，和实体平级，不是"放不下就丢进去"的杂物间。

### 领域事件（Domain Event）

领域事件记录**已发生的事实**：`OrderCreatedEvent`、`OrderPaidEvent`。聚合根在自己的方法里发布事件（记入事件列表），由应用层在事务提交后派发。事件让限界上下文之间**松耦合协作**：支付上下文不必被订单上下文直接调用，订阅事件即可。

### 仓储（Repository）

仓储是聚合的**存取接口**，定义在领域层（`IOrderRepository`），实现在基础设施层（内存、EF Core、Dapper 皆可）。领域层只依赖 `IOrderRepository` 接口，不知道数据库的存在——这也是为什么领域模型可以用纯内存跑起来、方便单元测试。

## 代码示例：电商订单域

示例工程 `src/DDD` 实现了三个限界上下文，演示上述全部战术工具。目录结构：

```text
src/DDD
├── Program.cs                     # 演示入口
├── Application/                   # 应用服务（用例编排）
│   └── OrderApplicationService.cs
├── Domain/                        # 领域层：不依赖任何基础设施
│   ├── Common/Money.cs            # 值对象：金额
│   ├── Ordering/                  # 订单限界上下文
│   │   ├── Order.cs               # 聚合根
│   │   ├── OrderItem.cs           # 聚合内实体
│   │   ├── OrderDomainService.cs  # 领域服务：折扣规则
│   │   ├── OrderEvents.cs         # 领域事件
│   │   └── IOrderRepository.cs    # 仓储接口
│   ├── Inventory/                 # 库存限界上下文
│   │   └── Product.cs             # 聚合根：库存不变量
│   └── Payment/                   # 支付限界上下文
│       └── Payment.cs             # 聚合根：一次支付不可变
└── Infrastructure/                # 基础设施层：内存仓储实现
```

### 值对象：Money

金额用 `readonly record struct` 实现：不可变、按值比较、运算符保证币种一致：

```csharp
public readonly record struct Money(decimal Amount, string Currency = "CNY")
{
    public static Money operator +(Money a, Money b)
    {
        EnsureSameCurrency(a, b);
        return new Money(a.Amount + b.Amount, a.Currency);
    }

    public static Money operator *(Money a, int factor) => new(a.Amount * factor, a.Currency);

    private static void EnsureSameCurrency(Money a, Money b)
    {
        if (a.Currency != b.Currency)
            throw new InvalidOperationException($"币种不一致：{a.Currency} 与 {b.Currency}");
    }

    public override string ToString() => $"{Amount:F2} {Currency}";
}
```

### 聚合根：Order

`Order` 用私有构造函数 + 静态工厂 `Create` 保证创建必经业务规则；订单行通过 `AddItem` 添加，**数量为正、状态待支付**两个不变量由聚合根统一把关：

```csharp
public class Order
{
    private readonly List<OrderItem> _items = [];
    private readonly List<DomainEvent> _events = [];

    private Order(Guid id, Guid customerId) { ... }   // 私有构造函数

    public static Order Create(Guid customerId)        // 静态工厂
    {
        var order = new Order(Guid.NewGuid(), customerId);
        order._events.Add(new OrderCreatedEvent(order.Id, order.CustomerId));
        return order;
    }

    public OrderStatus Status { get; private set; }    // 状态只能内部流转
    public IReadOnlyList<OrderItem> Items => _items;   // 只读视图

    // 订单总额由行实时累加，任何时刻保持一致
    public Money TotalAmount => _items.Aggregate(Money.Zero(), (sum, item) => sum + item.Subtotal);

    public void AddItem(Guid productId, string productName, Money unitPrice, int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("商品数量必须大于 0", nameof(quantity));
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("只有待支付的订单才能修改商品");

        _items.Add(new OrderItem(productId, productName, unitPrice, quantity));
    }

    public void Pay(Money discount)                    // 状态机：杜绝重复支付
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("订单当前状态不可支付");

        Status = OrderStatus.Paid;
        _events.Add(new OrderPaidEvent(Id, TotalAmount, discount, TotalAmount - discount));
    }
}
```

注意 `OrderItem` 的构造函数是 `internal`——**订单行只能由聚合根创建**，外部代码无法构造"游离"的订单行，一致性边界由此物理强制（完整代码见 `OrderItem.cs`）。

### 领域服务：折扣规则

折扣规则要看件数、要看总额，不属于任何单一实体，由领域服务承载：

```csharp
public static class OrderDomainService
{
    /// <summary>满 3 件打 9 折；否则满 500 元减 50 元；都不满足则无折扣</summary>
    public static Money CalculateDiscount(Order order)
    {
        var total = order.TotalAmount;
        var itemCount = order.Items.Sum(item => item.Quantity);

        if (itemCount >= 3)
            return total * 0.1m;

        if (total.Amount >= 500)
            return new Money(50m, total.Currency);

        return Money.Zero(total.Currency);
    }
}
```

### 应用服务：用例编排

应用服务不属于领域层，它只做三件事：**翻译请求 → 调用领域对象 → 保存结果**。下单用例协调订单与库存两个限界上下文：

```csharp
public class OrderApplicationService
{
    // 1. 先整体校验库存，避免只扣减了部分商品
    // 2. 创建订单聚合，逐行添加商品（数量为正由 Order 保证）
    // 3. 调用 product.Reserve(quantity) 预占库存

    public Order PlaceOrder(Guid customerId, params (Guid ProductId, int Quantity)[] requests)
    {
        foreach (var (productId, quantity) in requests)
        {
            var product = _inventory.FindById(productId)
                ?? throw new InvalidOperationException($"商品 {productId} 不存在");
            if (product.Stock < quantity)
                throw new InvalidOperationException($"库存不足：{product.Name} 仅剩 {product.Stock} 件");
        }

        var order = Order.Create(customerId);
        foreach (var (productId, quantity) in requests)
        {
            var product = _inventory.FindById(productId)!;
            order.AddItem(product.Id, product.Name, product.UnitPrice, quantity);
            product.Reserve(quantity);
        }

        _orders.Add(order);
        return order;
    }
}
```

### 运行结果

运行 `dotnet run --project src/DDD`（仓库完整示例）输出如下：

```text
== 库存商品 ==
名称        单价          库存
笔记本电脑     5999.00 CNY 10
机械键盘      899.00 CNY  20
显示器       1299.00 CNY 8

== 下单成功：订单 b50f2e8c-… ==
商品名称        单价          数量    小计
笔记本电脑       5999.00 CNY 2     11998.00 CNY
机械键盘        899.00 CNY  1     899.00 CNY
订单总额：12897.00 CNY

== 支付成功 ==
订单状态：Paid
订单总额：12897.00 CNY，优惠：1289.70 CNY，实付：11607.30 CNY

== 订单列表 ==
订单 b50f2e8c-… | 客户 04550186-… | 总额 12897.00 CNY | 状态 Paid
  [领域事件] OrderCreatedEvent @ 23:57:43
  [领域事件] OrderPaidEvent @ 23:57:43
```

买 2 台笔记本加 1 把键盘共 3 件，触发"满 3 件打 9 折"，优惠 1289.70 元；订单事件随聚合产生并被应用层记录，两个限界上下文（库存预占、支付记录）通过应用服务协作完成整个用例。

## DDD 与 EF Core 的关系

本示例刻意用**内存仓储**，把注意力放在领域建模上。实际项目中仓储实现常用 EF Core：聚合映射为一个 `DbContext` 内的根实体（`Order` 对应 `Orders` 表，`OrderItem` 对应 `OrderItems` 表），聚合的加载与保存由 `DbContext` 完成，领域层依然不感知数据库。

需要注意两点：

- **仓储接口语义是"以聚合为单位"存取**，不是逐表 CRUD；EF Core 的 `DbSet<Order>` 天然贴合这一语义。
- **聚合与表不是一一对应的**：一个聚合可以跨多张表（Order + OrderItem），一张表也可能不属于任何聚合（报表数据）。先建模，再定映射，顺序不要颠倒。

## 典型应用场景

- **复杂业务系统**：订单、计费、保险核保等规则密集的领域，DDD 的建模工具能有效管理复杂度。
- **微服务拆分依据**：限界上下文是最自然的服务边界，先划上下文再拆服务，避免"按表拆服务"。
- **需要长期演进的系统**：聚合与仓储的边界让重构局部化，领域模型可脱离基础设施独立演进与测试。
- **不适合的场景**：简单的 CRUD 后台、以报表为主的系统，强行套 DDD 只会增加复杂度，不划算。

## 总结

- **战略设计定边界**：通用语言对齐认知，限界上下文划分模型边界，子域决定资源投入。
- **战术设计落代码**：实体与值对象区分"是谁"与"是什么"，聚合保证一致性，领域服务承载跨实体规则，领域事件解耦上下文，仓储隔离基础设施。
- **分层架构**：领域层是核心且零依赖，应用层编排用例，基础设施层实现仓储等细节。
- **保持克制**：DDD 不是银弹，复杂度低的系统用简单设计更务实；从最小限界上下文与最小聚合开始，边做边学。

## 参考链接

- 设计模式栏目：[设计模式概览](./../design-patterns/index)
- 测试驱动开发（TDD）：以测试驱动领域建模的配套实践，见 [测试驱动开发（TDD）](./../tdd/index)
