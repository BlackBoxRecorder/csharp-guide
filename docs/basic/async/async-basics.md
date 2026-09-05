---
title: 异步编程基础
description: C# 异步编程基础，包括 async/await、Task、Task<T>、ValueTask 等核心概念。
---

# 异步编程基础

异步编程是 C# 中处理耗时操作（如 I/O、网络请求、数据库查询）的核心技术，它允许程序在等待操作完成时继续执行其他任务，从而提高应用程序的响应性和吞吐量。

> **前置阅读**：[什么是异步](/basic/async/async) - 理解异步的本质和价值

## 2. async 和 await 关键字

C# 使用 `async` 和 `await` 关键字来简化异步编程。

### 2.1 async 关键字

`async` 关键字用于标记一个方法为异步方法。异步方法可以包含 `await` 表达式。

```csharp
public async Task<string> GetDataAsync()
{
    // 异步操作
    await Task.Delay(1000); // 模拟耗时操作
    return "数据已获取";
}
```

### 2.2 await 关键字

`await` 关键字用于暂停异步方法的执行，直到等待的任务完成。它不会阻塞线程，而是将控制权返回给调用者。

```csharp
public async Task ProcessDataAsync()
{
    Console.WriteLine("开始处理数据...");
    
    // await 会暂停方法，但不阻塞线程
    string data = await GetDataAsync();
    
    Console.WriteLine($"获取到数据：{data}");
}
```

## 3. Task 和 Task<T>

`Task` 和 `Task<T>` 是 .NET 中表示异步操作的核心类型。

### 3.1 Task

`Task` 表示一个不返回值的异步操作。

```csharp
public async Task DoWorkAsync()
{
    Console.WriteLine("开始工作...");
    await Task.Delay(2000); // 模拟耗时工作
    Console.WriteLine("工作完成！");
}

// 调用方式
static async Task Main()
{
    await DoWorkAsync();
    // 输出：
    // 开始工作...
    // 工作完成！
}
```

### 3.2 Task<T>

`Task<T>` 表示一个返回类型为 `T` 的异步操作。

```csharp
public async Task<int> CalculateAsync(int x, int y)
{
    await Task.Delay(1000); // 模拟计算
    return x + y;
}

// 调用方式
static async Task Main()
{
    int result = await CalculateAsync(5, 3);
    Console.WriteLine($"计算结果：{result}");
    // 输出：
    // 计算结果：8
}
```

## 4. 异步方法的返回类型

异步方法可以有以下返回类型：

### 4.1 Task（无返回值）

```csharp
public async Task LogMessageAsync(string message)
{
    await Task.Delay(100);
    Console.WriteLine($"日志：{message}");
}
```

### 4.2 Task<T>（有返回值）

```csharp
public async Task<string> ReadFileAsync(string path)
{
    using var reader = new StreamReader(path);
    return await reader.ReadToEndAsync();
}
```

### 4.3 void（仅用于事件处理程序）

```csharp
// 仅用于事件处理程序，不推荐在其他地方使用
private async void Button_Click(object sender, EventArgs e)
{
    await LoadDataAsync();
}
```

**注意**：避免使用 `async void` 方法，除非是事件处理程序。因为 `async void` 方法无法被等待，异常也难以捕获。

### 4.4 ValueTask 和 ValueTask<T>

`ValueTask<T>` 是 C# 7.0 引入的轻量级异步返回类型，用于优化性能敏感场景：

```csharp
// 当结果可能已经可用时使用 ValueTask
public async ValueTask<int> GetValueAsync()
{
    if (_cache.TryGetValue(key, out var value))
    {
        return value; // 同步返回，无堆分配
    }
    
    return await FetchFromDatabaseAsync(); // 异步路径
}
```

**使用场景**：
- 结果可能同步返回（如缓存命中）
- 高频调用的异步方法
- 需要减少堆分配

**注意事项**：
- `ValueTask<T>` 只能被 await 一次
- 不能多次调用 `GetAwaiter().GetResult()`
- 详细用法请参考 [异步编程最佳实践](/basic/async/async-best-practices)

## 5. 异步方法的执行流程

让我们通过一个完整的示例来理解异步方法的执行流程：

