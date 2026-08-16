---
title: 访问者模式
description: C# 访问者模式（Visitor）详解：在不修改类的前提下为类层次增加新操作，结合房屋单元示例讲解。
---

# 访问者模式（Visitor）

访问者模式在**不修改现有类**的前提下，为类层次结构**增加新的操作**。它把"对元素的处理逻辑"从元素类中抽离出来，放进独立的访问者对象中。就像房产中介带客户看房：房子（公寓、单间、卧室）不需要为每个客户改变自己，中介带着不同的"看房策略"（装修评估、租金评估）上门即可。

## 解决什么问题

对象结构中的元素类往往需要**追加各种操作**（统计、导出、渲染……）。直接往元素类里加方法会：

1. **修改爆炸**：每个新操作都要给每个元素类加方法，操作越多类越臃肿。
2. **违背开闭原则**：新增操作 = 修改所有元素类。
3. **操作逻辑散落**：同一操作的不同元素实现分散各处，难以统一管理。

访问者把"操作"本身建模为对象：元素只提供一个 `Accept(visitor)` 入口，具体做什么由访问者决定——**元素结构稳定、操作频繁变化**的场景最合适。

## 核心结构

| 角色 | 说明 | 示例代码 |
| :--- | :--- | :--- |
| 访问者接口 | 为每种元素声明一个访问方法 | `IUnitVisitor` |
| 具体访问者 | 实现某种操作在每种元素上的行为 | `ApartmentVisitor`、`LivingRoomVisitor` 等 |
| 元素接口 | 声明 `Accept` 方法接收访问者 | `Unit`（抽象类） |
| 具体元素 | 实现 `Accept`，回调访问者的对应方法 | `Apartment`、`Studio`、`Bedroom`、`LivingRoom` |

## 代码示例

仓库示例 `src/DesignPatterns/VisitorPattern` 以房屋结构为背景。**元素基类**——`Accept` 先调用访问者中对应自己的方法，再递归让子元素接受访问：

```csharp
public abstract class Unit
{
    private readonly Unit[] _units;

    public Unit(params Unit[] units) => _units = units;

    public virtual void Accept(IUnitVisitor visitor)
    {
        foreach (var unit in _units)
        {
            unit.Accept(visitor);   // 递归遍历子元素
        }
    }

    public abstract string ToString();
}
```

**具体元素**——每种房间在 `Accept` 中回调访问者的对应方法（双重分派的关键）：

```csharp
public class Apartment : Unit
{
    public Apartment(params Unit[] units) : base(units) { }

    public override void Accept(IUnitVisitor visitor)
    {
        visitor.VisitApartment(this);   // 调用"属于我"的访问方法
        base.Accept(visitor);           // 再访问子元素
    }

    public override string ToString() => "Apartment";
}

public class LivingRoom : Unit
{
    public LivingRoom(params Unit[] units) : base(units) { }

    public override void Accept(IUnitVisitor visitor)
    {
        visitor.VisitLivingRoom(this);
        base.Accept(visitor);
    }

    public override string ToString() => "Living Room";
}

public class Bedroom : Unit
{
    public Bedroom(params Unit[] units) : base(units) { }

    public override void Accept(IUnitVisitor visitor)
    {
        visitor.VisitBedroom(this);
        base.Accept(visitor);
    }

    public override string ToString() => "Bedroom";
}

public class Studio : Unit
{
    public Studio(params Unit[] units) : base(units) { }

    public override void Accept(IUnitVisitor visitor)
    {
        visitor.VisitStudio(this);
        base.Accept(visitor);
    }

    public override string ToString() => "Studio";
}
```

**访问者接口**——为每种元素声明一个访问方法：

```csharp
public interface IUnitVisitor
{
    void VisitApartment(Apartment apartment);
    void VisitStudio(Studio studio);
    void VisitBedroom(Bedroom bedroom);
    void VisitLivingRoom(LivingRoom livingRoom);
}
```

**具体访问者**——只实现自己关心的元素，其余留空。比如"客厅参观者"只对客厅感兴趣：

```csharp
public class LivingRoomVisitor : IUnitVisitor
{
    public void VisitApartment(Apartment apartment) { }
    public void VisitStudio(Studio studio) { }
    public void VisitBedroom(Bedroom bedroom) { }

    public void VisitLivingRoom(LivingRoom livingRoom)
        => Console.WriteLine("This is the living room");
}
```

**客户端**——同一栋房子，换不同的访问者就得到不同的输出（完整代码见 `Program.cs`）：

```csharp
var apartment = new Apartment(new LivingRoom(), new Bedroom(), new Bedroom());
var studio = new Studio(new LivingRoom(), new Bedroom());

Console.WriteLine("Visiting an Apartment");
apartment.Accept(new ApartmentVisitor());
apartment.Accept(new LivingRoomVisitor());
apartment.Accept(new BedroomVisitor());

Console.WriteLine("Visiting a Studio");
studio.Accept(new StudioVisitor());
studio.Accept(new LivingRoomVisitor());
studio.Accept(new BedroomVisitor());
```

运行结果（节选）：

```text
Visiting an Apartment
This is an apartment
This is the living room
Here is a bedroom
Here is a bedroom
Visiting a Studio
This is a studio
This is the living room
Here is a bedroom
```

注意 `ApartmentVisitor` 只打印了公寓本身，`LivingRoomVisitor` 只打印客厅——元素结构（嵌套关系）始终不变，操作（访问者）随意叠加。

## 双重分派

访问者模式的核心是**双重分派（Double Dispatch）**：

1. **第一次分派**：客户端调用 `apartment.Accept(visitor)`，由元素类型决定进入哪个 `Accept`。
2. **第二次分派**：`Accept` 内部调用 `visitor.VisitApartment(this)`，由访问者类型决定执行哪个操作。

两次分派让"元素类型 × 访问者类型"的组合在运行时精确命中，这正是把操作外置的技术基础。

## 典型应用场景

- **编译器**：AST（语法树）节点结构稳定，类型检查、代码生成、格式化等操作以访问者形式追加。
- **报表生成**：对同一组业务对象输出 HTML、PDF、Excel 等不同格式。
- **对象图分析**：序列化、依赖分析、内存快照等工具遍历对象图执行不同操作。
- **UI 元素处理**：对控件树执行布局、渲染、焦点管理等操作。

## 总结

- **操作与结构分离**：元素结构稳定、操作频繁变化时，新增操作只需新增访问者。
- **开闭原则**：不修改元素类即可扩展操作；元素类新增时需同步扩展所有访问者（可提供默认实现缓解）。
- **职责内聚**：同一种操作的所有元素实现集中在一个访问者中。
- **注意**：元素类型经常变化时不适合访问者模式——每新增一个元素类，所有访问者接口都要跟着改，这正是它的代价。
