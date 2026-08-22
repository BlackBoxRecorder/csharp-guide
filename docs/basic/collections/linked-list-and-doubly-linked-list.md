---
title: 链表与双向链表：原理、实现与应用
slug: linked-list-and-doubly-linked-list
---

# 链表与双向链表：原理、实现与应用

## 1. 引言

链表是计算机科学中最基础且重要的数据结构之一。它是一种线性数据结构，其中元素（节点）通过指针连接，而非像数组那样在内存中连续存储。

### 为什么需要学习链表？

1. **理解内存管理**：链表帮助我们理解动态内存分配和指针的概念
2. **掌握基础数据结构**：许多高级数据结构（栈、队列、图等）都基于链表实现
3. **解决实际问题**：在特定场景下，链表比数组更高效
4. **面试必备**：链表问题是技术面试中的常见题目

### 本文内容

本文将涵盖：
- 链表和双向链表的基本概念
- 底层原理和内存表示
- 完整的 C# 实现（代码示例可在 `src/DataStructures` 项目中找到）
- 实际应用场景（LRU 缓存）
- 性能分析和最佳实践

---

## 2. 链表基础

### 2.1 单向链表（Singly Linked List）

单向链表是最简单的链表形式，每个节点包含：
- **数据域**：存储实际数据
- **指针域**：指向下一个节点的引用

#### 节点结构

```csharp
public class ListNode<T>
{
    public T Value { get; set; }
    public ListNode<T>? Next { get; set; }
    
    public ListNode(T value, ListNode<T>? next = null)
    {
        Value = value;
        Next = next;
    }
}
```

#### 链表的内存表示

```text
单向链表的逻辑结构：

head
  │
  ▼
┌─────┬─────┐   ┌─────┬─────┐   ┌─────┬─────┐   ┌─────┬─────┐
│  1  │  ───┼──→│  2  │  ───┼──→│  3  │  ───┼──→│  4  │ null│
└─────┴─────┘   └─────┴─────┘   └─────┴─────┘   └─────┴─────┘

内存中的实际分布（非连续）：

地址: 0x100    0x200    0x300    0x400
     ┌─────┐  ┌─────┐  ┌─────┐  ┌─────┐
     │  1  │  │  3  │  │  2  │  │  4  │
     │  ───┼→ │  ───┼→ │  ───┼→ │ null│
     └─────┘  └─────┘  └─────┘  └─────┘
      ↑
     head
```

### 2.2 双向链表（Doubly Linked List）

双向链表的每个节点包含：
- **数据域**：存储实际数据
- **前驱指针**：指向前一个节点
- **后继指针**：指向后一个节点

#### 节点结构

```csharp
public class DoublyListNode<T>
{
    public T Value { get; set; }
    public DoublyListNode<T>? Prev { get; set; }
    public DoublyListNode<T>? Next { get; set; }
    
    public DoublyListNode(T value, DoublyListNode<T>? prev = null, DoublyListNode<T>? next = null)
    {
        Value = value;
        Prev = prev;
        Next = next;
    }
}
```

#### 链表的内存表示

```text
双向链表的逻辑结构：

head                                           tail
  │                                               │
  ▼                                               ▼
┌─────┬─────┬─────┐   ┌─────┬─────┬─────┐   ┌─────┬─────┬─────┐
│null │  1  │  ───┼──→│  ←──│  2  │  ───┼──→│  ←──│  3  │ null│
└─────┴─────┴─────┘   └─────┴─────┴─────┘   └─────┴─────┴─────┘
      ↑                                               │
      └───────────────────────────────────────────────┘

特点：
- 每个节点有两个指针（prev 和 next）
- 可以双向遍历
- 删除操作更高效（不需要查找前驱节点）
```

---

## 3. 链表原理

### 3.1 内存分配

#### 数组 vs 链表的内存分配

