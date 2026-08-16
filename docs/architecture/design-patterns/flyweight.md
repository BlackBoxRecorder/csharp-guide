---
title: 享元模式
description: C# 享元模式（Flyweight）详解：共享细粒度对象以减少内存占用，结合奶茶订单示例讲解。
---

# 享元模式（Flyweight）

享元模式通过**共享**来减少创建对象的数量，从而降低内存占用。它把对象的属性分成两类：可以共享的**内部状态**（类型、配置）与不可共享的**外部状态**（位置、数量）。就像奶茶店：店里只有 4 种配方，但每天有几百杯订单——配方实例只需各准备一份，订单只是"引用"它们。

## 解决什么问题

当系统需要创建**大量相似对象**时，每个对象都占用独立内存：

1. **内存压力**：1000 个订单创建 1000 个 `BubbleMilkTea` 实例，其中绝大多数信息完全相同。
2. **初始化开销**：每次 `new` 都要执行构造逻辑（加载配方、打印初始化日志等），重复劳动。

享元让同类型的对象**共享同一个实例**，用工厂负责"存在即复用，不存在才创建"。

## 核心结构

| 角色 | 说明 | 示例代码 |
| :--- | :--- | :--- |
| 抽象享元 | 定义享元对象的接口 | `IBeverage` |
| 具体享元 | 被共享的对象，只含内部状态 | `BubbleMilkTea`、`FoamMilkTea` 等 |
| 享元工厂 | 维护享元池，按需创建与复用 | `BeverageFlyweightFactory` |
| 客户端 | 通过工厂获取享元对象 | `BubbleTeaShop` |

## 代码示例

仓库示例 `src/DesignPatterns/FlyweightPattern` 以奶茶订单为背景。**抽象享元**：

```csharp
public interface IBeverage
{
    void Drink();
}
```

**具体享元**——每种奶茶一个类，构造时打印初始化日志便于观察共享效果：

```csharp
public class BubbleMilkTea : IBeverage
{
    public BubbleMilkTea() => Console.WriteLine("Initializing a Bubble Milk Tea instance");

    public void Drink() => Console.WriteLine("hmmm... this is bubble milk tea");
}

public class FoamMilkTea : IBeverage
{
    public FoamMilkTea() => Console.WriteLine("Initializing a Foam Milk Tea instance");

    public void Drink() => Console.WriteLine("hmmm... this is foam milk tea");
}
```

**享元工厂**——用字典缓存已创建的实例，同类型订单直接复用：

```csharp
public class BeverageFlyweightFactory
{
    private readonly Dictionary<BeverageType, IBeverage> _beverages;

    public BeverageFlyweightFactory()
    {
        _beverages = new Dictionary<BeverageType, IBeverage>();
    }

    public IBeverage MakeBeverage(BeverageType type)
    {
        _beverages.TryGetValue(type, out var beverage);
        if (beverage == null)
        {
            beverage = type switch
            {
                BeverageType.BubbleMilk => new BubbleMilkTea(),
                BeverageType.FoamMilk => new FoamMilkTea(),
                BeverageType.OolongMilk => new OolingMilkTea(),
                BeverageType.CoconutMilk => new CoconutMilkTea(),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
            _beverages.Add(type, beverage);
        }
        return beverage;
    }
}
```

**客户端**——奶茶店连续接收 6 个订单，其中多杯同类型。注意构造日志的打印次数：

```csharp
public class BubbleTeaShop
{
    private readonly List<IBeverage> takeAwayOrders = new();

    public BubbleTeaShop()
    {
        var factory = new BeverageFlyweightFactory();

        takeAwayOrders.Add(factory.MakeBeverage(BeverageType.BubbleMilk));
        takeAwayOrders.Add(factory.MakeBeverage(BeverageType.BubbleMilk));   // 复用
        takeAwayOrders.Add(factory.MakeBeverage(BeverageType.CoconutMilk));
        takeAwayOrders.Add(factory.MakeBeverage(BeverageType.FoamMilk));
        takeAwayOrders.Add(factory.MakeBeverage(BeverageType.OolongMilk));
        takeAwayOrders.Add(factory.MakeBeverage(BeverageType.OolongMilk));   // 复用
    }

    public void Enumerate()
    {
        Console.WriteLine("Enumerating take away orders\n");
        foreach (var beverage in takeAwayOrders)
        {
            beverage.Drink();
        }
    }
}
```

运行结果——6 个订单只初始化了 4 个实例：

```text
Initializing a Bubble Milk Tea instance
Initializing a Coconut Milk Tea instance
Initializing a Foam Milk Tea instance
Initializing an Oolong Milk Tea instance
Enumerating take away orders

hmmm... this is bubble milk tea
hmmm... this is bubble milk tea
hmmm... this is coconut milk tea
hmmm... this is foam milk tea
hmmm... this is oolong milk tea
hmmm... this is oolong milk tea
```

## 内部状态与外部状态

| 状态类型 | 定义 | 处理方式 |
| :--- | :--- | :--- |
| 内部状态 | 对象固有的、可共享的信息（配方、类型） | 存放在享元对象内部 |
| 外部状态 | 随场景变化的、不可共享的信息（订单号、杯数） | 由客户端持有，调用时传入 |

示例中的奶茶配方是内部状态；若订单需要区分"加糖/去冰"，这些参数就属于外部状态，应由 `Drink(orderInfo)` 之类的调用传入，而不能塞进享元对象。

## 典型应用场景

- **字符串驻留**：CLR 的字符串驻留池（intern pool）共享相同内容的字符串字面量。
- **游戏渲染**：大量相同粒子的纹理、模型共享一个资源对象，只保存各自的变换矩阵（外部状态）。
- **字体与图标**：同一字号的文字只加载一份字形数据，位置信息由排版引擎持有。
- **数据库连接池**：连接对象本身昂贵，池化后复用。

## 总结

- **共享省内存**：同类型对象共用一份实例，内存占用从 O(N) 降到 O(种类数)。
- **工厂集中管理**：享元工厂是复用的唯一入口，"先查池、再创建"。
- **注意外部状态**：识别并分离外部状态是享元能否安全共享的前提，否则会出现数据串扰。
- **注意适用边界**：对象数量大且种类少时才值得引入享元；种类多、差异大的场景收益有限。
