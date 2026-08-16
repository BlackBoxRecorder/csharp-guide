---
title: 抽象工厂模式
description: C# 抽象工厂模式（Abstract Factory）详解：创建一组相关或依赖的对象家族，无需指定具体类，结合披萨配料工厂示例讲解。
---

# 抽象工厂模式（Abstract Factory）

抽象工厂模式提供了一种**创建对象家族**的接口，客户端只需要与抽象工厂打交道，就能得到一整套风格统一、相互匹配的产品，而无需关心它们的具体类型。

## 解决什么问题

当系统需要创建**一组相互关联的对象**，且这些对象存在多个"产品系列"时，如果直接在客户端用 `new` 创建，会产生两个问题：

1. **耦合具体类**：客户端一旦写出 `new ThinCrust()`，就与具体类型绑死，更换产品系列需要改动客户端代码。
2. **破坏一致性**：不同系列的产品不能混搭（比如"纽约风格的厚面团 + 芝加哥风格的深盘"，在现实中就是失败的配方）。

抽象工厂把"产品家族"的创建逻辑收敛到独立的工厂类中，客户端只依赖抽象工厂接口，系列之间的搭配由工厂内部保证。

## 核心结构

抽象工厂模式包含四个角色：

| 角色 | 说明 | 示例代码 |
| :--- | :--- | :--- |
| 抽象工厂 | 声明创建一族产品的接口 | `IIngredientsFactory` |
| 具体工厂 | 实现某个产品系列的创建逻辑 | `NyIngredientsFactory`、`ChicagoIngredientsFactory` |
| 抽象产品 | 一族产品的公共接口 | `IDough`、`ISauce`、`ICheese` 等 |
| 具体产品 | 某个系列下的实际产品 | `ThinCrust`、`DeepDish`、`Mozarella` 等 |

## 代码示例

仓库示例 `src/DesignPatterns/FactoryPattern/Abstract Factory` 以披萨配料为背景：纽约店与芝加哥店各有自己的面团、酱料、芝士等配料，但必须配套使用。

**抽象工厂**声明创建一族配料的方法：

```csharp
public interface IIngredientsFactory
{
    IDough CreateDough();
    IEnumerable<IVeggies> CreateVeggies();
    ISauce CreateSauce();
    ICheese CreateCheese();
    IClam CreateClam();
}
```

**具体工厂**为每个系列提供配套实现。纽约工厂生产薄脆面团、樱桃番茄酱和马苏里拉芝士：

```csharp
public class NyIngredientsFactory : IIngredientsFactory
{
    public IDough CreateDough() => new ThinCrust();
    public ISauce CreateSauce() => new CherryTomato();
    public ICheese CreateCheese() => new Mozarella();
    public IClam CreateClam() => new FrozenClam();
    public IEnumerable<IVeggies> CreateVeggies()
        => new IVeggies[] { new Onion(), new Pepper(), new Olive() };
}
```

芝加哥工厂则生产深盘面团、李子番茄酱和帕玛森芝士，两套配方互不干扰：

```csharp
public class ChicagoIngredientsFactory : IIngredientsFactory
{
    public IDough CreateDough() => new DeepDish();
    public ISauce CreateSauce() => new PlumTomato();
    public ICheese CreateCheese() => new Parmesan();
    public IClam CreateClam() => new FreshClam();
    public IEnumerable<IVeggies> CreateVeggies()
        => new IVeggies[] { new Onion(), new Cucumber(), new Pepper() };
}
```

披萨类只依赖抽象工厂，制作流程完全不知道配料的具体类型：

```csharp
public class CheesePizza : Pizza
{
    private readonly IIngredientsFactory _ingredients;

    public CheesePizza(IIngredientsFactory ingredients)
    {
        _ingredients = ingredients;
    }

    internal override void Prepare()
    {
        Console.WriteLine($"Preparing {Name} Using");
        Console.WriteLine($"Dough: {_ingredients.CreateDough().Name}, " +
                          $"Cheese: {_ingredients.CreateCheese().Name}, " +
                          $"Sauce: {_ingredients.CreateSauce().Name}");
    }
}
```

演示入口（完整代码见 `Program.cs`）：

```csharp
Console.WriteLine("Yankees fan orders:");
var yankees = new NyPizzaFactory();
yankees.Order(PizzaType.Cheese);

Console.WriteLine();
Console.WriteLine("Cubs fan orders:");
var cubs = new ChicagoPizzaFactory();
cubs.Order(PizzaType.Clam);
```

运行结果示例：

```text
Yankees fan orders:
Preparing NY Style Cheese Using
Dough: Thin Crust, Cheese: Mozarella, Sauce: Cherry Tomato
Baking at 135 degree Celsius for 20 minutes
Cutting into diagonal pieces
Putting pizza in blue coloured box

Cubs fan orders:
Preparing Chicago Clam Using
Dough: Deep Dish, Clam: Fresh Clam, Sauce: Plum Tomato, Cheese: Parmesan
Baking at 135 degree Celsius for 20 minutes
Cutting into diagonal pieces
Putting pizza in red coloured box
```

## 与工厂方法的关系

抽象工厂常与工厂方法搭配使用：抽象工厂负责创建"配料家族"，工厂方法负责创建"披萨产品"。本仓库的 `FactoryPattern` 工程正是两者的组合示例——披萨工厂（工厂方法）内部持有一个配料工厂（抽象工厂）来组装出完整的披萨。两篇文档可对照阅读。

## 典型应用场景

- **跨平台 UI 组件库**：`WindowsButton` / `MacButton` / `LinuxButton` 属于同一家族，由对应平台的主题工厂统一创建，保证风格一致。
- **数据库访问层**：`SqlConnection` / `OracleConnection` / `MySqlConnection` 各成系列，用 `DbProviderFactory` 这类抽象工厂按配置切换。
- **配置化系统**：同一套业务逻辑需要支持"测试环境配置"与"生产环境配置"两套对象族。

## 总结

- **面向接口创建一族产品**：客户端只依赖 `IIngredientsFactory`，不接触任何具体产品类。
- **保证系列内配套一致**：产品搭配规则收敛在具体工厂内部，杜绝跨系列混搭。
- **开闭原则**：新增一个产品系列（如"加州风味"）只需新增一个具体工厂，无需修改现有代码。
- **代价**：产品族一旦扩展（新增配料类型），所有具体工厂都要同步修改。
