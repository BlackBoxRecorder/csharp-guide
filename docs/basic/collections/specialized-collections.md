---
title: 专用集合
description: C# 专用集合详解，包括 Collection 基类、KeyedCollection、ObservableCollection、Deque、ConcurrentBag、OrderedDictionary 及遗留非泛型集合。
---

除了 [常用数据结构](./basic) 和 [有序集合](./sorted-collections)、[只读与不可变集合](./readonly-and-immutable) 中介绍的通用集合，.NET 还提供了一批面向特定场景的专用集合。它们规模较小、用途明确，能在合适的场景中大幅简化代码。

### 1. `Collection<T>` 与 `KeyedCollection<TKey, TItem>`

- **描述**：
  - `Collection<T>`：一个**可扩展基类**，包住内部的 `IList<T>`，并暴露 `InsertItem`、`RemoveItem`、`SetItem`、`ClearItems` 等虚方法，子类可以覆写它们来实现"增删时做额外处理"。
  - `KeyedCollection<TKey, TItem>`：继承自 `Collection<T>`，**既可按索引访问，也可按键访问**（通过 `this[TKey key]`），本质是字典与列表的结合。
- **常用成员**：
  - `InsertItem(int index, TItem item)` 等虚方法：子类覆写钩子。
  - `this[int index]` / `this[TKey key]`：索引或按键访问。
  - `Add(TItem item)` / `Remove(TItem item)`：增删元素。
- **适用场景**：需要"集合行为 + 自定义逻辑"的领域模型，如日志集合、带 ID 的实体集合。

```csharp
public class LogCollection : Collection<string>
{
    protected override void InsertItem(int index, string item)
    {
        Console.WriteLine($"[日志] 添加: {item}");
        base.InsertItem(index, item);
    }
}

var logs = new LogCollection();
logs.Add("启动");  // 输出：[日志] 添加: 启动
logs.Add("关闭");  // 输出：[日志] 添加: 关闭
Console.WriteLine(logs.Count); // 输出：2
```

### 2. `ObservableCollection<T>`

- **描述**：当元素被添加、移除或重置时，触发 `CollectionChanged` 事件，通知外界数据发生了变化。
- **常用成员**：
  - `CollectionChanged` 事件：数据变更时触发。
  - `Add` / `Remove` / `Move` / `Clear`：与 `List<T>` 类似，但每次变更都会发事件。
- **适用场景**：**WPF / MAUI 数据绑定**——绑定到 `ListBox`、`DataGrid` 等控件后，集合变化会自动刷新界面。

```csharp
using System.Collections.ObjectModel;
using System.Collections.Specialized;

var items = new ObservableCollection<string>();
items.CollectionChanged += (_, e) =>
{
    if (e.Action == NotifyCollectionChangedAction.Add)
        Console.WriteLine($"新增: {e.NewItems?[0]}");
};

items.Add("第一项"); // 输出：新增: 第一项
```

### 3. `Deque<T>`（.NET 9+）

- **描述**：**双端队列**，位于 `System.Collections.Generic` 命名空间。两端都可以 O(1) 时间添加和移除元素，补齐了 `Queue<T>`（只能尾进头出）和 `Stack<T>`（只能顶进顶出）各自只擅长一端的不足。
- **常用方法**：
  - `PushFront(T item)` / `PushBack(T item)`：在头部/尾部添加。
  - `PopFront()` / `PopBack()`：从头部/尾部移除并返回。
  - `PeekFront()` / `PeekBack()`：查看两端元素但不移除。
- **适用场景**：滑动窗口、撤销/重做之外的"双端操作"需求，如任务双端调度。

```csharp
var deque = new Deque<int>();
deque.PushBack(2);        // [2]
deque.PushBack(3);        // [2, 3]
deque.PushFront(1);       // [1, 2, 3]

Console.WriteLine(deque.PeekFront()); // 输出：1
Console.WriteLine(deque.PopBack());   // 输出：3
Console.WriteLine(deque.Count);       // 输出：2
```

### 4. `ConcurrentBag<T>`

- **描述**：线程安全的**无序**集合，位于 `System.Collections.Concurrent` 命名空间。每个线程向自己维护的局部列表中读写元素，跨线程的负载均衡按需进行，适合"每个线程独立添加、最后整体取走"的模式。
- **常用方法**：
  - `Add(T item)`：添加元素。
  - `TryTake(out T item)`：尝试取出一个元素（无特定顺序）。
  - `TryPeek(out T item)`：尝试查看一个元素。
- **适用场景**：多线程产生任务、最后统一消费，且**不关心顺序**的场景；需要严格顺序或 FIFO 时改用 [ConcurrentQueue](./Channel) 一类的队列集合。

```csharp
using System.Collections.Concurrent;

var bag = new ConcurrentBag<int>();
Parallel.For(0, 10, i => bag.Add(i));

var sum = 0;
while (bag.TryTake(out var item))
{
    sum += item;
}
Console.WriteLine(sum); // 输出：45
```

### 5. `OrderedDictionary` 与 `NameValueCollection`

- **描述**：
  - `OrderedDictionary`（`System.Collections.Specialized`）：非泛型键值对集合，**保持元素的插入顺序**，同时支持按键和按索引访问。
  - `NameValueCollection`（`System.Collections.Specialized`）：一个键可以对应**多个值**的字符串集合，常用于解析 HTTP 查询字符串。
- **常用成员**：
  - `OrderedDictionary`：`Add`、`Remove`、`Keys`、`Values`、`this[int index]`。
  - `NameValueCollection`：`Add(string key, string value)`、`Get(string key)`、`GetValues(string key)`。
- **适用场景**：需要"保持插入顺序"的键值对（泛型 `Dictionary` 不保证顺序）以及一键多值映射。

```csharp
using System.Collections.Specialized;

var ordered = new OrderedDictionary
{
    ["first"] = 1,
    ["second"] = 2,
};
Console.WriteLine(ordered[0]); // 输出：1（按插入顺序访问）

var query = new NameValueCollection();
query.Add("tag", "csharp");
query.Add("tag", "dotnet");
Console.WriteLine(query["tag"]); // 输出：csharp,dotnet
```

### 6. 遗留非泛型集合

| 类型 | 说明 | 现代替代 |
| --- | --- | --- |
| `ArrayList` | 非泛型动态数组，元素是 `object` | `List<T>` |
| `Hashtable` | 非泛型键值对，键是 `object` | `Dictionary<TKey, TValue>` |
| `ListDictionary` | 少量元素时性能好的键值对 | `Dictionary<TKey, TValue>` |
| `HybridDictionary` | 小数据量用列表、大数据量自动切换为哈希 | `Dictionary<TKey, TValue>` |
| `StringDictionary` | 键和值都是字符串的非泛型字典 | `Dictionary<string, string>` |

这些类型来自 .NET Framework 1.x 时代，**新代码一律不要使用**：它们以 `object` 存储元素，既无类型安全，又会产生装箱开销。仅在维护遗留代码时才会遇到。

### 7. 选型建议

- 需要"集合 + 自定义逻辑"：`Collection<T>`。
- 需要按键和索引双重访问：`KeyedCollection<TKey, TItem>`。
- WPF/MAUI 界面绑定：`ObservableCollection<T>`。
- 双端增删：`Deque<T>`（.NET 9+）。
- 多线程无序收集：`ConcurrentBag<T>`。
- 保持插入顺序的键值对：`OrderedDictionary`。
- 需要解析一键多值（如查询参数）：`NameValueCollection`。
- 其他情况优先选择 [常用数据结构](./basic) 中的泛型集合。
