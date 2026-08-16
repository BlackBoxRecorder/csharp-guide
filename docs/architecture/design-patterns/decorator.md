---
title: 装饰器模式
description: C# 装饰器模式（Decorator）详解：动态地为对象添加职责，比继承更灵活，结合咖啡加料示例讲解。
---

# 装饰器模式（Decorator）

装饰器模式**动态地**为对象添加职责，且无需修改对象本身。就像点咖啡：先选一种基底（浓缩、深焙、家常拼配），再按需叠加配料（摩卡、奶泡）。配料不改变咖啡的类型，只是层层包裹，为它增加描述与价格。

## 解决什么问题

如果"咖啡 + 配料组合"用继承表达，会迅速失控：

1. **类爆炸**：基料 3 种 × 配料组合 2ⁿ 种 = 需要几十个类（`EspressoWithMocha`、`DarkRoastWithMochaAndWhip`……）。
2. **组合爆炸**：每加一种新配料，所有组合类都要重新排列组合。
3. **复用困难**：行为绑定在继承链上，无法在运行时自由增减。

装饰器把每个配料做成一个**包裹层**，运行时按需嵌套。加料就是"穿衣服"，想脱就换一层新的。

## 核心结构

| 角色 | 说明 | 示例代码 |
| :--- | :--- | :--- |
| 抽象组件 | 定义被装饰对象的接口 | `Beverage` |
| 具体组件 | 被装饰的原始对象 | `Espresso`、`DarkRoast`、`HouseBlend` |
| 装饰器基类 | 继承组件，并持有组件引用 | `CondimentDecorator` |
| 具体装饰器 | 为对象添加具体职责 | `MochaCondiment`、`WhipCondiment` |

关键在"同类型包裹"：装饰器本身也是 `Beverage`，所以它可以继续被别的装饰器包裹，形成任意深度。

## 代码示例

仓库示例 `src/DesignPatterns/DecoratorPattern` 以咖啡店为背景。**抽象组件**：

```csharp
public abstract class Beverage
{
    protected string _description = "No Description";
    public abstract string Description { get; }
    public abstract double Cost();
}
```

**具体组件**——三种咖啡基底，各自给出描述与价格：

```csharp
public class Espresso : Beverage
{
    public Espresso() => _description = "Espresso";
    public override string Description => _description;
    public override double Cost() => 1.99;
}

public class DarkRoast : Beverage
{
    public DarkRoast() => _description = "Dark Roast";
    public override string Description => _description;
    public override double Cost() => 1.49;
}

public class HouseBlend : Beverage
{
    public HouseBlend() => _description = "House Blend";
    public override string Description => _description;
    public override double Cost() => 2.49;
}
```

**装饰器基类**——继承 `Beverage`，使装饰器可以被继续装饰：

```csharp
public abstract class CondimentDecorator : Beverage
{
    public abstract override string Description { get; }
}
```

**具体装饰器**——摩卡包裹一层 `Beverage`，在原有描述与价格上叠加；连续包两层自动变"Double Mocha"：

```csharp
public class MochaCondiment : CondimentDecorator
{
    private readonly Beverage _beverage;

    public MochaCondiment(Beverage beverage) => _beverage = beverage;

    public override string Description
    {
        get
        {
            return _beverage.Description.StartsWith("Mocha")
                ? "Double " + _beverage.Description   // 叠两层摩卡
                : "Mocha " + _beverage.Description;
        }
    }

    public override double Cost() => 0.2 + _beverage.Cost();
}
```

奶泡装饰器结构相同，每层加 0.15 元：

```csharp
public class WhipCondiment : CondimentDecorator
{
    private readonly Beverage _beverage;

    public WhipCondiment(Beverage beverage) => _beverage = beverage;

    public override string Description
        => _beverage.Description.StartsWith("Whip")
            ? "Double " + _beverage.Description
            : "Whip " + _beverage.Description;

    public override double Cost() => 0.15 + _beverage.Cost();
}
```

演示入口（完整代码见 `Program.cs`）——运行时任意组合：

```csharp
Beverage beverage = new Espresso();
Console.WriteLine(beverage.Description + " $" + beverage.Cost());

Beverage beverage2 = new DarkRoast();
beverage2 = new MochaCondiment(beverage2);       // 加一份摩卡
beverage2 = new MochaCondiment(beverage2);       // 再加一份（Double Mocha）
beverage2 = new WhipCondiment(beverage2);        // 加奶泡
Console.WriteLine(beverage2.Description + " $" + beverage2.Cost());

Beverage beverage3 = new HouseBlend();
beverage3 = new MochaCondiment(beverage3);
beverage3 = new WhipCondiment(beverage3);
Console.WriteLine(beverage3.Description + " $" + beverage3.Cost());
```

运行结果：

```text
Espresso $1.99
Double Mocha Dark Roast $2.34
Mocha Whip House Blend $2.84
```

`2.34 = 1.49 + 0.2 + 0.2 + 0.15`，每层装饰器只负责自己那一份职责，价格层层叠加。

## 装饰器与继承的对比

| 对比项 | 继承 | 装饰器 |
| :--- | :--- | :--- |
| 组合方式 | 编译期固定 | 运行时动态嵌套 |
| 类数量 | 组合爆炸 | 每职责一个类 |
| 职责叠加 | 逐层子类化 | 任意顺序、任意层数 |
| 对原对象的影响 | 影响整个类族 | 只影响被包裹的单个实例 |

## 典型应用场景

- **流处理**：`BufferedStream` 包 `FileStream` 包 `MemoryStream`，层层加缓冲、压缩、加密——.NET 标准库中装饰器的典型应用。
- **日志增强**：给基础 `ILogger` 叠加时间戳、级别过滤、重试等装饰器。
- **中间件管道**：ASP.NET Core 的 `UseMiddleware` 管道、HTTP 客户端 `DelegatingHandler` 都是装饰器思想。
- **权限校验**：对数据访问对象动态包裹鉴权层，业务代码无感知。

## 总结

- **运行时加职责**：用包裹代替继承，组合爆炸变为线性扩展。
- **开闭原则**：新增配料只需新增一个装饰器类。
- **注意装饰器透明性**：装饰器应保持与被装饰对象的接口一致，避免类型判断破坏动态叠加。
- **注意小对象泛滥**：装饰器嵌套会产生大量小对象，恰当使用（如流处理）则收益远大于成本。