```text
数组（连续内存）：
┌─────┬─────┬─────┬─────┬─────┐
│  0  │  1  │  2  │  3  │  4  │  ← 连续的内存块
└─────┴─────┴─────┴─────┴─────┘
地址：100   104   108   112   116

链表（动态内存）：
┌─────┬─────┐   ┌─────┬─────┐   ┌─────┬─────┐
│  A  │  ───┼──→│  B  │  ───┼──→│  C  │ null│  ← 分散的内存块
└─────┴─────┘   └─────┴─────┘   └─────┴─────┘
地址：200        500        300
```

#### 动态内存分配的优势

1. **无需预先分配大小**：链表可以在运行时动态增长或缩小
2. **内存利用率高**：只在需要时分配内存
3. **插入删除高效**：不需要移动其他元素

### 3.2 基本操作原理

#### 插入操作

**在头部插入（O(1)）：**

```text
步骤：
1. 创建新节点
2. 新节点的 next 指向当前 head
3. 更新 head 指向新节点

示例：在头部插入节点 0

插入前：
head → [1] → [2] → [3] → null

插入后：
head → [0] → [1] → [2] → [3] → null
```

**在中间插入（O(n)）：**

```text
步骤：
1. 找到插入位置的前一个节点（prev）
2. 创建新节点
3. 新节点的 next 指向 prev.next
4. prev.next 指向新节点

示例：在节点 1 和 2 之间插入节点 1.5

插入前：
head → [1] → [2] → [3] → null

插入后：
head → [1] → [1.5] → [2] → [3] → null
```

#### 删除操作

**删除头部节点（O(1)）：**

```text
步骤：
1. 保存当前 head
2. 更新 head 指向 head.next
3. 释放原 head 节点

示例：删除节点 1

删除前：
head → [1] → [2] → [3] → null

删除后：
head → [2] → [3] → null
```

**删除中间节点（O(n)）：**

```text
步骤：
1. 找到要删除节点的前一个节点（prev）
2. 保存要删除的节点（current）
3. prev.next 指向 current.next
4. 释放 current 节点

示例：删除节点 2

删除前：
head → [1] → [2] → [3] → null

删除后：
head → [1] → [3] → null
```

#### 搜索操作

```text
顺序搜索（O(n)）：
从 head 开始，逐个节点比较，直到找到目标或到达链表末尾

示例：搜索节点 3

步骤 1: head → [1] → [2] → [3] → null
        检查 [1]：不是 3，继续

步骤 2: head → [1] → [2] → [3] → null
              检查 [2]：不是 3，继续

步骤 3: head → [1] → [2] → [3] → null
                    检查 [3]：找到！
```

### 3.3 时间复杂度分析

| 操作 | 单向链表 | 双向链表 | 数组 |
|------|----------|----------|------|
| 访问第 i 个元素 | O(n) | O(n) | O(1) |
| 头部插入 | O(1) | O(1) | O(n) |
| 尾部插入 | O(n) | O(1) | O(1) |
| 中间插入 | O(n) | O(n) | O(n) |
| 头部删除 | O(1) | O(1) | O(n) |
| 尾部删除 | O(n) | O(1) | O(1) |
| 中间删除 | O(n) | O(1)* | O(n) |
| 搜索 | O(n) | O(n) | O(n) |

*注：双向链表在已知节点位置的情况下，删除操作为 O(1)

---

## 4. C# 实现

### 4.1 单向链表实现