```csharp
using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        Console.WriteLine($"[1] 主线程开始，线程ID：{Environment.CurrentManagedThreadId}");
        
        await Step1Async();
        
        Console.WriteLine($"[5] 主线程继续，线程ID：{Environment.CurrentManagedThreadId}");
    }

    static async Task Step1Async()
    {
        Console.WriteLine($"[2] Step1 开始，线程ID：{Environment.CurrentManagedThreadId}");
        
        await Task.Delay(1000); // 模拟耗时操作
        
        Console.WriteLine($"[3] Step1 继续，线程ID：{Environment.CurrentManagedThreadId}");
        
        await Step2Async();
        
        Console.WriteLine($"[4] Step1 完成，线程ID：{Environment.CurrentManagedThreadId}");
    }

    static async Task Step2Async()
    {
        Console.WriteLine($"[3.1] Step2 开始，线程ID：{Environment.CurrentManagedThreadId}");
        
        await Task.Delay(500);
        
        Console.WriteLine($"[3.2] Step2 完成，线程ID：{Environment.CurrentManagedThreadId}");
    }
}
```

**输出：**
```
[1] 主线程开始，线程ID：1
[2] Step1 开始，线程ID：1
[3] Step1 继续，线程ID：4
[3.1] Step2 开始，线程ID：4
[3.2] Step2 完成，线程ID：5
[4] Step1 完成，线程ID：5
[5] 主线程继续，线程ID：5
```

**执行流程说明：**
1. 主线程开始执行
2. 调用 `Step1Async()`，遇到 `await Task.Delay(1000)` 时，方法暂停，控制权返回给 `Main`
3. `Main` 等待 `Step1Async()` 完成，但不阻塞线程
4. 延迟完成后，`Step1Async` 在可能不同的线程上继续执行
5. 最终所有任务完成，`Main` 继续执行

## 6. 异常处理

异步方法中的异常处理与同步方法类似，但有一些重要区别：

```csharp
public async Task RiskyOperationAsync()
{
    try
    {
        await Task.Delay(100);
        throw new InvalidOperationException("模拟异常");
    }
    catch (InvalidOperationException ex)
    {
        Console.WriteLine($"捕获异常：{ex.Message}");
        throw; // 重新抛出异常
    }
}

static async Task Main()
{
    try
    {
        await RiskyOperationAsync();
    }
    catch (InvalidOperationException ex)
    {
        Console.WriteLine($"主方法捕获异常：{ex.Message}");
    }
    // 输出：
    // 捕获异常：模拟异常
    // 主方法捕获异常：模拟异常
}
```

## 7. 取消异步操作

使用 `CancellationToken` 可以取消异步操作：

```csharp
public async Task LongRunningOperationAsync(CancellationToken cancellationToken)
{
    for (int i = 0; i < 10; i++)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        Console.WriteLine($"处理步骤 {i + 1}...");
        await Task.Delay(500, cancellationToken);
    }
}
```

详细用法（链式取消、进度报告等）请参考 [异步编程模式](/basic/async/async-patterns)。

## 8. 同步方法调用异步方法

在同步方法中调用异步方法需要特别注意，避免死锁：

```csharp
// ❌ 错误方式（可能导致死锁）
public void SyncMethod()
{
    var result = AsyncMethod().Result; // 危险！
}

// ✅ 正确方式
public async Task SyncMethodAsync()
{
    var result = await AsyncMethod(); // 安全
}

// ✅ 如果必须同步调用
public void SyncMethodSafe()
{
    var result = Task.Run(() => AsyncMethod()).Result; // 相对安全
}
```

## 9. 总结

异步编程是现代 C# 开发的重要组成部分。通过 `async` 和 `await` 关键字，我们可以编写出既高效又易于维护的异步代码。

**核心要点**：
- 异步方法返回 `Task`、`Task<T>`、`ValueTask<T>` 或 `void`
- `await` 会暂停方法执行，但不阻塞线程
- 异步方法的执行流程是“分段”的
- 正确处理异常和取消操作

**下一步学习**：
- [异步编程模式](/basic/async/async-patterns) - 并行执行、取消、进度报告等
- [异步编程最佳实践](/basic/async/async-best-practices) - 避免死锁、性能优化、测试调试
