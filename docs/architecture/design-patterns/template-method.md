---
title: 模板方法模式
description: C# 模板方法模式（Template Method）详解：定义算法骨架，将步骤延迟到子类实现，结合饮料冲泡示例讲解。
---

# 模板方法模式（Template Method）

模板方法模式在父类中定义**算法的骨架**，把其中某些步骤的实现**延迟到子类**。骨架固定不变，子类只负责填充可变步骤。就像泡饮料：煮沸水、冲泡、倒杯是固定流程，但"泡什么"（茶叶还是咖啡粉）、"加什么配料"（柠檬还是牛奶）由子类决定。

## 解决什么问题

多个算法流程**结构相同、步骤各异**时，复制粘贴每个流程会造成：

1. **重复代码**：煮沸、倒杯等公共步骤在每个子类里重复出现。
2. **结构漂移**：有人少写一步、有人顺序不对，流程逐渐走样。
3. **公共逻辑难改**：想要统一调整流程（如增加"消毒"步骤），必须逐个修改。

模板方法把公共流程上提到父类，用**钩子（Hook）**控制流程分支，子类只需实现差异步骤。

## 核心结构

| 角色 | 说明 | 示例代码 |
| :--- | :--- | :--- |
| 抽象类 | 定义模板方法（算法骨架）与抽象步骤 | `Beverage`（`Prepare`） |
| 具体子类 | 实现抽象步骤与钩子 | `Tea`、`Coffee` |
| 钩子方法 | 子类可覆写的流程控制点 | `WantsCondiments` |

## 代码示例

仓库示例 `src/DesignPatterns/TemplatePattern` 以泡饮料为背景。**抽象类**定义模板方法 `Prepare`——骨架固定，可变步骤为抽象方法，配料环节由钩子属性控制：

```csharp
public abstract class Beverage
{
    public void Prepare()   // 模板方法：算法骨架
    {
        Boil();             // 固定步骤
        Brew();             // 抽象步骤：子类实现
        Pour();             // 固定步骤
        if (WantsCondiments)    // 钩子：子类决定是否执行
            AddCondiments();    // 抽象步骤
    }

    public bool WantsCondiments { private get; set; }

    protected abstract void Brew();
    protected abstract void AddCondiments();

    private void Boil() => Console.WriteLine("Boling Water");
    private void Pour() => Console.WriteLine("Pouring in Cup");
}
```

**具体子类**——茶与咖啡各自实现冲泡与配料步骤：

```csharp
public class Tea : Beverage
{
    protected override void Brew()
        => Console.WriteLine("Adding tea leaves to water and boil");

    protected override void AddCondiments()
        => Console.WriteLine("Adding Lemon and Sugar");
}

public class Coffee : Beverage
{
    protected override void Brew()
        => Console.WriteLine("Add Coffee Grounds to water and boil");

    protected override void AddCondiments()
        => Console.WriteLine("Add Milk and Sugar");
}
```

演示入口（完整代码见 `Program.cs`）：

```csharp
var tea = new Tea { WantsCondiments = true };
tea.Prepare();

Console.WriteLine();
var coffee = new Coffee { WantsCondiments = true };
coffee.Prepare();
```

运行结果：

```text
Boling Water
Adding tea leaves to water and boil
Pouring in Cup
Adding Lemon and Sugar

Boling Water
Add Coffee Grounds to water and boil
Pouring in Cup
Add Milk and Sugar
```

流程骨架完全一致，只有差异步骤不同。如果把 `WantsCondiments` 设为 `false`，配料步骤会被跳过——这就是钩子的作用。

## 模板方法的第二个示例：排序

仓库示例的 `TemplatePattern/Comparable` 展示了模板方法在 .NET 中的经典应用——`Array.Sort` 就是模板方法：排序骨架（比较、交换、分区）由框架实现，`IComparable.CompareTo` 是留给你的可变步骤：

```csharp
public class Person : IComparable
{
    public string Name { get; }
    public int Age { get; }

    public Person(string name, int age)
    {
        Name = name;
        Age = age;
    }

    public int CompareTo(object obj)   // 可变步骤：定义"怎么比"
    {
        var other = (Person)obj;
        var nameCompare = string.Compare(Name, other.Name, StringComparison.Ordinal);
        return nameCompare != 0 ? nameCompare : Age.CompareTo(other.Age);
    }

    public override string ToString() => $"{Name} : {Age} < ";
}
```

```csharp
var people = new List<Person>
{
    new("Ram", 25), new("Abishek", 12), new("Ram", 18)
};
people.Sort();   // 框架执行排序骨架，比较逻辑由 CompareTo 提供
```

运行结果：

```text
排序前：Ram : 25 < Abishek : 12 < Ram : 18 <
排序后：Abishek : 12 < Ram : 18 < Ram : 25 <
```

## 与策略模式的区别

| 对比项 | 模板方法 | 策略模式 |
| :--- | :--- | :--- |
| 变化方式 | 继承：子类覆写父类步骤 | 组合：注入策略对象 |
| 骨架位置 | 父类（模板方法） | 客户端或上下文 |
| 灵活性 | 编译期确定（覆写固定方法） | 运行时任意替换 |
| 适用场景 | 流程固定、步骤可变 | 算法整体可替换 |

## 典型应用场景

- **框架生命周期**：ASP.NET Core 的 `Startup`、`IHostedService` 的 `StartAsync`/`StopAsync` 都是模板方法。
- **测试夹具**：`SetUp` / `Teardown` 由测试框架调度，测试类只实现具体步骤。
- **数据处理管道**：读数据 → 转换 → 校验 → 落库，各步骤由不同子类实现。
- **`IComparable` / `IComparer`**：排序、去重的骨架复用，比较规则自定义。

## 总结

- **骨架复用**：公共流程写一次，子类只填差异步骤。
- **钩子控制流**：用 `virtual` 属性/方法让子类灵活开关流程环节。
- **好莱坞原则**："别打电话给我们，我们会打给你"——父类调度子类，而不是子类驱动父类。
- **注意**：骨架步骤越多，子类负担越重；应保持模板方法精简，避免"重方法"。