```csharp
using System;
using System.Collections.Generic;

public class ListNode<T>
{
    public T Value { get; set; }
    public ListNode<T>? Next { get; set; }
    
    public ListNode(T value, ListNode<T>? next = null)
    {
        Value = value;
        Next = next;
    }
    
    public override string ToString() => $"ListNode({Value})";
}

public class SinglyLinkedList<T>
{
    private ListNode<T>? _head;
    public int Count { get; private set; }
    
    public bool IsEmpty => _head is null;
    
    public void InsertAtHead(T value)
    {
        var newNode = new ListNode<T>(value, _head);
        _head = newNode;
        Count++;
    }
    
    public void InsertAtTail(T value)
    {
        var newNode = new ListNode<T>(value);
        if (IsEmpty)
        {
            _head = newNode;
        }
        else
        {
            var current = _head!;
            while (current.Next is not null)
            {
                current = current.Next;
            }
            current.Next = newNode;
        }
        Count++;
    }
    
    public bool InsertAt(int index, T value)
    {
        if (index < 0 || index > Count)
            return false;
        
        if (index == 0)
        {
            InsertAtHead(value);
            return true;
        }
        
        var current = _head!;
        for (int i = 0; i < index - 1; i++)
        {
            current = current.Next!;
        }
        
        var newNode = new ListNode<T>(value, current.Next);
        current.Next = newNode;
        Count++;
        return true;
    }
    
    public T? RemoveAtHead()
    {
        if (IsEmpty)
            return default;
        
        var value = _head!.Value;
        _head = _head.Next;
        Count--;
        return value;
    }
    
    public bool Remove(T value)
    {
        if (IsEmpty)
            return false;
        
        if (EqualityComparer<T>.Default.Equals(_head!.Value, value))
        {
            _head = _head.Next;
            Count--;
            return true;
        }
        
        var current = _head;
        while (current?.Next is not null && 
               !EqualityComparer<T>.Default.Equals(current.Next.Value, value))
        {
            current = current.Next;
        }
        
        if (current?.Next is not null)
        {
            current.Next = current.Next.Next;
            Count--;
            return true;
        }
        
        return false;
    }
    
    public bool Contains(T value)
    {
        var current = _head;
        while (current is not null)
        {
            if (EqualityComparer<T>.Default.Equals(current.Value, value))
                return true;
            current = current.Next;
        }
        return false;
    }
    
    public T? GetAt(int index)
    {
        if (index < 0 || index >= Count)
            return default;
        
        var current = _head!;
        for (int i = 0; i < index; i++)
        {
            current = current.Next!;
        }
        
        return current.Value;
    }
    
    public string Display()
    {
        if (IsEmpty)
            return "Empty LinkedList";
        
        var parts = new List<string>();
        var current = _head;
        
        while (current is not null)
        {
            parts.Add(current.Value?.ToString() ?? "null");
            current = current.Next;
        }
        
        return string.Join(" → ", parts) + " → null";
    }
    
    public override string ToString() => $"SinglyLinkedList(Count={Count})";
}

// 使用示例
var linkedList = new SinglyLinkedList<int>();

// 插入元素
linkedList.InsertAtTail(1);
linkedList.InsertAtTail(2);
linkedList.InsertAtTail(3);
linkedList.InsertAtHead(0);

Console.WriteLine($"链表: {linkedList.Display()}");
// 输出：链表: 0 → 1 → 2 → 3 → null

Console.WriteLine($"长度: {linkedList.Count}");
// 输出：长度: 4

// 搜索元素
Console.WriteLine($"搜索 2: {linkedList.Contains(2)}");
// 输出：搜索 2: True

Console.WriteLine($"搜索 5: {linkedList.Contains(5)}");
// 输出：搜索 5: False

// 删除元素
linkedList.Remove(2);
Console.WriteLine($"删除 2 后: {linkedList.Display()}");
// 输出：删除 2 后: 0 → 1 → 3 → null
```

### 4.2 双向链表实现

