---
title: C# 中的常用数据结构
description: C# 常用数据结构详解，包括 List、Dictionary、Queue、Stack、LinkedList、HashSet、SortedList 等的使用场景和最佳实践。
---

C# 提供了丰富的数据结构,以下是 C# 中一些常见的数据结构，它们位于 `System.Collections` 和 `System.Collections.Generic` 命名空间中。这些数据结构可以帮助你有效地组织、存储和操作数据。

### 1. `List<T>`

- **描述**：`List<T>` 是一个动态数组，可以存储任意数量的指定类型的元素。它提供了快速的索引访问，并且可以在需要时自动扩展容量。
- **常用方法**：
  - `Add(T item)`：添加一个元素到列表末尾。
  - `Remove(T item)`：移除第一个匹配的元素。
  - `Clear()`：移除所有元素。
  - `Contains(T item)`：检查列表是否包含某个元素。
  - `IndexOf(T item)`：返回第一个匹配元素的索引。
  - `Sort()`：对列表进行排序。
  - `Reverse()`：反转列表中的元素顺序。
- **适用场景**：当你需要一个动态大小的数组，并且频繁地进行插入和删除操作时，`List<T>` 是一个很好的选择。

### 2. `Dictionary<TKey, TValue>`

- **描述**：`Dictionary<TKey, TValue>` 是一个键/值对集合，允许通过键快速查找值。它是线程不安全的，但提供了优秀的性能。
- **常用方法**：
  - `Add(TKey key, TValue value)`：添加一个键/值对。
  - `Remove(TKey key)`：移除指定键的键/值对。
  - `ContainsKey(TKey key)`：检查字典是否包含指定键。
  - `TryGetValue(TKey key, out TValue value)`：尝试获取与指定键关联的值。
  - `Keys`：获取字典中所有键的集合。
  - `Values`：获取字典中所有值的集合。
- **适用场景**：当你需要快速查找、插入和删除键/值对时，`Dictionary<TKey, TValue>` 是首选。

### 3. `HashSet<T>`

- **描述**：`HashSet<T>` 是一个无序的集合，不允许重复元素。它提供了非常快的查找、插入和删除操作（平均时间复杂度为 O(1)）。
- **常用方法**：
  - `Add(T item)`：添加一个元素到集合中。
  - `Remove(T item)`：移除指定的元素。
  - `Contains(T item)`：检查集合是否包含指定元素。
  - `UnionWith(IEnumerable<T> other)`：将另一个集合的所有元素添加到当前集合中。
  - `IntersectWith(IEnumerable<T> other)`：保留当前集合与另一个集合的交集。
  - `ExceptWith(IEnumerable<T> other)`：从当前集合中移除与另一个集合共有的元素。
- **适用场景**：当你需要一个没有重复元素的集合，并且需要高效的查找、插入和删除操作时，`HashSet<T>` 是理想的选择。

### 4. `Queue<T>`

- **描述**：`Queue<T>` 是一个先进先出（FIFO）的数据结构。你可以从队列的一端添加元素，从另一端移除元素。
- **常用方法**：
  - `Enqueue(T item)`：在队列末尾添加一个元素。
  - `Dequeue()`：移除并返回队列的第一个元素。
  - `Peek()`：返回队列的第一个元素，但不移除它。
  - `Count`：获取队列中的元素数量。
- **适用场景**：当你需要按照先进先出的原则处理任务或事件时，`Queue<T>` 是合适的选择。

### 5. `Stack<T>`

- **描述**：`Stack<T>` 是一个后进先出（LIFO）的数据结构。你可以从栈顶添加和移除元素。
- **常用方法**：
  - `Push(T item)`：将一个元素压入栈顶。
  - `Pop()`：移除并返回栈顶的元素。
  - `Peek()`：返回栈顶的元素，但不移除它。
  - `Count`：获取栈中的元素数量。
- **适用场景**：当你需要按照后进先出的原则处理任务或事件时，`Stack<T>` 是合适的选择。

### 6. `LinkedList<T>`

- **描述**：`LinkedList<T>` 是一个双向链表，每个节点包含一个前驱指针和一个后继指针。它适合频繁的插入和删除操作，但在随机访问方面不如数组或列表高效。
- **常用方法**：
  - `AddFirst(T item)`：在链表头部添加一个节点。
  - `AddLast(T item)`：在链表尾部添加一个节点。
  - `AddBefore(Node<T> node, T item)`：在指定节点之前添加一个节点。
  - `AddAfter(Node<T> node, T item)`：在指定节点之后添加一个节点。
  - `Remove(Node<T> node)`：移除指定的节点。
  - `RemoveFirst()`：移除链表头部的节点。
  - `RemoveLast()`：移除链表尾部的节点。
