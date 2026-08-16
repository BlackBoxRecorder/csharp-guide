---
title: 迭代器模式
description: C# 迭代器模式（Iterator）详解：顺序访问集合元素而不暴露内部表示，结合菜单遍历示例讲解。
---

# 迭代器模式（Iterator）

迭代器模式提供一种**顺序访问集合元素**的方式，同时不暴露集合的内部表示（数组、链表、哈希表……）。就像餐厅服务员面对两份结构不同的菜单——早餐菜单是 `ArrayList`，晚餐菜单是固定数组——但遍历方式完全一致：`foreach`。

## 解决什么问题

不同集合的内部结构不同（数组、列表、字典），遍历方式也各不相同。如果客户端直接操作集合内部：

1. **遍历逻辑与集合结构耦合**：换一种集合结构，所有遍历代码都要重写。
2. **暴露内部细节**：客户端直接操作底层数组，破坏封装。
3. **代码重复**：每处理一种集合就要写一套遍历。

迭代器把"如何遍历"封装成独立对象，客户端只依赖统一的 `IEnumerable` / `IEnumerator` 接口，集合内部结构对客户端透明。

## 核心结构

| 角色 | 说明 | 示例代码 |
| :--- | :--- | :--- |
| 迭代器接口 | 声明遍历方法 | `IEnumerator`（`MoveNext`、`Current`、`Reset`） |
| 具体迭代器 | 实现针对特定集合的遍历 | `BreakfastMenuEnum`、`DinnerMenuEnum` |
| 可迭代接口 | 返回迭代器的工厂方法 | `IEnumerable`（`GetEnumerator`） |
| 具体集合 | 内部持有数据，对外只暴露迭代器 | `BreakfastMenu`、`DinnerMenu` |

## 代码示例

仓库示例 `src/DesignPatterns/IteratorPattern` 以两种菜单为背景。**集合一**——早餐菜单用 `ArrayList` 存储，通过属性对外暴露迭代器：

```csharp
public class BreakfastMenu
{
    private ArrayList _items;

    public IEnumerable Items
    {
        get { return new BreakfastMenuIterator(_items); }
    }

    public BreakfastMenu()
    {
        _items = new ArrayList();
        AddItem("Waffle", "Blueberry Sauce topped breakfast Waffles", 125, false);
        AddItem("Sandwich", "Veggie Sandwich with tomato and cucumber", 75, true);
    }

    private void AddItem(string name, string description, int price, bool veg)
    {
        _items.Add(new Menu(name, description, price, veg));
    }
}
```

**集合二**——晚餐菜单用固定大小数组存储，结构完全不同：

```csharp
public class DinnerMenu
{
    private const int Max = 1;
    private int _count;
    private Menu[] _items;

    public IEnumerable Items
    {
        get { return new DinnerMenuIterator(_items); }
    }

    public DinnerMenu()
    {
        _items = new Menu[Max];
        AddItems("Hamburger", "Hamburger with cheese and onions", 160, false);
    }

    private void AddItems(string name, string description, int price, bool veg)
    {
        var item = new Menu(name, description, price, veg);
        if (_count <= Max)
        {
            _items[_count] = item;
            _count++;
        }
        else throw new IndexOutOfRangeException();
    }
}
```

**迭代器**——封装遍历逻辑。`MoveNext` 推进游标并判断是否越界，`Current` 返回当前元素：

```csharp
public class BreakfastMenuEnum : IEnumerator
{
    private readonly ArrayList _items;
    private int _position = -1;

    public BreakfastMenuEnum(ArrayList items) => _items = items;

    public bool MoveNext()
    {
        _position++;
        return _position < _items.Count;
    }

    public void Reset() => _position = -1;

    object IEnumerator.Current => Current;

    public Menu Current
    {
        get
        {
            try { return (Menu)_items[_position]; }
            catch (IndexOutOfRangeException) { throw new InvalidOperationException(); }
        }
    }
}
```

`DinnerMenuEnum` 结构相同，只是底层换成数组。**可迭代包装**让 `foreach` 语法可用：

```csharp
public class BreakfastMenuIterator : IEnumerable
{
    private readonly ArrayList _items;

    public BreakfastMenuIterator(ArrayList items) => _items = items;

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IEnumerator GetEnumerator() => new BreakfastMenuEnum(_items);
}
```

**客户端**——面对两种结构完全不同的菜单，用同一套 `foreach` 遍历：

```csharp
public class Client
{
    private readonly IEnumerable _breakfast;
    private readonly IEnumerable _dinner;

    public Client(BreakfastMenu breakfast, DinnerMenu dinner)
    {
        _breakfast = breakfast.Items;
        _dinner = dinner.Items;
    }

    public void PrintMenu()
    {
        PrintMenu(_breakfast);
        PrintMenu(_dinner);
    }

    private void PrintMenu(IEnumerable iter)
    {
        foreach (var item in iter)   // 统一遍历，无视内部结构
        {
            var menu = (Menu)item;
            Console.WriteLine($"{menu.Name}  Rs. {menu.Price} " +
                              $"{(menu.Vegetarian ? "*" : "x")} \n {menu.Description}");
        }
    }
}
```

演示入口（完整代码见 `Program.cs`）：

```csharp
var breakfast = new BreakfastMenu();
var dinner = new DinnerMenu();
var waiter = new Client(breakfast, dinner);
waiter.PrintMenu();
```

运行结果（节选）：

```text
Waffle  Rs. 125 x
 Blueberry Sauce topped breakfast Waffles
Sandwich  Rs. 75 *
 Veggie Sandwich with tomato and cucumber
Hamburger  Rs. 160 x
 Hamburger with cheese and onions
```

## 迭代器与 yield

C# 提供了 `yield return` 语法糖，让迭代器实现变得极其简洁——编译器自动生成状态机。上面手写的 `MoveNext` / `Current` 可以简化为：

```csharp
public IEnumerable Items
{
    get
    {
        foreach (var item in _items)
        {
            yield return item;
        }
    }
}
```

理解手写迭代器的内部机制（游标、`MoveNext`、`Current`），有助于理解 `foreach` 与 LINQ 延迟执行的工作原理。

## 典型应用场景

- **集合框架**：`List<T>`、`Dictionary<TKey,TValue>` 都通过 `IEnumerable<T>` 暴露遍历能力。
- **LINQ 延迟执行**：`Select`、`Where` 返回的是惰性迭代器，遍历时才真正计算。
- **自定义集合**：业务对象封装内部存储，对外只提供 `IEnumerable`。
- **统一异构数据源**：多个数据源结构不同，但都实现 `IEnumerable`，消费方统一遍历。

## 总结

- **遍历与结构解耦**：集合内部怎么存，客户端完全无感知。
- **统一接口**：`IEnumerable` / `IEnumerator` 是 .NET 遍历的通用契约。
- **封装性**：集合内部表示被迭代器隔离，可自由更换存储结构。
- **注意**：遍历过程中修改集合会引发 `InvalidOperationException`（版本检查机制），需谨慎处理。