```csharp
using System;
using System.Collections.Generic;

public class DoublyListNode<T>
{
    public T Value { get; set; }
    public DoublyListNode<T>? Prev { get; set; }
    public DoublyListNode<T>? Next { get; set; }
    
    public DoublyListNode(T value, DoublyListNode<T>? prev = null, DoublyListNode<T>? next = null)
    {
        Value = value;
        Prev = prev;
        Next = next;
    }
    
    public override string ToString() => $"DoublyListNode({Value})";
}

public class DoublyLinkedList<T>
{
    private DoublyListNode<T>? _head;
    private DoublyListNode<T>? _tail;
    public int Count { get; private set; }
    
    public bool IsEmpty => _head is null;
    
    public void InsertAtHead(T value)
    {
        var newNode = new DoublyListNode<T>(value);
        
        if (IsEmpty)
        {
            _head = newNode;
            _tail = newNode;
        }
        else
        {
            newNode.Next = _head;
            _head!.Prev = newNode;
            _head = newNode;
        }
        Count++;
    }
    
    public void InsertAtTail(T value)
    {
        var newNode = new DoublyListNode<T>(value);
        
        if (IsEmpty)
        {
            _head = newNode;
            _tail = newNode;
        }
        else
        {
            newNode.Prev = _tail;
            _tail!.Next = newNode;
            _tail = newNode;
        }
        Count++;
    }
    
    public bool InsertAfter(DoublyListNode<T> node, T value)
    {
        if (node is null)
            return false;
        
        var newNode = new DoublyListNode<T>(value);
        
        newNode.Next = node.Next;
        newNode.Prev = node;
        
        if (node.Next is not null)
        {
            node.Next.Prev = newNode;
        }
        
        node.Next = newNode;
        
        if (node == _tail)
        {
            _tail = newNode;
        }
        
        Count++;
        return true;
    }
    
    public bool RemoveNode(DoublyListNode<T> node)
    {
        if (node is null)
            return false;
        
        // 更新前一个节点的 Next 指针
        if (node.Prev is not null)
        {
            node.Prev.Next = node.Next;
        }
        else
        {
            _head = node.Next;
        }
        
        // 更新后一个节点的 Prev 指针
        if (node.Next is not null)
        {
            node.Next.Prev = node.Prev;
        }
        else
        {
            _tail = node.Prev;
        }
        
        Count--;
        return true;
    }
    
    public bool Remove(T value)
    {
        var node = FindNode(value);
        return node is not null && RemoveNode(node);
    }
    
    private DoublyListNode<T>? FindNode(T value)
    {
        var current = _head;
        while (current is not null)
        {
            if (EqualityComparer<T>.Default.Equals(current.Value, value))
                return current;
            current = current.Next;
        }
        return null;
    }
    
    public bool Contains(T value) => FindNode(value) is not null;
    
    public T? GetAt(int index)
    {
        if (index < 0 || index >= Count)
            return default;
        
        // 优化：从头部或尾部开始遍历，取决于哪个更近
        DoublyListNode<T> current;
        if (index < Count / 2)
        {
            current = _head!;
            for (int i = 0; i < index; i++)
            {
                current = current.Next!;
            }
        }
        else
        {
            current = _tail!;
            for (int i = Count - 1; i > index; i--)
            {
                current = current.Prev!;
            }
        }
        
        return current.Value;
    }
    
    public T[] ToArray()
    {
        var result = new T[Count];
        var current = _head;
        int index = 0;
        
        while (current is not null)
        {
            result[index++] = current.Value;
            current = current.Next;
        }
        
        return result;
    }
    
    public T[] ToArrayReverse()
    {
        var result = new T[Count];
        var current = _tail;
        int index = 0;
        
        while (current is not null)
        {
            result[index++] = current.Value;
            current = current.Prev;
        }
        
        return result;
    }
    
    public string Display()
    {
        if (IsEmpty)
            return "Empty DoublyLinkedList";
        
        var parts = new List<string>();
        var current = _head;
        
        while (current is not null)
        {
            parts.Add(current.Value?.ToString() ?? "null");
            current = current.Next;
        }
        
        return "null ← " + string.Join(" ⇄ ", parts) + " → null";
    }
    
    public override string ToString() => $"DoublyLinkedList(Count={Count})";
}

// 使用示例
var doublyList = new DoublyLinkedList<int>();

// 插入元素
doublyList.InsertAtTail(1);
doublyList.InsertAtTail(2);
doublyList.InsertAtTail(3);
doublyList.InsertAtHead(0);

Console.WriteLine($"双向链表: {doublyList.Display()}");
// 输出：双向链表: null ← 0 ⇄ 1 ⇄ 2 ⇄ 3 → null

Console.WriteLine($"长度: {doublyList.Count}");
// 输出：长度: 4

// 正向遍历
Console.WriteLine($"正向遍历: [{string.Join(", ", doublyList.ToArray())}]");
// 输出：正向遍历: [0, 1, 2, 3]

// 反向遍历
Console.WriteLine($"反向遍历: [{string.Join(", ", doublyList.ToArrayReverse())}]");
// 输出：反向遍历: [3, 2, 1, 0]

// 删除元素
doublyList.Remove(2);
Console.WriteLine($"删除 2 后: {doublyList.Display()}");
// 输出：删除 2 后: null ← 0 ⇄ 1 ⇄ 3 → null
```

