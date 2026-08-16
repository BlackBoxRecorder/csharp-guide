---
title: 策略模式
description: C# 策略模式（Strategy）详解：定义一系列算法并封装，使它们可以互相替换，结合鸭子行为示例讲解。
---

# 策略模式（Strategy）

策略模式定义**一族算法**，把每个算法封装成独立类，并让它们**可以互相替换**，算法变化不影响使用算法的客户端。就像鸭子：会不会飞、怎么叫，不是鸭子这个类写死的，而是运行时可以替换的"行为策略"。

## 解决什么问题

当对象有多种行为变体（飞行方式、叫声、排序算法）时，用继承表达会出现：

1. **子类爆炸**：`FlyWingsDuck`、`FlyNopeDuck`、`QuackNormalDuck`……飞行与叫声两两组合，类数量翻倍。
2. **行为复用困难**：飞行逻辑写在子类里，无法在无关的鸭子之间共享。
3. **运行时不灵活**：继承关系编译期固定，无法在运行时更换行为。

策略模式把**易变的行为**抽成接口 + 一组实现类，对象通过组合持有行为，运行时任意替换。

## 核心结构

| 角色 | 说明 | 示例代码 |
| :--- | :--- | :--- |
| 策略接口 | 定义算法族的公共接口 | `IFlyBehaviour`、`IQuackBehaviour` |
| 具体策略 | 算法的不同实现 | `FlyWings`、`FlyNope`、`QuackNormal` 等 |
| 上下文 | 持有策略引用，将操作委托给策略 | `Duck` |

## 代码示例

仓库示例 `src/DesignPatterns/StrategyPattern` 以鸭子为背景。**策略接口**——把"飞"与"叫"定义成独立的行为族：

```csharp
public interface IFlyBehaviour
{
    void Fly();
}

public interface IQuackBehaviour
{
    void Quack();
}
```

**具体策略**——每种行为一个实现：

```csharp
public class FlyWings : IFlyBehaviour
{
    public void Fly() => Console.WriteLine("Flap Flap");
}

public class FlyNope : IFlyBehaviour
{
    public void Fly() => Console.WriteLine("I can't fly");
}

public class QuackNormal : IQuackBehaviour
{
    public void Quack() => Console.WriteLine("Quack Quack");
}

public class QuackNope : IQuackBehaviour
{
    public void Quack() => Console.WriteLine("...");
}

public class QuackSqueak : IQuackBehaviour
{
    public void Quack() => Console.WriteLine("Squeeeak");
}
```

**上下文**——鸭子通过属性注入行为，执行时委托给策略：

```csharp
public class Duck
{
    private IQuackBehaviour _quacker;
    private IFlyBehaviour _flyer;

    public IQuackBehaviour Quacker { set => _quacker = value; }
    public IFlyBehaviour Flyer { set => _flyer = value; }

    protected void PerformQuack() => _quacker.Quack();
    protected void PerformFly() => _flyer.Fly();
}

public class MallardDuck : Duck
{
    public MallardDuck()
    {
        Flyer = new FlyNope();        // 默认：不会飞的野鸭
        Quacker = new QuackNope();
    }

    public void Display()
    {
        PerformFly();
        PerformQuack();
    }
}
```

演示入口（完整代码见 `Program.cs`）——**运行时更换行为**：

```csharp
var mallard = new MallardDuck { Quacker = new QuackNormal() };   // 换叫声策略
mallard.Display();

mallard.Flyer = new FlyWings();   // 换飞行策略：鸭子突然会飞了
mallard.Display();
```

运行结果：

```text
I can't fly
Quack Quack
Flap Flap
Quack Quack
```

鸭子对象本身没变，只是替换了内部的策略对象，行为立即改变。

## 策略模式与委托

C# 中行为策略更轻量的写法是**委托/函数参数**：

```csharp
public class Duck
{
    public Action FlyAction { get; set; }
    public Action QuackAction { get; set; }

    public void Display()
    {
        FlyAction?.Invoke();
        QuackAction?.Invoke();
    }
}

// 使用
var duck = new Duck
{
    FlyAction = () => Console.WriteLine("Flap Flap"),
    QuackAction = () => Console.WriteLine("Quack Quack")
};
```

行为只有一两个方法的场景，委托即可满足；行为复杂（多方法、带状态）时才值得定义完整策略类。

## 典型应用场景

- **排序算法**：`IComparer<T>` 就是策略接口，`List<T>.Sort` 接收不同的比较策略。
- **验证规则**：用户名、密码、邮箱等验证规则各为策略，组合应用到表单。
- **支付方式**：支付宝、微信、银行卡作为支付策略，结算时动态选择。
- **价格计算**：会员价、折扣价、满减价作为策略，订单按条件选用。

## 总结

- **算法族封装**：行为各自独立成类，可单独测试与复用。
- **运行时替换**：通过组合 + 属性注入，行为可以随时更换。
- **开闭原则**：新增一种行为，只需新增策略类，不改上下文。
- **注意**：策略类会随变体增多而膨胀；上下文与策略之间传递参数时，应使用接口定义的数据契约。
