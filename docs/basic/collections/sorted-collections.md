---
title: 有序集合：SortedList 与 SortedDictionary
description: C# 有序集合详解，SortedList 与 SortedDictionary 的实现原理、性能对比和适用场景，以及与 SortedSet 的关系。
---

在 [常用数据结构](./basic) 中介绍的 `SortedSet<T>` 只能维护**值**的有序集合，而当需要按照键自动排序的键值对集合时，就要用到 `SortedList<TKey, TValue>` 和 `SortedDictionary<TKey, TValue>`。它们都位于 `System.Collections.Generic` 命名空间，会按键自动排序，但内部实现和性能特征截然不同。

### 1. `SortedList<TKey, TValue>`

- **描述**：按**键排序**的键值对集合，内部使用**两个连续数组**（一个存键、一个存值）配合**二分查找**实现。查找效率高，内存占用紧凑，但插入和删除需要移动元素。
- **常用方法**：
  - `Add(TKey key, TValue value)`：添加键/值对，若键已存在则抛出异常。
  - `Remove(TKey key)`：移除指定键的键/值对。
  - `ContainsKey(TKey key)`：检查是否包含指定键。
  - `TryGetValue(TKey key, out TValue value)`：尝试获取值。
  - `IndexOfKey(TKey key)`：返回指定键的索引。
  - `IndexOfValue(TValue value)`：返回指定值在值数组中的索引。
  - `Keys` / `Values`：获取按排序顺序排列的键/值集合。
- **适用场景**：数据量相对固定、以**查询为主、插入删除较少**的场景，如配置表、静态映射表。

```csharp
var scores = new SortedList<string, int>
{
    ["Alice"] = 92,
    ["Bob"] = 85,
    ["Charlie"] = 97,
};

// 键自动按字母顺序排列
foreach (var pair in scores)
{
    Console.WriteLine($"{pair.Key}: {pair.Value}");
}
// 输出：
// Alice: 92
// Bob: 85
// Charlie: 97

Console.WriteLine(scores.IndexOfKey("Bob")); // 输出：1
```

### 2. `SortedDictionary<TKey, TValue>`

- **描述**：按**键排序**的键值对集合，内部使用**红黑树**实现。插入、删除和查找的时间复杂度均为 O(log n)，在频繁增删的场景下优于 `SortedList`，但内存占用更高。
- **常用方法**：
  - `Add(TKey key, TValue value)`：添加键/值对。
  - `Remove(TKey key)`：移除指定键的键/值对。
  - `ContainsKey(TKey key)`：检查是否包含指定键。
  - `TryGetValue(TKey key, out TValue value)`：尝试获取值。
  - `Min` / `Max`：获取最小/最大的键。
  - `Keys` / `Values`：获取按排序顺序排列的键/值集合。
- **适用场景**：**频繁插入和删除**、需要始终维持有序状态的动态数据，如实时排行榜、日程调度表。

```csharp
var schedule = new SortedDictionary<DateTime, string>();
var today = DateTime.Today;

schedule.Add(today.AddHours(9), "晨会");
schedule.Add(today.AddHours(14), "代码评审");
schedule.Add(today.AddHours(10), "需求评审");

foreach (var pair in schedule)
{
    Console.WriteLine($"{pair.Key:HH:mm} - {pair.Value}");
}
// 输出：
// 09:00 - 晨会
// 10:00 - 需求评审
// 14:00 - 代码评审
```

### 3. 两者对比

| 维度 | `SortedList<TKey, TValue>` | `SortedDictionary<TKey, TValue>` |
| --- | --- | --- |
| 内部实现 | 连续数组 + 二分查找 | 红黑树 |
| 查找 | O(log n)，缓存友好，通常更快 | O(log n) |
| 插入/删除 | O(n)（需移动元素） | O(log n) |
| 内存占用 | 更紧凑 | 更高（每个节点含指针） |
| 索引访问 | 支持 `IndexOfKey` 等索引操作 | 不支持 |
| 典型场景 | 静态数据、查询为主 | 动态数据、频繁增删 |

### 4. 与 `SortedSet<T>` 的关系

三者都基于"自动排序"这一主题，区别只在于存储形态：

- `SortedSet<T>`：只存**值**的有序集合（见 [常用数据结构](./basic)）。
- `SortedList` / `SortedDictionary`：存**键值对**，按键排序。

如果键和值相同，直接用 `SortedSet<T>`；如果需要关联数据，则在两者中按增删频率选型。另外，`SortedDictionary<TKey, TValue>` 与普通 `Dictionary<TKey, TValue>` 相比，牺牲了 O(1) 查找换来了有序性——**只有在确实需要按键排序输出时才应选用**，否则直接用 `Dictionary`。