### 4.3 代码示例总结

```csharp
// 单向链表示例
var sll = new SinglyLinkedList<int>();
sll.InsertAtHead(1);
sll.InsertAtTail(2);
sll.InsertAtTail(3);
Console.WriteLine(sll.Display());  // 输出：1 → 2 → 3 → null

// 双向链表示例
var dll = new DoublyLinkedList<int>();
dll.InsertAtHead(1);
dll.InsertAtTail(2);
dll.InsertAtTail(3);
Console.WriteLine(dll.Display());  // 输出：null ← 1 ⇄ 2 ⇄ 3 → null
```

---

## 5. 使用场景

### 5.1 链表的典型应用场景

#### 1. 动态数据集合
- **场景**：数据量不确定，需要频繁增删
- **优势**：无需预先分配大小，动态扩展
- **示例**：实时数据流处理、日志系统

#### 2. 频繁插入删除操作
- **场景**：需要在任意位置频繁插入删除
- **优势**：插入删除操作时间复杂度为 O(1)（已知位置）
- **示例**：文本编辑器、撤销/重做功能

#### 3. 实现其他数据结构
- **栈**：使用链表实现，支持动态大小
- **队列**：使用链表实现，高效入队出队
- **图**：邻接表表示使用链表

#### 4. 内存受限环境
- **场景**：内存碎片化严重
- **优势**：不需要连续内存空间
- **示例**：嵌入式系统、旧式内存管理

### 5.2 双向链表的优势

#### 1. 双向遍历
- **场景**：需要从任意位置向前或向后遍历
- **示例**：浏览器历史记录（前进/后退）

#### 2. 删除操作更高效
- **场景**：需要删除已知节点
- **优势**：不需要查找前驱节点，O(1) 时间复杂度
- **示例**：LRU 缓存淘汰

#### 3. 实际应用示例

**浏览器历史记录：**

```text
双向链表实现浏览器历史：

当前页面：Google
历史：null ← Yahoo ← Google → Amazon → null

点击后退：
当前页面：Yahoo
历史：null ← Yahoo → Google → Amazon → null

点击前进：
当前页面：Google
历史：null ← Yahoo ← Google → Amazon → null
```

**音乐播放列表：**

```text
双向链表实现播放列表：

当前歌曲：Song B
播放列表：null ← Song A ⇄ Song B ⇄ Song C → null

下一首：
当前歌曲：Song C
播放列表：null ← Song A ⇄ Song B ⇄ Song C → null

上一首：
当前歌曲：Song B
播放列表：null ← Song A ⇄ Song B ⇄ Song C → null
```

### 5.3 链表与数组的比较

| 特性 | 链表 | 数组 |
|------|------|------|
| **内存分配** | 动态，非连续 | 静态，连续 |
| **大小调整** | 动态增长/缩小 | 固定大小（或需要重新分配） |
| **随机访问** | O(n) | O(1) |
| **插入删除** | O(1)（已知位置） | O(n) |
| **内存开销** | 额外指针开销 | 无额外开销 |
| **缓存友好性** | 差（内存不连续） | 好（内存连续） |
| **适用场景** | 频繁插入删除 | 频繁随机访问 |