- **适用场景**：当你需要频繁地在集合的任意位置插入或删除元素时，`LinkedList<T>` 是一个不错的选择。

### 7. `SortedSet<T>`

- **描述**：`SortedSet<T>` 是一个有序的集合，不允许重复元素。它会根据元素的自然顺序或自定义比较器自动排序。
- **常用方法**：
  - `Add(T item)`：添加一个元素到集合中。
  - `Remove(T item)`：移除指定的元素。
  - `Min`：获取集合中的最小元素。
  - `Max`：获取集合中的最大元素。
  - `GetViewBetween(T lowerValue, T upperValue)`：返回介于两个值之间的子集合。
- **适用场景**：当你需要一个有序的、没有重复元素的集合时，`SortedSet<T>` 是一个很好的选择。

### 8. `Concurrent Collections`

- **描述**：`System.Collections.Concurrent` 命名空间提供了一系列线程安全的集合类，适用于多线程环境。常见的并发集合包括：
  - `ConcurrentDictionary<TKey, TValue>`：线程安全的键/值对集合。
  - `ConcurrentQueue<T>`：线程安全的队列。
  - `ConcurrentStack<T>`：线程安全的栈。
  - `BlockingCollection<T>`：一个线程安全的集合，支持生产者-消费者模式。
- **适用场景**：当你在多线程环境中需要安全地共享和操作集合时，应该使用这些并发集合类。

### 9. `Tuple<T1, T2, ...>`

- **描述**：`Tuple` 是一个用于存储多个不同类型的值的不可变对象。它可以有 1 到 8 个元素，也可以通过 `ValueTuple` 创建更复杂的元组。
- **常用方法**：
  - `Item1, Item2, ...`：访问元组中的各个元素。
  - `ToString()`：返回元组的字符串表示。
- **适用场景**：当你需要临时组合多个值并传递给其他方法时，`Tuple` 是一个方便的选择。不过，对于更复杂的场景，建议使用命名类型或记录（record）。

### 10. `Span<T> 和 ReadOnlySpan<T>`

- **描述**：`Span<T>` 和 `ReadOnlySpan<T>` 是轻量级的内存区域表示，允许高效地处理连续的内存块，而无需分配额外的堆内存。它们广泛用于高性能场景，如字符串处理、网络通信等。
- **适用场景**：当你需要高效地处理大量数据并且希望避免不必要的内存分配时，`Span<T>` 和 `ReadOnlySpan<T>` 是非常好的选择。

### 11. `Array`

- **描述**：`Array` 是 C# 中最基础的集合类型，表示固定大小的元素序列。它可以是单维或多维的。
- **常用方法**：
  - `Length`：获取数组的长度。
  - `Rank`：获取数组的维度数。
  - `GetLength(int dimension)`：获取指定维度的长度。
  - `GetValue(params int[] indices)`：获取指定索引处的元素。
  - `SetValue(object value, params int[] indices)`：设置指定索引处的元素。
- **适用场景**：当你知道集合的大小不会改变，并且需要快速的索引访问时，`Array` 是一个合适的选择。

### 12. `BitArray`

- **描述**：`BitArray` 是一个位集合，允许你高效地存储和操作布尔值（`true` 或 `false`）。它内部使用整数来表示位，因此非常适合处理大量布尔标志。
- **常用方法**：
  - `Set(int index, bool value)`：设置指定索引处的位。
  - `Get(int index)`：获取指定索引处的位。
  - `Not()`：对所有位取反。
  - `And(BitArray array)`：执行按位与操作。
  - `Or(BitArray array)`：执行按位或操作。
  - `Xor(BitArray array)`：执行按位异或操作。
- **适用场景**：当你需要处理大量的布尔标志或位掩码时，`BitArray` 是一个高效的选择。

### 13. `PriorityQueue<TElement, TPriority>`

- **描述**：`PriorityQueue<TElement, TPriority>` 是一个优先队列，允许你根据优先级插入和移除元素。优先级最高的元素总是最先被移除。
- **常用方法**：
  - `Enqueue(TElement element, TPriority priority)`：插入一个元素及其优先级。
  - `Dequeue()`：移除并返回优先级最高的元素。
  - `TryPeek(out TElement element, out TPriority priority)`：获取优先级最高的元素，但不移除它。
  - `TryDequeue(out TElement element, out TPriority priority)`：移除并返回优先级最高的元素。
- **适用场景**：当你需要根据优先级处理任务或事件时，`PriorityQueue<TElement, TPriority>` 是一个合适的选择。
