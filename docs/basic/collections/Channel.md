---
title: Channel
description: C# Channel 详解，基于 System.Threading.Channels 的线程安全生产者-消费者数据结构，支持异步读写。
---

参考： <https://learn.microsoft.com/zh-cn/dotnet/core/extensions/channels>

在 C# 中，`Channel<T>` 是一个线程安全的数据结构，基于 `System.Threading.Channels` 命名空间。它被设计用来在生产者和消费者模式中进行线程间的通信。`Channel<T>` 类及其相关的读写操作（如 `WriteAsync` 和 `ReadAsync`）都是线程安全的，这意味着你可以在多个线程中并发地执行读取和写入操作，而无需额外的同步逻辑。

`Channel<T>` 提供了多种类型的通道，包括但不限于：

- `UnboundedChannel<T>`：可以容纳任意数量元素的通道。
- `BoundedChannel<T>`：有容量限制的通道，当达到上限时，写入操作会被阻塞直到有足够的空间。
- `SingleReaderChannel<T>`：允许多个写入者但只有一个读取者的通道。
- `SingleWriterChannel<T>`：允许多个读取者但只有一个写入者的通道。

使用 `Channel<T>` 时，你可以创建不同类型的通道以适应你的应用程序的需求。例如，如果你的应用程序需要多个生产者和多个消费者，那么你可以选择 `UnboundedChannel<T>` 或 `BoundedChannel<T>`。如果只需要单个消费者或单个生产者，那么可以选择相应的 `SingleReaderChannel<T>` 或 `SingleWriterChannel<T>` 以获得更好的性能。

需要注意的是，虽然 `Channel<T>` 的读写操作是线程安全的，但在多线程环境中使用时，仍然应该注意可能存在的竞态条件或其他并发问题，确保你的整体应用逻辑也是线程安全的。此外，`Channel<T>` 支持异步编程模型（基于 `async`/`await`），这使得它非常适合用于I/O密集型或CPU密集型的多线程应用场景。

以下是一个具体的示例，演示如何使用 Channel 来实现生产者-消费者模式。

### 示例代码

```csharp
using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        // 创建一个 Channel
        var channel = Channel.CreateUnbounded<int>();

        // 启动生产者
        var producer = Task.Run(async () =>
        {
            for (int i = 0; i < 10; i++)
            {
                await channel.Writer.WriteAsync(i);
                Console.WriteLine($"生产者: 生产了 {i}");
                await Task.Delay(500); // 模拟生产延迟
            }
            channel.Writer.Complete(); // 完成写入
        });

        // 启动消费者
        var consumer = Task.Run(async () =>
        {
            await foreach (var item in channel.Reader.ReadAllAsync())
            {
                Console.WriteLine($"消费者: 消费了 {item}");
                await Task.Delay(1000); // 模拟消费延迟
            }
        });

        await Task.WhenAll(producer, consumer);
    }
}
```

### 代码说明

1. **创建 Channel**: 使用 `Channel.CreateUnbounded<int>()` 创建一个无界通道，允许生产者和消费者之间传递整数。

2. **生产者**: 在一个任务中，生产者循环生成数字（0 到 9），并将其写入通道。每次写入后，生产者会等待一段时间以模拟生产延迟。

3. **消费者**: 在另一个任务中，消费者从通道中读取数据并处理。它使用 `await foreach` 循环来异步读取所有数据，并在处理每个项目后等待一段时间以模拟消费延迟。

4. **完成写入**: 生产者完成写入后调用 `channel.Writer.Complete()`，通知消费者没有更多数据可供读取。

### 运行结果

运行此程序时，您将看到生产者和消费者交替输出，显示生产和消费的过程。这个示例展示了如何使用 C# 的 Channel 来实现高效的异步数据传递。
