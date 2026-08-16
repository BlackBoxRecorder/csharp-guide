---
title: 内存与高性能处理
description: C# 内存与高性能处理类型详解，包括 Memory、ArraySegment、StringBuilder、Buffer 的用法、与 Span 的配合及选型建议。
---

[常用数据结构](./basic) 中提到的 `Span<T>` 是栈上的高性能内存视图，但它**不能存进字段、不能用于 `async` 方法**。本文介绍围绕内存和字符串处理的一组补充类型，它们与 `Span<T>` 互补，覆盖"跨异步边界"和"减少分配"两大场景。

### 1. `Memory<T>` 与 `ReadOnlyMemory<T>`

- **描述**：`Span<T>` 的**堆版本**，可以存入字段、作为 `async` 方法的参数或返回值。它同样是一段连续内存的视图（内部持有 `T[]`、`string` 或原生内存的引用 + 长度），`Memory<T>.Span` 属性可以随时取回 `Span<T>`。
- **常用成员**：
  - `.Span`：获取对应的 `Span<T>` 视图。
  - `.Length`：元素数量。
  - `.Slice(int start, int length)`：截取子片段（同样不复制数据）。
  - `Memory<T>.Empty`：空实例。
- **适用场景**：需要跨 `await` 传递的缓冲区处理，如网络接收循环、流式解析。

```csharp
static async Task HandleAsync(Memory<byte> buffer)
{
    // 在异步方法中跨 await 使用 Memory<T>
    await Task.Delay(10);
    var span = buffer.Span;
    Console.WriteLine(span.Length); // 输出：8
}

var data = new byte[8];
await HandleAsync(data);
```

> 提示：`string` 与 `ReadOnlyMemory<char>` 之间可以零拷贝互转（`AsMemory()`）。而 `Span<T>` 只能用于同步的、栈生命周期的场景，两者按"是否需要跨异步边界"来取舍。

### 2. `ArraySegment<T>`

- **描述**：表示一个**数组的片段**（起始索引 + 长度），不复制元素。它在 .NET Framework 时代就被广泛使用，是"无拷贝子数组"的经典方案；如今同等需求更推荐 `Span<T>` / `Memory<T>`，但理解它对阅读旧代码仍有价值。
- **常用成员**：
  - `.Array`：原始数组。
  - `.Offset` / `.Count`：片段的起始索引和长度。
  - `.Slice(int start, int length)`：再截取子片段。
- **适用场景**：需要把数组的一部分传给 API 且不想分配新数组的场合。

```csharp
var array = new[] { 10, 20, 30, 40, 50 };
var segment = new ArraySegment<int>(array, 1, 3); // [20, 30, 40]

foreach (var value in segment)
{
    Console.Write($"{value} "); // 输出：20 30 40
}
```

### 3. `StringBuilder`

- **描述**：**可变字符串**。`string` 是不可变的，每次拼接都会创建新字符串；在循环中大量拼接时，`StringBuilder` 通过内部缓冲区原地修改，避免了频繁分配和复制。
- **常用方法**：
  - `Append(string value)` / `AppendLine(string value)`：追加内容。
  - `AppendFormat(string format, params object[] args)`：格式化追加。
  - `Insert(int index, string value)`：插入内容。
  - `Remove(int startIndex, int length)`：移除区间内容。
  - `Replace(string oldValue, string newValue)`：替换。
  - `ToString()`：生成最终字符串。
- **适用场景**：循环拼接、动态构造长文本（日志、HTML、SQL 拼接等）。

```csharp
var sb = new StringBuilder();
for (var i = 0; i < 5; i++)
{
    sb.Append(i).Append(' ');
}
Console.WriteLine(sb.ToString()); // 输出：0 1 2 3 4
```

> 注意：少量固定次数的拼接（如 2~3 个字符串）直接使用 `$""` 字符串插值即可，编译器会进行优化，不必一律上 `StringBuilder`。

### 4. `Buffer<T>`（.NET 8+）

- **描述**：`System.Buffers` 命名空间下的**内存缓冲**，内部按需持有 `T[]` 或原生内存，专门用于流式写入大量数据的场景（如 `Stream`、序列化输出）。它有"租用-使用-归还"的语义，可以避免为每块数据都分配新数组。
- **常用方法**：
  - `GetSpan(int sizeHint)`：获取写入用的 `Span<T>`。
  - `Advance(int count)`：确认已写入的字节数。
  - `GetMemory(int sizeHint)`：获取写入用的 `Memory<T>`（用于异步场景）。
- **适用场景**：高性能网络协议、序列化器等需要分块写出数据的底层组件；普通业务代码一般用不到。

```csharp
using System.Buffers;

var buffer = new Buffer<byte>();
var span = buffer.GetSpan(4);
span[0] = 0x48; // 'H'
span[1] = 0x49; // 'I'
buffer.Advance(2);
Console.WriteLine(buffer.Length); // 输出：2
```

### 5. 选型建议

| 需求 | 推荐类型 |
| --- | --- |
| 同步、栈生命周期内的连续内存视图 | `Span<T>` / `ReadOnlySpan<T>`（见 [常用数据结构](./basic)） |
| 跨 `async` 边界的连续内存视图 | `Memory<T>` / `ReadOnlyMemory<T>` |
| 数组子片段、不分配新数组 | `ArraySegment<T>`（或直接换用 Span/Memory） |
| 循环拼接字符串 | `StringBuilder` |
| 底层流式写出、分块缓冲 | `Buffer<T>`（.NET 8+） |
