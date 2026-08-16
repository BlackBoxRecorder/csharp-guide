---
title: 工厂方法模式
description: C# 工厂方法模式（Factory Method）详解：将对象创建延迟到子类，由子类决定实例化哪个类，结合披萨工厂示例讲解。
---

# 工厂方法模式（Factory Method）

工厂方法模式在父类中定义一个**创建对象的抽象方法**，把"实例化哪个类"的决策下放到子类。父类的业务流程保持不变，子类通过覆写工厂方法来决定产出的具体产品。披萨店的"下单流程"人人相同，但纽约店和芝加哥店做出的披萨各不相同——这正是工厂方法的场景。

## 解决什么问题

一段业务流程（如"下单 → 准备 → 烘烤 → 切块 → 装盒"）需要作用于不同类型的产品。如果直接在流程中 `new` 具体产品：

1. **业务代码被具体类污染**：`Order()` 里写满 `if/else` 判断该创建哪种披萨，新增一种披萨就要改动业务代码，违反开闭原则。
2. **流程与创建耦合**：一旦创建逻辑变化，核心流程代码也要跟着改，无法复用。

工厂方法把"创建产品"这一步抽成抽象方法，让流程代码只依赖抽象产品，创建决策由子类工厂完成。

## 核心结构

| 角色 | 说明 | 示例代码 |
| :--- | :--- | :--- |
| 抽象工厂 | 定义业务流程，并声明抽象工厂方法 | `PizzaFactory`（`Order` + `Create`） |
| 具体工厂 | 覆写工厂方法，决定创建哪种产品 | `NyPizzaFactory`、`ChicagoPizzaFactory` |
| 抽象产品 | 产品的公共接口或基类 | `Pizza` |
| 具体产品 | 工厂方法实际产出的对象 | `CheesePizza`、`ClamPizza`、`VeggiePizza` |

## 代码示例

仓库示例 `src/DesignPatterns/FactoryPattern/Factory Method` 以披萨店为背景。**抽象工厂**定义完整的下单流程，并把创建披萨的动作留给子类：

```csharp
public abstract class PizzaFactory
{
    public Pizza Order(PizzaType type)
    {
        var pizza = Create(type);   // 工厂方法：延迟到子类实现
        pizza.Prepare();
        pizza.Bake();
        pizza.Cut();
        pizza.Box();
        return pizza;
    }

    protected abstract Pizza Create(PizzaType type);
}
```

**具体工厂**覆写 `Create`，决定披萨的配料来源与命名。纽约店用纽约配料工厂，披萨命名带 "NY Style"：

```csharp
public class NyPizzaFactory : PizzaFactory
{
    protected override Pizza Create(PizzaType type)
    {
        Pizza pizza;
        IIngredientsFactory ingredients = new NyIngredientsFactory();

        if (type == PizzaType.Cheese)
        {
            pizza = new CheesePizza(ingredients) { Name = "NY Style Cheese" };
        }
        else if (type == PizzaType.Clam)
        {
            pizza = new ClamPizza(ingredients) { Name = "NY Style Clam" };
        }
        else
        {
            pizza = new VeggiePizza(ingredients) { Name = "NY Style Veggie" };
        }
        pizza.Color = "blue";
        return pizza;
    }
}
```

芝加哥店则换用芝加哥配料工厂，产出深盘风味的披萨：

```csharp
public class ChicagoPizzaFactory : PizzaFactory
{
    protected override Pizza Create(PizzaType type)
    {
        Pizza pizza;
        IIngredientsFactory ingredients = new ChicagoIngredientsFactory();

        if (type == PizzaType.Cheese)
        {
            pizza = new CheesePizza(ingredients) { Name = "Chicago Cheese" };
        }
        else if (type == PizzaType.Clam)
        {
            pizza = new ClamPizza(ingredients) { Name = "Chicago Clam" };
        }
        else
        {
            pizza = new VeggiePizza(ingredients) { Name = "Chicago Veggie" };
        }
        pizza.Color = "red";
        return pizza;
    }
}
```

**抽象产品** `Pizza` 定义了所有披萨共有的制作步骤，其中 `Prepare` 由具体披萨实现：

```csharp
public abstract class Pizza
{
    public string Color;
    public string Name { protected get; set; }

    internal abstract void Prepare();
    internal void Bake()  => Console.WriteLine("Baking at 135 degree Celsius for 20 minutes");
    internal void Cut()   => Console.WriteLine("Cutting into diagonal pieces");
    internal void Box()   => Console.WriteLine($"Putting pizza in {Color} coloured box");
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

运行结果（节选）：

```text
Yankees fan orders:
Preparing NY Style Cheese Using
Dough: Thin Crust, Cheese: Mozarella, Sauce: Cherry Tomato
Baking at 135 degree Celsius for 20 minutes
Cutting into diagonal pieces
Putting pizza in blue coloured box
```

## 工厂方法与抽象工厂

两者的名字相似，但职责不同：

- **工厂方法**：一个工厂方法创建一个**产品**，用继承（子类覆写）实现变化，属于类级模式。
- **抽象工厂**：一个工厂接口创建**一族产品**，用组合（工厂内部聚合多个创建方法）实现变化，属于对象级模式。

本仓库的 `FactoryPattern` 工程同时演示了两者：`PizzaFactory.Create` 是工厂方法，披萨内部的 `IIngredientsFactory` 是抽象工厂。可对照 [抽象工厂模式](./abstract-factory) 阅读。

## 典型应用场景

- **日志记录器**：`LoggerFactory` 根据配置创建 `FileLogger`、`ConsoleLogger`、`DatabaseLogger`，业务代码只依赖 `ILogger`。
- **连接工厂**：`SqlClientFactory`、`MySqlConnectorFactory` 各自实现同一个工厂基类，`DbProviderFactories` 按配置获取。
- **文档解析器**：按文件扩展名创建不同的 `Parser`（JSON / XML / CSV），解析流程完全复用。

## 总结

- **创建延迟到子类**：父类只声明"要什么"，子类决定"给什么"。
- **业务逻辑复用**：`Order` 流程写一次，所有披萨工厂共享。
- **开闭原则**：新增一种披萨或一家新店，只需新增具体类，不改动流程代码。
- **注意**：如果产品种类较少且稳定，直接使用简单工厂（`switch` 创建）即可，不必引入继承层次。
