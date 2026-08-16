---
title: 责任链模式
description: C# 责任链模式（Chain of Responsibility）详解：将请求沿处理链传递直到有对象处理它，结合运算处理器示例讲解。
---

# 责任链模式（Chain of Responsibility）

责任链模式把多个处理器串成**一条链**，请求从链头进入，沿链逐个传递，直到某个处理器接手。就像客服系统：一线客服解决不了转给组长，组长解决不了转给经理——每个人都只关心"我能不能处理"，处理不了就把请求向后传。

## 解决什么问题

当**请求的接收方不确定**、需要按优先级或类型分派时，直接用 `if/else` 判断会：

1. **硬编码分派逻辑**：`if (action == "add") ... else if (action == "minus") ...`，新增处理类型就要改调用方。
2. **处理逻辑臃肿**：所有处理规则挤在一个类里，违背单一职责。
3. **扩展困难**：调整处理顺序、增删处理器都要改动核心代码。

责任链把每个处理器做成独立类，由调用方在运行时自由组装链的顺序与长度。**请求方不知道也不关心谁最终处理**。

## 核心结构

| 角色 | 说明 | 示例代码 |
| :--- | :--- | :--- |
| 处理器接口 | 声明处理请求与设置后继的方法 | `IHandler` |
| 抽象处理器 | 持有后继引用，转发未处理的请求 | `BaseHandler` |
| 具体处理器 | 判断能否处理，不能则传给后继 | `AdditionHandler`、`SubtractionHandler`、`MultiplicationHandler` |

## 代码示例

仓库示例 `src/DesignPatterns/ChainOfResponsibilityPattern` 以四则运算为背景。**处理器接口**：

```csharp
public interface IHandler
{
    void AddChain(IHandler handler);   // 设置后继处理器
    double? Handle(double[] values, string action);
}
```

**抽象处理器**——维护后继引用：

```csharp
public abstract class BaseHandler : IHandler
{
    protected IHandler _nextInLine;

    public void AddChain(IHandler handler) => _nextInLine = handler;

    public abstract double? Handle(double[] values, string action);
}
```

**具体处理器**——只能处理自己关心的运算，其余请求交给后继；链尾无人处理时返回 `null`：

```csharp
public class AdditionHandler : BaseHandler
{
    public override double? Handle(double[] values, string action)
    {
        if (action.ToLower() == "add")
        {
            return values.Sum();
        }
        return _nextInLine?.Handle(values, action);   // 不归我管，向后传
    }
}

public class SubtractionHandler : BaseHandler
{
    public override double? Handle(double[] values, string action)
    {
        if (action.ToLower() == "minus")
        {
            return values[0] - values.Skip(1).Sum();
        }
        return _nextInLine?.Handle(values, action);
    }
}

public class MultiplicationHandler : BaseHandler
{
    public override double? Handle(double[] values, string action)
    {
        if (action.ToLower() == "multiply")
        {
            return values.Aggregate(1.0, (acc, v) => acc * v);
        }
        return _nextInLine?.Handle(values, action);
    }
}
```

演示入口（完整代码见 `Program.cs`）——运行时组装链：加 → 减 → 乘。请求从链头 `additionHandler` 进入：

```csharp
var additionHandler = new AdditionHandler();
var subtractionHandler = new SubtractionHandler();
var multiplicationHandler = new MultiplicationHandler();

subtractionHandler.AddChain(multiplicationHandler);
additionHandler.AddChain(subtractionHandler);

double[] numbers = new double[] { 2, 3, 4, 5 };
var additionResult = additionHandler.Handle(numbers, "Add");
var subtractionResult = additionHandler.Handle(numbers, "Minus");
var multResult = additionHandler.Handle(numbers, "Multiply");
var divisionResult = additionHandler.Handle(numbers, "divide");   // 链上无人处理除法！

Console.WriteLine($"Addition = {additionResult}");
Console.WriteLine($"Subtraction = {subtractionResult}");
Console.WriteLine($"Multiplication = {multResult}");
Console.WriteLine($"Division = {divisionResult}");
```

运行结果：

```text
Addition = 14
Subtraction = -10
Multiplication = 120
Division =
```

`Division` 为 `null`（打印为空）——链上没有任何处理器接收除法请求，请求在链尾静默结束，这正是责任链"无人处理也不报错"的行为特征。

## 变体：责任链的结束方式

| 结束方式 | 行为 | 适用场景 |
| :--- | :--- | :--- |
| 静默结束（示例） | 链尾无人处理，返回默认值 | 可选处理流程 |
| 强制兜底 | 链尾挂一个兜底处理器 | 必须有人处理的请求 |
| 中断处理 | 处理器处理完即停止 | 只允许一个处理器接手 |

## 典型应用场景

- **ASP.NET Core 中间件管道**：请求依次穿过认证、日志、路由等中间件，每个中间件决定继续还是短路。
- **审批流**：请假单按"组长 → 经理 → 总监"依次审批，层级可配置。
- **异常处理链**：`Exception` 按类型在多个处理策略间传递。
- **日志过滤器**：`ILogger` 的过滤器链按级别/来源决定是否记录。

## 总结

- **请求与处理解耦**：发送方不知道也不关心谁来处理。
- **链可动态组装**：处理器顺序、长度在运行时自由配置。
- **单一职责**：每个处理器只做一类判断，符合开闭原则。
- **注意**：链过长会影响性能，且排错时需要沿链追踪；请求可能"无人处理"而静默丢失，需根据业务决定是否兜底。