#### 何时选择链表？

1. **需要频繁插入删除**：链表在已知位置的插入删除为 O(1)
2. **数据量不确定**：链表可以动态增长
3. **不需要随机访问**：如果主要顺序访问数据
4. **内存碎片化**：链表不需要连续内存

#### 何时选择数组？

1. **需要随机访问**：数组支持 O(1) 随机访问
2. **数据量固定**：数组大小已知且不变
3. **缓存性能重要**：数组内存连续，缓存友好
4. **内存效率要求高**：数组无额外指针开销

---

## 6. 实际应用：LRU 缓存

### 6.1 什么是 LRU 缓存？

LRU（Least Recently Used，最近最少使用）是一种常见的缓存淘汰策略。当缓存满时，优先移除最近最少使用的数据。

#### LRU 缓存的工作原理

```text
LRU 缓存示例（容量为 3）：

初始状态：
缓存：{}
访问顺序：[]

访问 A：
缓存：{A}
访问顺序：[A]

访问 B：
缓存：{A, B}
访问顺序：[A, B]

访问 C：
缓存：{A, B, C}
访问顺序：[A, B, C]

访问 D（缓存满，淘汰 A）：
缓存：{B, C, D}
访问顺序：[B, C, D]

再次访问 B（命中，更新顺序）：
缓存：{B, C, D}
访问顺序：[C, D, B]
```

### 6.2 LRU 缓存实现

LRU 缓存通常使用 **双向链表 + 哈希表** 实现：

- **双向链表**：维护访问顺序，头部是最近使用的，尾部是最久未使用的
- **哈希表**：提供 O(1) 的查找能力

#### 数据结构设计

```text
LRU 缓存结构：

哈希表（HashMap）：
┌─────────┬─────────┐
│    key  │  node   │
├─────────┼─────────┤
│   "A"   │ [A节点] │
│   "B"   │ [B节点] │
│   "C"   │ [C节点] │
└─────────┴─────────┘

双向链表（按访问顺序）：
head ⇄ [最近使用] ⇄ [次近使用] ⇄ ... ⇄ [最久未使用] ⇄ tail

操作：
- get(key): 查找 key，如果存在，将节点移到链表头部
- put(key, value): 插入/更新 key-value，将节点移到链表头部，如果满了则删除尾部节点
```

### 6.3 完整代码示例

