---
title: 建造者模式
description: C# 建造者模式（Builder）详解：分步构建复杂对象，将构造过程与表示分离，结合汉堡制作示例讲解。
---

# 建造者模式（Builder）

建造者模式将**复杂对象的构建过程**与**对象的最终表示**分离：同一个构建过程（导演指挥的步骤序列）可以装配出不同配置的成品。就像同一个厨师，用同一套工序，按不同配方能做出不同的汉堡。

## 解决什么问题

当一个对象的构造需要**多个步骤**，且步骤的组合方式随需求变化时，直接写构造函数会面临：

1. **构造函数爆炸**：参数越来越多（大小、形状、配料……），调用方难以分辨每个参数的含义，也容易传错顺序。
2. **步骤无法复用**：不同的成品（"我的汉堡"、"妻子的汉堡"）虽然步骤相同，但每一步的实现不同，代码会在客户端重复散落。

建造者把"步骤序列"固化在导演（Director）中，把"每一步怎么做"下放到不同的具体建造者中。客户端只需要选定一个建造者，剩下的交给导演。

## 核心结构

| 角色 | 说明 | 示例代码 |
| :--- | :--- | :--- |
| 产品 | 被构建的复杂对象 | `Hamburger` |
| 抽象建造者 | 声明构建产品的各个步骤 | `IBuilder` |
| 具体建造者 | 实现每个步骤的具体做法，并维护半成品 | `MyHamburgerBuilder`、`WifesHamburgerBuilder` |
| 导演 | 编排步骤的执行顺序，不关心具体实现 | `Cook` |

## 代码示例

仓库示例 `src/DesignPatterns/BuilderPattern` 以制作汉堡为背景。产品是一个包含配料、尺寸、形状的汉堡：

```csharp
public class Hamburger
{
    public int Size { get; set; }
    public string Shape { get; set; }
    public string[] Ingredients { get; set; }

    public override string ToString()
        => $"Ingredients: {string.Join(" ", Ingredients)}, Size: {Size}, Shape: {Shape}";
}
```

**抽象建造者**声明三个构建步骤，外加 `Reset()`（开始新的一次构建）和 `Build()`（取回成品）：

```csharp
public interface IBuilder
{
    void AddIngredients();
    void AddShape();
    void AddSize();
    void Reset();
    Hamburger Build();
}
```

**具体建造者**决定每一步的细节。"我的汉堡"加面包、肉、番茄、沙拉和蛋黄酱：

```csharp
public class MyHamburgerBuilder : IBuilder
{
    private Hamburger _hamburger;

    public void AddIngredients()
        => _hamburger.Ingredients = new[] { "Bread", "Meat", "Tomato", "Salad", "Mayonnaise" };

    public void AddShape() => _hamburger.Shape = "Kite";
    public void AddSize() => _hamburger.Size = 10;   // 英寸

    public void Reset() => _hamburger = new Hamburger();
    public Hamburger Build() => _hamburger;
}
```

"妻子的汉堡"则只加面包和沙拉，尺寸也更小，但**步骤顺序完全相同**：

```csharp
public class WifesHamburgerBuilder : IBuilder
{
    private Hamburger _hamburger;

    public void AddIngredients() => _hamburger.Ingredients = new[] { "Bread", "Salad" };
    public void AddShape() => _hamburger.Shape = "Cuboid";
    public void AddSize() => _hamburger.Size = 6;    // 英寸

    public void Reset() => _hamburger = new Hamburger();
    public Hamburger Build() => _hamburger;
}
```

**导演**只负责按固定顺序调用步骤，并支持运行时更换建造者：

```csharp
public class Cook
{
    private IBuilder _builder;

    public Cook(IBuilder builder) => AcceptBuilder(builder);

    public void ChangeBuilder(IBuilder builder) => AcceptBuilder(builder);

    public Hamburger Build()
    {
        _builder.AddIngredients();
        _builder.AddShape();
        _builder.AddSize();
        return _builder.Build();
    }

    private void AcceptBuilder(IBuilder builder)
    {
        _builder = builder;
        _builder.Reset();
    }
}
```

演示入口（完整代码见 `Program.cs`）：

```csharp
var builder = new MyHamburgerBuilder();
var cook = new Cook(builder);
var myHamburger = cook.Build();

cook.ChangeBuilder(new WifesHamburgerBuilder());
var wifesHamburger = cook.Build();

Console.WriteLine($"My Hamburger: {myHamburger}");
Console.WriteLine($"My Wife's Hamburger: {wifesHamburger}");
```

运行结果：

```text
My Hamburger: Ingredients: Bread Meat Tomato Salad Mayonnaise , Size: 10, Shape: Kite
My Wife's Hamburger: Ingredients: Bread Salad , Size: 6, Shape: Cuboid
```

## 与工厂模式的区别

| 对比项 | 工厂模式 | 建造者模式 |
| :--- | :--- | :--- |
| 构建粒度 | 一次性返回成品 | 分步骤构建，步骤可定制 |
| 关注点 | 创建哪个产品 | 产品如何一步步组装 |
| 典型场景 | 产品类型多、构造简单 | 产品内部结构复杂、步骤固定 |

## 典型应用场景

- **配置对象的构建**：`DbContextOptionsBuilder`、`HostBuilder` 都是典型建造者，链式调用 `UseSqlite(...)`、`AddControllers(...)` 逐步组装配置。
- **文档生成器**：同一份数据按不同格式（HTML、PDF、Markdown）生成，每个格式一个具体建造者。
- **复杂报表**：表头、表体、页脚步骤固定，但样式与内容来源可替换。

## 总结

- **构建过程与表示分离**：导演固定"怎么搭"，建造者决定"搭成什么样"。
- **步骤复用**：一套工序产出多种成品，新增一种成品只需新增一个具体建造者。
- **封装变化**：客户端不知道每一步的细节，只看到最终产品。
- **注意**：产品本身可以很简单（如 `Hamburger` 只是数据容器），复杂的是构建流程。
