---
title: 中介者模式
description: C# 中介者模式（Mediator）详解：通过中介对象简化对象间的交互，结合项目经理协调示例讲解。
---

# 中介者模式（Mediator）

中介者模式引入一个**中介对象**来协调多个对象之间的交互，让对象们不再互相直接引用，而是统一通过中介者通信。就像项目团队：客户、程序员、测试员不直接对话，所有消息都经过项目经理转发——消息流向由项目经理一人掌控。

## 解决什么问题

对象之间直接交互时，会形成**网状依赖**：

1. **耦合度高**：N 个对象两两交互，最多产生 N×(N-1)/2 条连接，每个对象都要认识其他所有对象。
2. **复用困难**：对象被交互逻辑缠绕，脱离特定协作环境就无法单独使用。
3. **改动牵一发动全身**：一个对象的接口变化，所有关联对象都要跟着改。

中介者把网状交互收敛为**星形结构**：所有对象只认识中介者，交互规则集中在中介者内部。

## 核心结构

| 角色 | 说明 | 示例代码 |
| :--- | :--- | :--- |
| 中介者接口 | 声明对象间的通信方法 | `Mediator` |
| 具体中介者 | 实现消息路由规则 | `ManagerMediator` |
| 同事接口 | 声明对象收发消息的方法 | `Colleague` |
| 具体同事 | 实际参与协作的对象 | `Customer`、`Programmer`、`Tester` |

## 代码示例

仓库示例 `src/DesignPatterns/MediatorPattern` 以项目团队为背景。**中介者接口**：

```csharp
public abstract class Mediator
{
    public abstract void Send(string message, Colleague colleague);
}
```

**具体中介者**——项目经理掌握消息路由规则：客户的消息发给程序员，程序员的发给测试员，测试员的发给客户：

```csharp
public class ManagerMediator : Mediator
{
    public Colleague Customer { get; set; }
    public Colleague Programmer { get; set; }
    public Colleague Tester { get; set; }

    public override void Send(string message, Colleague colleague)
    {
        if (colleague == Customer)
        {
            Programmer.Notify(message);
        }
        else if (colleague == Programmer)
        {
            Tester.Notify(message);
        }
        else
        {
            Customer.Notify(message);
        }
    }
}
```

**同事基类**——每个同事都持有中介者引用，发消息统一走 `mediator.Send`：

```csharp
public abstract class Colleague
{
    protected Mediator mediator;

    public Colleague(Mediator mediator) => this.mediator = mediator;

    public virtual void Send(string message) => mediator.Send(message, this);

    public abstract void Notify(string message);
}
```

**具体同事**——只实现"收到消息怎么展示"，互不认识对方：

```csharp
public class Customer : Colleague
{
    public Customer(Mediator mediator) : base(mediator) { }

    public override void Notify(string message)
        => Console.WriteLine($"Message to customer: {message}");
}

public class Programmer : Colleague
{
    public Programmer(Mediator mediator) : base(mediator) { }

    public override void Notify(string message)
        => Console.WriteLine($"Message to programmer: {message}");
}

public class Tester : Colleague
{
    public Tester(Mediator mediator) : base(mediator) { }

    public override void Notify(string message)
        => Console.WriteLine($"Message to tester: {message}");
}
```

演示入口（完整代码见 `Program.cs`）：

```csharp
var mediator = new ManagerMediator();
var customer = new Customer(mediator);
var programmer = new Programmer(mediator);
var tester = new Tester(mediator);

mediator.Customer = customer;
mediator.Programmer = programmer;
mediator.Tester = tester;

customer.Send("We have an order, need to make a program");
programmer.Send("I have done program, need to test it");
tester.Send("I have done testing, here is ready program for you");
```

运行结果：

```text
Message to programmer: We have an order, need to make a program
Message to tester: I have done program, need to test it
Message to customer: I have done testing, here is ready program for you
```

客户只调用 `Send`，消息经项目经理路由到程序员；任何同事都不直接持有其他同事的引用。

## 直接交互 vs 中介者

| 对比项 | 直接交互 | 中介者 |
| :--- | :--- | :--- |
| 依赖结构 | 网状（N² 条连接） | 星形（N 条连接） |
| 交互规则 | 分散在各对象 | 集中在中介者 |
| 对象复用 | 困难 | 同事彼此解耦，易于复用 |
| 变更影响 | 全局波及 | 只改中介者 |

## 典型应用场景

- **聊天室**：用户不直接点对点通信，消息经服务器（中介者）广播或路由。
- **表单控件联动**：多个控件（下拉框、输入框、表格）之间的联动逻辑集中在一个中介者中，避免控件互相引用。
- **消息队列**：发布者与订阅者通过消息中间件解耦，本质是中介者思想的分布式实现。
- **事件总线**：`EventAggregator`、`MediatR` 等库让组件通过中介者通信。

## 总结

- **星形依赖**：把 N² 的网状交互降为 N 条"对象 → 中介者"连接。
- **规则集中**：消息路由、协作逻辑集中在中介者，便于统一修改。
- **对象解耦**：同事类不知道彼此存在，可独立复用与测试。
- **注意**：中介者容易膨胀为"上帝对象"，交互规则复杂时应拆分职责；同事之间交互极少时不必引入中介者。
