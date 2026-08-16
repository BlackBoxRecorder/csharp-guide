---
title: 组合模式
description: C# 组合模式（Composite）详解：将对象组织成树形结构，使单个对象与组合对象使用一致，结合菜单树示例讲解。
---

# 组合模式（Composite）

组合模式将对象组织成**树形结构**，并让**叶子节点**（单项）与**组合节点**（容器）使用一致的接口。调用方无需区分"这是一道菜还是一个菜单"，对两者执行同样的操作。文件系统的文件夹/文件、公司的部门/员工，都是这种结构。

## 解决什么问题

处理树形结构时，最直接的写法是让调用方判断节点类型：

```csharp
if (node is Menu menu) { /* 递归遍历子节点 */ }
else if (node is MenuItem item) { /* 处理单项 */ }
```

类型判断散落在各处，新增节点类型就要改所有调用方。组合模式让叶子与容器实现**同一个抽象接口**：

1. **统一操作**：调用方只面对 `MenuComponent`，打印、遍历、增删子节点都无需关心具体类型。
2. **递归天然**：容器节点把操作转发给所有子节点，树的遍历由结构自身完成。
3. **开闭原则**：新增一种菜单项，不影响既有调用代码。

## 核心结构

| 角色 | 说明 | 示例代码 |
| :--- | :--- | :--- |
| 组件 | 叶子与容器的公共抽象 | `MenuComponent` |
| 叶子 | 不可再分的基本单元 | `MenuItem` |
| 容器 | 持有子组件集合，转发操作 | `Menu` |

## 代码示例

仓库示例 `src/DesignPatterns/CompositePattern` 以餐厅菜单为背景。**组件抽象**提供默认操作，不支持的操作直接抛异常：

```csharp
public class MenuComponent
{
    public virtual void Add(MenuComponent component)
        => throw new NotImplementedException();

    public virtual void Remove(MenuComponent component)
        => throw new NotImplementedException();

    public virtual MenuComponent GetChild(int i)
        => throw new NotImplementedException();

    public virtual string Name { get; }
    public virtual string Description { get; }
    public virtual bool Vegetarian { get; }
    public virtual double Price { get; }

    public virtual void Print()
        => throw new NotImplementedException();
}
```

**叶子节点**是具体的菜品，只实现数据访问与打印：

```csharp
public class MenuItem : MenuComponent
{
    public MenuItem(string name, string description, double price, bool isveg)
    {
        Name = name;
        Description = description;
        Price = price;
        Vegetarian = isveg;
    }

    public override string Name { get; }
    public override string Description { get; }
    public override double Price { get; }
    public override bool Vegetarian { get; }

    public override void Print()
        => Console.WriteLine($"{Name} : {Price}  {(Vegetarian ? '+' : '*')} \n {Description}");
}
```

**容器节点**维护子组件列表，`Print()` 递归转发给所有子节点——不管子节点是菜品还是子菜单：

```csharp
public class Menu : MenuComponent
{
    private readonly List<MenuComponent> _components = new();

    public Menu(string name, string description)
    {
        Name = name;
        Description = description;
    }

    public override void Add(MenuComponent component) => _components.Add(component);
    public override void Remove(MenuComponent component) => _components.Remove(component);
    public override MenuComponent GetChild(int i) => _components[i];

    public override string Name { get; }
    public override string Description { get; }

    public override void Print()
    {
        Console.WriteLine(Name);
        Console.WriteLine("___________");
        foreach (var menuComponent in _components)
        {
            menuComponent.Print();   // 递归：子节点可能是 Menu，也可能是 MenuItem
        }
        Console.WriteLine();
    }
}
```

**客户端**完全不需要类型判断，对根菜单直接 `Print()`（完整代码见 `Program.cs`）：

```csharp
var breakfast = new Menu("Breakfast", "Pancake House");
var lunch = new Menu("Lunch", "Deli Diner");
var dinner = new Menu("Dinner", "Dinneroni");
var dessert = new Menu("Dessert", "Ice Cream");

breakfast.Add(new MenuItem("Waffles", "Butterscotch waffles", 140, false));
lunch.Add(new MenuItem("Burger", "Cheese and Onion Burger", 250, true));
dinner.Add(new MenuItem("Pizza", "Cheese and Tomato Pizza", 210, true));

dessert.Add(new MenuItem("Ice Cream", "Vanilla and Chocolate", 120, true));
dinner.Add(dessert);   // 子菜单嵌套：菜单套菜单

var menu = new Menu("All", "McDonalds");
menu.Add(breakfast);
menu.Add(lunch);
menu.Add(dinner);

menu.Print();
```

运行结果（节选）：

```text
All
___________
Breakfast
___________
Waffles : 140  *
 Butterscotch waffles
...
Dinner
___________
Pizza : 210  +
 Cheese and Tomato Pizza
Dessert
___________
Ice Cream : 120  +
 Vanilla and Chocolate
```

## 安全组合与透明组合

| 类型 | 做法 | 权衡 |
| :--- | :--- | :--- |
| 透明组合（示例采用） | 组件接口包含 Add/Remove，叶子也"可见"这些方法 | 调用方无感知，但叶子调用 Add 会抛异常 |
| 安全组合 | 容器接口才有 Add/Remove，叶子接口更窄 | 类型安全，但调用方需要类型判断 |

## 典型应用场景

- **菜单系统**：菜单嵌套子菜单（本示例）。
- **文件系统**：文件夹包含文件与子文件夹，`DirectoryInfo` / `FileInfo` 都实现文件系统项接口。
- **组织架构**：部门包含员工与子部门，统一统计人数、成本。
- **控件树**：WPF/ASP.NET 控件树，`Panel` 容器与 `Button` 叶子统一参与布局与渲染。

## 总结

- **叶子与容器同构**：调用方用同一接口操作整个树。
- **递归结构**：打印、遍历等操作天然递归，新增层级无需改动客户端。
- **开闭原则**：新增一种叶子或容器类型，既有代码不受影响。
- **注意**：树形结构过深时递归可能栈溢出，可改用显式栈遍历；组合模式与迭代器模式常搭配使用。
