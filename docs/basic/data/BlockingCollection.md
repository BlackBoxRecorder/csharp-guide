---
title: BlockingCollection
description: C# BlockingCollection 详解，线程安全的生产者-消费者集合，包括阻塞和限制功能、并发编程实践。
---

`BlockingCollection<T>` 是 .NET Framework 4 中引入的一个线程安全的集合，它被设计用来实现生产者-消费者模式。`BlockingCollection` 提供了阻塞和限制功能，使得添加和获取元素的操作可以同步执行，而无需显式使用锁。
以下是 `BlockingCollection<T>` 的一些主要特点和用法：

### 创建 BlockingCollection

```csharp
BlockingCollection<T> bc = new BlockingCollection<T>();
```

或者，可以指定一个内部容量限制：

```csharp
int boundedCapacity = 100;
BlockingCollection<T> bc = new BlockingCollection<T>(boundedCapacity);
```

### 添加元素

添加元素到 `BlockingCollection` 可以使用 `Add` 方法，如果集合已满（达到容量限制），则该方法会阻塞调用线程。

```csharp
bc.Add(item);
```

也可以使用 `TryAdd` 方法，该方法在集合已满时不会阻塞，而是返回一个布尔值表示操作是否成功。

```csharp
bool success = bc.TryAdd(item);
```

### 取出元素

从 `BlockingCollection` 中取出元素可以使用 `Take` 方法，如果集合为空，则该方法会阻塞调用线程。

```csharp
T item = bc.Take();
```

同样，可以使用 `TryTake` 方法，该方法在集合为空时不会阻塞，而是返回一个布尔值表示操作是否成功。

```csharp
bool success = bc.TryTake(out T item， TimeSpan.FromSeconds(1));
```

### 完成添加操作

调用 `CompleteAdding` 方法来指示不再向集合中添加任何项。这通常在生产者完成其工作后调用。

```csharp
bc.CompleteAdding();
```

### 迭代集合

可以使用 `GetConsumingEnumerable` 方法来迭代集合中的元素。这个方法会阻塞，直到集合中有元素可消费。

```csharp
foreach (var item in bc.GetConsumingEnumerable())
{
    // 处理item
}
```

### 示例：生产者-消费者模式

以下是一个简单的生产者和消费者示例：

```csharp
BlockingCollection<int> bc = new BlockingCollection<int>();
// 生产者
Task producer = Task.Run(() =>
{
    for (int i = 0; i < 10; i++)
    {
        bc.Add(i);
        Console.WriteLine("Produced: " + i);
    }
    bc.CompleteAdding();
});
// 消费者
Task consumer = Task.Run(() =>
{
    foreach (var item in bc.GetConsumingEnumerable())
    {
        Console.WriteLine("Consumed: " + item);
    }
});
Task.WaitAll(producer, consumer);
```

在这个例子中，生产者任务向 `BlockingCollection` 中添加整数，而消费者任务从集合中取出并处理这些整数。当生产者完成添加操作后，它调用 `CompleteAdding` 方法来通知消费者没有更多的项可以处理。

使用 `BlockingCollection` 可以简化多线程编程，特别是处理生产者-消费者问题，因为它提供了线程安全的数据结构和阻塞操作，从而避免了复杂的同步问题。