```csharp
using System;
using System.Collections.Generic;

public class LRUNode<TKey, TValue>
{
    public TKey Key { get; }
    public TValue Value { get; set; }
    public LRUNode<TKey, TValue>? Prev { get; set; }
    public LRUNode<TKey, TValue>? Next { get; set; }
    
    public LRUNode(TKey key, TValue value)
    {
        Key = key;
        Value = value;
    }
}

public class LRUCache<TKey, TValue> where TKey : notnull
{
    private readonly int _capacity;
    private readonly Dictionary<TKey, LRUNode<TKey, TValue>> _cache;
    private readonly LRUNode<TKey, TValue> _head;
    private readonly LRUNode<TKey, TValue> _tail;
    
    public LRUCache(int capacity)
    {
        _capacity = capacity;
        _cache = new Dictionary<TKey, LRUNode<TKey, TValue>>(capacity);
        
        _head = new LRUNode<TKey, TValue>(default!, default!);
        _tail = new LRUNode<TKey, TValue>(default!, default!);
        _head.Next = _tail;
        _tail.Prev = _head;
    }
    
    private void AddNode(LRUNode<TKey, TValue> node)
    {
        node.Prev = _head;
        node.Next = _head.Next;
        
        _head.Next!.Prev = node;
        _head.Next = node;
    }
    
    private void RemoveNode(LRUNode<TKey, TValue> node)
    {
        node.Prev!.Next = node.Next;
        node.Next!.Prev = node.Prev;
    }
    
    private void MoveToHead(LRUNode<TKey, TValue> node)
    {
        RemoveNode(node);
        AddNode(node);
    }
    
    private LRUNode<TKey, TValue> PopTail()
    {
        var node = _tail.Prev!;
        RemoveNode(node);
        return node;
    }
    
    public TValue? Get(TKey key)
    {
        if (!_cache.TryGetValue(key, out var node))
            return default;
        
        MoveToHead(node);
        
        return node.Value;
    }
    
    public void Put(TKey key, TValue value)
    {
        if (_cache.TryGetValue(key, out var node))
        {
            node.Value = value;
            MoveToHead(node);
        }
        else
        {
            var newNode = new LRUNode<TKey, TValue>(key, value);
            _cache[key] = newNode;
            AddNode(newNode);
            
            if (_cache.Count > _capacity)
            {
                var tailNode = PopTail();
                _cache.Remove(tailNode.Key);
            }
        }
    }
    
    public void Display()
    {
        Console.WriteLine($"Cache (size={_cache.Count}, capacity={_capacity}):");
        
        Console.WriteLine("HashMap:");
        foreach (var kvp in _cache)
        {
            Console.WriteLine($"  {kvp.Key}: {kvp.Value.Value}");
        }
        
        Console.WriteLine("Order (most recent → least recent):");
        var parts = new List<string>();
        var current = _head.Next;
        while (current != _tail)
        {
            parts.Add($"{current!.Key}={current.Value}");
            current = current.Next;
        }
        Console.WriteLine($"  {string.Join(" → ", parts)}");
        Console.WriteLine();
    }
}

// 使用示例
var cache = new LRUCache<int, int>(3);

// 测试基本操作
Console.WriteLine("=== LRU 缓存测试 ===\n");

// 1. 添加元素
cache.Put(1, 100);
Console.WriteLine("添加 (1, 100):");
cache.Display();
// 输出：
// Cache (size=1, capacity=3):
// HashMap:
//   1: 100
// Order (most recent → least recent):
//   1=100

cache.Put(2, 200);
Console.WriteLine("添加 (2, 200):");
cache.Display();
// 输出：
// Cache (size=2, capacity=3):
// HashMap:
//   1: 100
//   2: 200
// Order (most recent → least recent):
//   2=200 → 1=100

cache.Put(3, 300);
Console.WriteLine("添加 (3, 300):");
cache.Display();
// 输出：
// Cache (size=3, capacity=3):
// HashMap:
//   1: 100
//   2: 200
//   3: 300
// Order (most recent → least recent):
//   3=300 → 2=200 → 1=100

// 2. 访问元素（更新访问顺序）
Console.WriteLine($"获取 key=1: {cache.Get(1)}");
// 输出：获取 key=1: 100

Console.WriteLine("访问 key=1 后:");
cache.Display();
// 输出：
// Cache (size=3, capacity=3):
// HashMap:
//   1: 100
//   2: 200
//   3: 300
// Order (most recent → least recent):
//   1=100 → 3=300 → 2=200

// 3. 添加新元素（触发淘汰）
cache.Put(4, 400);
Console.WriteLine("添加 (4, 400)（key=2 应该被淘汰）:");
cache.Display();
// 输出：
// Cache (size=3, capacity=3):
// HashMap:
//   1: 100
//   3: 300
//   4: 400
// Order (most recent → least recent):
//   4=400 → 1=100 → 3=300

// 4. 验证被淘汰的元素
Console.WriteLine($"获取 key=2: {cache.Get(2)}");
// 输出：获取 key=2: 

Console.WriteLine($"获取 key=3: {cache.Get(3)}");
// 输出：获取 key=3: 300

// 5. 更新已存在的元素
cache.Put(3, 301);
Console.WriteLine("更新 (3, 301):");
cache.Display();
// 输出：
// Cache (size=3, capacity=3):
// HashMap:
//   1: 100
//   3: 301
//   4: 400
// Order (most recent → least recent):
//   3=301 → 4=400 → 1=100

Console.WriteLine("\n=== 性能测试 ===\n");

// 性能测试
var largeCache = new LRUCache<int, int>(1000);

// 测试插入性能
var startTime = DateTime.UtcNow;
for (int i = 0; i < 10000; i++)
{
    largeCache.Put(i, i * 10);
}
var endTime = DateTime.UtcNow;
Console.WriteLine($"插入 10000 个元素: {(endTime - startTime).TotalMilliseconds:F4} 毫秒");

// 测试查询性能
startTime = DateTime.UtcNow;
for (int i = 0; i < 10000; i++)
{
    largeCache.Get(i);
}
endTime = DateTime.UtcNow;
Console.WriteLine($"查询 10000 次: {(endTime - startTime).TotalMilliseconds:F4} 毫秒");
```

