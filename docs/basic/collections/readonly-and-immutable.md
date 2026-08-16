---
title: 只读与不可变集合
description: C# 只读与不可变集合详解，ReadOnlyCollection 包装器、Frozen 系列与 System.Collections.Immutable 不可变集合的用法和选型。
---

普通集合（如 `List<T>`、`Dictionary<TKey, TValue>`，见 [常用数据结构](./basic)）在传递给别人后可以被任意修改，这在共享数据时容易引入难以排查的 bug。C# 提供了三类"防修改"的集合，它们的不可变程度和性能特征各不相同：

| 类型 | 可修改？ | 修改方式 | 典型用途 |
| --- | --- | --- | --- |
| `ReadOnlyCollection<T>` | 只能通过原集合修改 | 修改原集合会反映到只读视图 | 暴露内部列表的只读外壳 |
| `FrozenDictionary` / `FrozenSet` | 构建后完全不可修改 | 无 | 启动时构建、运行期只读的静态数据 |
| Immutable 系列 | 不可修改 | 返回**新实例**（结构共享） | 函数式风格、并发共享 |

### 1. `ReadOnlyCollection<T>`

- **描述**：一个**只读包装器**，包住一个已有的 `IList<T>`，外部只能读不能改。注意：它本身没有做拷贝，修改底层列表会反映到包装器上。
- **常用成员**：
  - `Count`：元素数量。
  - `this[int index]`：按索引读取元素。
  - `Contains(T item)` / `IndexOf(T item)`：查询。
- **适用场景**：把类的内部列表以只读形式暴露给调用方，防止外部直接增删元素。

```csharp
var inner = new List<int> { 1, 2, 3 };
var readOnly = new ReadOnlyCollection<int>(inner);

Console.WriteLine(readOnly.Count); // 输出：3

// 底层列表被修改后，只读视图同步变化
inner.Add(4);
Console.WriteLine(readOnly.Count); // 输出：4
```

> 提示：`List<T>` 等类型本身也有 `AsReadOnly()` 方法，效果相同。另外，只读**视图**与只读**接口**（如 `IReadOnlyList<T>`）并不是一回事——接口约束的是"通过该引用不能修改"，但底层对象可能仍然可变。

### 2. `FrozenDictionary<TKey, TValue>` 与 `FrozenSet<T>`

- **描述**：.NET 8 引入，位于 `System.Collections.Frozen` 命名空间。调用 `ToFrozenDictionary()` / `ToFrozenSet()` **一次性构建**后便不可修改，内部针对数据特征优化了布局，查找速度非常快。
- **常用成员**：与 `Dictionary` / `HashSet` 相同的查询成员（`TryGetValue`、`Contains`、`Count` 等），但没有任何修改方法。
- **适用场景**：应用启动时构建一次、运行期只读的配置、映射表；数据量大且查找频繁时收益明显。

```csharp
using System.Collections.Frozen;

var mapping = new Dictionary<string, int>
{
    ["one"] = 1,
    ["two"] = 2,
    ["three"] = 3,
}.ToFrozenDictionary();

Console.WriteLine(mapping["two"]); // 输出：2

// mapping["four"] = 4; // 编译错误：FrozenDictionary 没有修改方法
```

### 3. Immutable 系列（`System.Collections.Immutable`）

- **描述**：位于 `System.Collections.Immutable` 命名空间（NuGet 包 `System.Collections.Immutable`）。**任何"修改"操作都返回一个新实例**，原实例保持不变；新老实例通过**结构共享**复用未变化的部分，因此多次修改的开销可控。常见类型：
  - `ImmutableArray<T>` / `ImmutableList<T>` / `ImmutableDictionary<TKey, TValue>`
  - `ImmutableHashSet<T>` / `ImmutableSortedSet<T>` / `ImmutableSortedDictionary<TKey, TValue>`
  - `ImmutableQueue<T>` / `ImmutableStack<T>`
- **常用成员**：`Add`、`Remove`、`SetItem`、`RemoveRange` 等（都返回新实例），以及 `ToImmutableList()` 等转换方法。
- **适用场景**：需要**快照语义**的数据（如版本化状态）、多线程共享的只读数据、函数式风格的链式操作。

```csharp
using System.Collections.Immutable;

ImmutableList<int> list = ImmutableList<int>.Empty;
list = list.Add(1);
list = list.Add(2);

// 原实例不受影响
Console.WriteLine(list.Count);          // 输出：2
Console.WriteLine(list[0]);             // 输出：1

// 从可变集合快速转换
var immutable = new List<int> { 5, 6 }.ToImmutableList();
Console.WriteLine(immutable.Count);     // 输出：2
```

### 4. 如何选择

1. **只是想挡住外部修改**：用 `ReadOnlyCollection<T>`（成本最低，但底层仍可变）。
2. **数据固定不变且追求查询性能**：用 Frozen 系列（.NET 8+）。
3. **需要真正的快照/版本语义，或跨线程共享**：用 Immutable 系列。
4. **并发环境下的共享只读数据**：Immutable 系列天然线程安全；如果数据构建后完全不变，Frozen 系列查找更快。

> 注意：只读和不可变解决的是"数据被意外修改"的问题，与 [并发集合](./Channel) 系列解决"多线程竞争"的角度不同，两者可以组合使用。
