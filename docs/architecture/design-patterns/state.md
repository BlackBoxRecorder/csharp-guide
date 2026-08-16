---
title: 状态模式
description: C# 状态模式（State）详解：对象内部状态变化时改变其行为，结合口香糖机示例讲解。
---

# 状态模式（State）

状态模式让对象在**内部状态改变时改变自己的行为**，看起来就像"换了类"。口香糖机是最经典的例子：没有硬币、有硬币、正在出货、售罄——同一台机器，面对"投币"这个动作，不同状态下的反应完全不同。

## 解决什么问题

对象在不同状态下对同一操作有不同行为时，最直接的实现是在每个方法里写 `switch` 判断状态：

1. **条件语句蔓延**：每个方法都要判断状态，`switch` 遍布整个类，方法越长越难维护。
2. **状态迁移混乱**：状态转换逻辑散落在各方法里，新增状态要翻遍所有方法。
3. **违背开闭原则**：新增一种状态，所有方法都要跟着改。

状态模式把**每个状态的行为**封装成独立类，对象持有"当前状态对象"，操作直接委托给它。状态切换就是**更换状态对象**。

## 核心结构

| 角色 | 说明 | 示例代码 |
| :--- | :--- | :--- |
| 状态接口 | 声明所有状态都要实现的操作 | `IState` |
| 具体状态 | 实现某个状态下各操作的行为 | `NoQuarterState`、`HasQuarterState` 等 |
| 上下文 | 持有当前状态对象，将操作委托给它 | `GumballMachine` |

## 代码示例

仓库示例 `src/DesignPatterns/StatePattern` 以口香糖机为背景。**状态接口**定义所有状态共有的四个操作：

```csharp
public interface IState
{
    void InsertQuarter();   // 投币
    void EjectQuarter();    // 退币
    void TurnCrank();       // 转动手柄
    void Dispense();        // 出货
}
```

**上下文**——口香糖机持有全部状态实例与当前状态，所有操作委托给当前状态：

```csharp
public class GumballMachine
{
    public int Count { get; private set; }

    public IState SoldOutState;
    public IState NoQuarterState;
    public IState HasQuarterState;
    public IState SoldState;
    public IState WinnerState;

    public IState State { get; set; }

    public GumballMachine(int count)
    {
        Count = count;
        SoldOutState = new SoldOutState(this);
        NoQuarterState = new NoQuarterState(this);
        HasQuarterState = new HasQuarterState(this);
        SoldState = new SoldState(this);
        WinnerState = new WinnerState(this);

        State = Count > 0 ? NoQuarterState : SoldOutState;
    }

    public void InsertQuarter() => State.InsertQuarter();
    public void EjectQuarter() => State.EjectQuarter();
    public void TurnCrank()
    {
        State.TurnCrank();
        State.Dispense();
    }

    public void ReleaseBall()
    {
        Console.WriteLine("A ball comes rolling down");
        if (Count == 0) return;
        Count--;
    }
}
```

**具体状态一**——没有硬币的状态：投币成功并迁移到"有硬币"，其他操作都是无效提示：

```csharp
public class NoQuarterState : IState
{
    public GumballMachine Machine { get; }

    public NoQuarterState(GumballMachine machine) => Machine = machine;

    public void InsertQuarter()
    {
        Console.WriteLine("Inserted a quarter");
        Machine.State = Machine.HasQuarterState;   // 状态迁移
    }

    public void EjectQuarter() => Console.Write("Can't eject anything");
    public void TurnCrank() => Console.WriteLine("Can't turn crank without a quarter");
    public void Dispense() => Console.WriteLine("Can't dispense");
}
```

**具体状态二**——有硬币的状态：投币被拒，退币恢复"无硬币"，转动手柄有 10% 概率触发"中奖状态"（额外奖励一颗）：

```csharp
public class HasQuarterState : IState
{
    private GumballMachine Machine { get; }
    private readonly Random _random = new Random(DateTime.Now.Millisecond);

    public HasQuarterState(GumballMachine machine) => Machine = machine;

    public void InsertQuarter() => Console.WriteLine("Can't insert more than one");

    public void EjectQuarter()
    {
        Console.WriteLine("Quarter returned");
        Machine.State = Machine.NoQuarterState;
    }

    public void TurnCrank()
    {
        Console.WriteLine("You turned the crank");
        var winner = _random.Next(10);
        Machine.State = (winner == 5 && Machine.Count > 1)
            ? Machine.WinnerState
            : Machine.SoldState;
    }

    public void Dispense() => Console.WriteLine("Can't do that");
}
```

**具体状态三**——出货状态：真正放球，并根据余量决定回到"无硬币"还是进入"售罄"：

```csharp
public class SoldState : IState
{
    private GumballMachine Machine { get; }

    public SoldState(GumballMachine machine) => Machine = machine;

    public void InsertQuarter() => Console.WriteLine("Please wait, already in progress");
    public void EjectQuarter() => Console.WriteLine("Can't eject, already turned the crank");
    public void TurnCrank() => Console.WriteLine("Turning twice achieves nothing");

    public void Dispense()
    {
        Machine.ReleaseBall();
        if (Machine.Count > 0)
        {
            Machine.State = Machine.NoQuarterState;
        }
        else
        {
            Console.WriteLine("Oops! No more gumballs");
            Machine.State = Machine.SoldOutState;
        }
    }
}
```

演示入口（完整代码见 `Program.cs`）：

```csharp
var gumballMachine = new GumballMachine(5);
gumballMachine.InsertQuarter();   // 投币成功，进入 HasQuarterState
gumballMachine.TurnCrank();       // 出货，回到 NoQuarterState
gumballMachine.InsertQuarter();
gumballMachine.TurnCrank();
```

运行结果（节选）：

```text
Inserted a quarter
You turned the crank
A ball comes rolling down
Inserted a quarter
You turned the crank
A ball comes rolling down
```

同一台机器，同一组操作，行为随状态自动变化——`GumballMachine` 中没有任何 `if/switch` 状态判断。

## 与策略模式的区别

| 对比项 | 状态模式 | 策略模式 |
| :--- | :--- | :--- |
| 意图 | 让对象随状态改变行为 | 封装可替换的算法 |
| 状态迁移 | 状态之间**相互转换**（对象持有当前状态） | 策略通常由客户端**一次性指定** |
| 关注点 | 对象内部行为的动态变化 | 算法族的灵活替换 |

两者结构相似，但状态模式强调"迁移"（状态对象会主动切换上下文的状态），策略模式强调"替换"（客户端决定用哪个算法）。

## 典型应用场景

- **订单状态机**：待支付 → 已支付 → 已发货 → 已完成，各状态对"取消""退款"等操作响应不同。
- **网络连接状态**：连接、鉴权、就绪、断开等状态对应不同的数据收发行为。
- **流式处理管道**：解析器的状态（读头、读体、读尾）驱动不同的处理逻辑。
- **UI 交互状态**：编辑/只读/加载中的控件行为各不相同。

## 总结

- **行为随状态改变**：对象把操作委托给当前状态对象，无需条件判断。
- **状态迁移集中**：每个状态自己决定"什么情况下转到哪个状态"，规则内聚。
- **开闭原则**：新增状态只需新增一个状态类，不改既有状态。
- **注意**：状态类数量随状态增多而膨胀；状态类共享上下文实例，需注意多线程下的状态一致性。