### 6.4 LRU 缓存的复杂度分析

| 操作 | 时间复杂度 | 空间复杂度 |
|------|----------|----------|
| get(key) | O(1) | O(capacity) |
| put(key, value) | O(1) | O(capacity) |

**为什么是 O(1)？**
- 哈希表查找：O(1)
- 双向链表插入/删除（已知节点）：O(1)
- 移动到头部：O(1)

### 6.5 LRU 缓存的实际应用

1. **操作系统页面置换**：内存页面管理
2. **数据库查询缓存**：缓存频繁查询的结果
3. **Web 浏览器**：缓存最近访问的网页
4. **CDN（内容分发网络）**：缓存热门内容
5. **Redis 等缓存系统**：支持 LRU 淘汰策略

---

## 7. 总结

### 链表的优缺点

#### 优点
1. **动态大小**：可以随时增长或缩小
2. **高效插入删除**：在已知位置的插入删除为 O(1)
3. **内存灵活**：不需要连续内存空间
4. **实现其他数据结构**：栈、队列、图等

#### 缺点
1. **无随机访问**：访问第 i 个元素需要 O(n)
2. **额外内存开销**：每个节点需要存储指针
3. **缓存不友好**：内存不连续，缓存命中率低
4. **反向遍历困难**：单向链表不支持反向遍历

### 何时选择链表？

1. **频繁插入删除**：如果需要在任意位置频繁插入删除
2. **数据量不确定**：如果数据量会动态变化
3. **不需要随机访问**：如果主要顺序访问数据
4. **实现特定数据结构**：如 LRU 缓存、图邻接表等

### 学习建议

1. **理解原理**：深入理解链表的内存表示和操作原理
2. **动手实现**：自己实现链表，而不仅仅使用库
3. **练习题目**：通过 LeetCode 等平台练习链表题目
4. **比较分析**：理解链表与数组的区别和适用场景
5. **实际应用**：通过 LRU 缓存等实际项目加深理解

### 进一步学习

1. **循环链表**：最后一个节点指向第一个节点
2. **跳表（Skip List）**：多层链表，支持 O(log n) 查找
3. **链表与其他数据结构的结合**：如哈希链表（LinkedHashMap）
4. **并发链表**：线程安全的链表实现

---

## 附录：常见链表操作时间复杂度

| 操作 | 单向链表 | 双向链表 | 备注 |
|------|----------|----------|------|
| 访问头部 | O(1) | O(1) | |
| 访问尾部 | O(n) | O(1) | 双向链表有 tail 指针 |
| 访问第 i 个 | O(n) | O(n) | |
| 头部插入 | O(1) | O(1) | |
| 尾部插入 | O(n) | O(1) | 双向链表有 tail 指针 |
| 中间插入（已知位置） | O(1) | O(1) | |
| 中间插入（未知位置） | O(n) | O(n) | 需要先查找 |
| 头部删除 | O(1) | O(1) | |
| 尾部删除 | O(n) | O(1) | 双向链表有 tail 指针 |
| 中间删除（已知节点） | O(n) | O(1) | 单向链表需要找前驱 |
| 搜索 | O(n) | O(n) | |

---

_创建日期：2026-08-20_
_作者：TinyBlog_
