---
title: 异步编程最佳实践
description: C# 异步编程最佳实践，包括避免死锁、异常处理、性能优化、测试调试等。
---

# 异步编程最佳实践

异步编程虽然强大，但也容易陷入一些常见的陷阱。本章将介绍异步编程的最佳实践，帮助你编写更安全、更高效的异步代码。

> **前置阅读**：[异步编程模式](/basic/async/async-patterns) - 并行执行、取消、进度报告等

## 1. 避免死锁

死锁是异步编程中最常见的问题之一，特别是在同步代码调用异步方法时。

### 1.1 死锁的典型场景

```csharp
// ❌ 危险：在 UI 线程或 ASP.NET 同步上下文中调用
public void DangerousMethod()
{
    // 这会导致死锁！
    var result = GetDataAsync().Result;
}

public async Task<string> GetDataAsync()
{
    await Task.Delay(1000);
    return "数据";
}
```

**死锁原因：**
1. UI 线程调用 `GetDataAsync().Result`，阻塞等待任务完成
2. `GetDataAsync` 中的 `await` 尝试回到 UI 线程（同步上下文）
3. UI 线程被阻塞，无法处理回调
4. 形成死锁：UI 线程等待任务，任务等待 UI 线程

### 1.2 解决方案

**方案一：全程使用 async/await**

```csharp
// ✅ 正确：使用 async/await
public async Task SafeMethodAsync()
{
    var result = await GetDataAsync();
    Console.WriteLine(result);
}
```

**方案二：使用 ConfigureAwait(false)**

```csharp
// ✅ 在库代码中使用 ConfigureAwait(false)
public async Task<string> GetDataAsync()
{
    await Task.Delay(1000).ConfigureAwait(false);
    return "数据";
}

// 现在可以安全地同步调用（但不推荐）
public void SafeSyncMethod()
{
    var result = GetDataAsync().Result; // 不会死锁
}
```

**方案三：使用 Task.Run**

```csharp
// ✅ 在同步方法中调用异步方法
public void SafeSyncMethod()
{
    var result = Task.Run(() => GetDataAsync()).Result;
}
```

### 1.3 ConfigureAwait 详解

`ConfigureAwait(false)` 告诉运行时不需要回到原始同步上下文。

```csharp
public async Task LibraryMethodAsync()
{
    // 不需要回到调用者的同步上下文
    await SomeOperationAsync().ConfigureAwait(false);
    
    // 这里的代码可能在任意线程上执行
    ProcessData();
}

public async Task UIMethodAsync()
{
    // 需要回到 UI 线程
    await SomeOperationAsync(); // 不使用 ConfigureAwait
    
    // 这里的代码会在 UI 线程上执行
    UpdateUI();
}
```

**使用指南：**
- **库代码**：总是使用 `ConfigureAwait(false)`
- **UI 代码**：避免使用 `ConfigureAwait(false)`
- **ASP.NET Core**：不需要使用 `ConfigureAwait(false)`（没有同步上下文）

### 1.4 SynchronizationContext 详解

`SynchronizationContext` 是 .NET 中用于线程同步的机制，它决定了 `await` 之后代码在哪个线程上执行。

**不同平台的 SynchronizationContext**：

| 平台 | SynchronizationContext | 行为 |
|------|------------------------|------|
| UI (WPF/WinForms) | `DispatcherSynchronizationContext` | `await` 后回到 UI 线程 |
| ASP.NET (经典) | `AspNetSynchronizationContext` | `await` 后回到请求线程 |
| 控制台/ASP.NET Core | `null` | `await` 后在任意线程池线程 |

**为什么 UI 平台会死锁**：

```csharp
// UI 线程调用
var result = GetDataAsync().Result; // 阻塞 UI 线程

// GetDataAsync 内部
await Task.Delay(1000); // 尝试回到 UI 线程
// 但 UI 线程被阻塞 → 死锁
```

**ConfigureAwait(false) 的作用**：

```csharp
await Task.Delay(1000).ConfigureAwait(false);
// 告诉运行时：不要回到原始同步上下文
// 后续代码在任意线程池线程执行
```

## 2. 异常处理

异步方法中的异常处理需要特别注意。

### 2.1 基本异常处理

```csharp
public async Task<string> ReadFileAsync(string path)
{
    try
    {
        using var reader = new StreamReader(path);
        return await reader.ReadToEndAsync();
    }
    catch (FileNotFoundException ex)
    {
        Console.WriteLine($"文件未找到：{ex.FileName}");
        throw; // 重新抛出异常
    }
    catch (IOException ex)
    {
        Console.WriteLine($"IO 错误：{ex.Message}");
        throw;
    }
}
```

### 2.2 处理 AggregateException

当使用 `Task.WhenAll` 时，可能会产生多个异常：

```csharp
public async Task HandleMultipleExceptionsAsync()
{
    var task1 = SimulateErrorAsync("任务1");
    var task2 = SimulateErrorAsync("任务2");
    var task3 = SimulateSuccessAsync("任务3");
    
    try
    {
        await Task.WhenAll(task1, task2, task3);
    }
    catch (Exception ex)
    {
        // 只能捕获第一个异常
        Console.WriteLine($"捕获异常：{ex.Message}");
    }
    
    // 获取所有异常
    var tasks = new[] { task1, task2, task3 };
    var exceptions = tasks
        .Where(t => t.IsFaulted)
        .SelectMany(t => t.Exception!.InnerExceptions)
        .ToList();
    
    foreach (var ex in exceptions)
    {
        Console.WriteLine($"所有异常：{ex.Message}");
    }
}

public async Task SimulateErrorAsync(string name)
{
    await Task.Delay(100);
    throw new InvalidOperationException($"{name} 失败");
}

public async Task SimulateSuccessAsync(string name)
{
    await Task.Delay(100);
    Console.WriteLine($"{name} 成功");
}

static async Task Main()
{
    await HandleMultipleExceptionsAsync();
    // 输出：
    // 任务3 成功
    // 捕获异常：任务1 失败
    // 所有异常：任务1 失败
    // 所有异常：任务2 失败
}
```

### 2.3 异常过滤器

使用异常过滤器可以更精细地控制异常处理：

```csharp
public async Task RetryWithFilterAsync()
{
    int retryCount = 0;
    
    while (true)
    {
        try
        {
            retryCount++;
            await UnreliableOperationAsync();
            break;
        }
        catch (HttpRequestException ex) when (retryCount < 3)
        {
            Console.WriteLine($"请求失败，重试 {retryCount}：{ex.Message}");
            await Task.Delay(1000 * retryCount);
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"请求最终失败：{ex.Message}");
            throw;
        }
    }
}
```

### 2.4 使用 ExceptionDispatchInfo

保留原始异常堆栈跟踪：

```csharp
using System.Runtime.ExceptionServices;

public async Task<string> CaptureExceptionAsync()
{
    try
    {
        await RiskyOperationAsync();
        return "成功";
    }
    catch (Exception ex)
    {
        // 捕获并保存异常信息
        ExceptionDispatchInfo.Capture(ex).Throw();
        throw; // 永远不会执行
    }
}

// 在其他地方重新抛出，保留原始堆栈
public async Task ProcessAsync()
{
    try
    {
        var result = await CaptureExceptionAsync();
    }
    catch (Exception ex)
    {
        // 堆栈跟踪指向原始抛出位置
        Console.WriteLine(ex.StackTrace);
    }
}
```

## 3. 避免常见陷阱

### 3.1 避免 async void

```csharp
// ❌ 危险：async void
public async void DangerousVoidMethod()
{
    await Task.Delay(1000);
    throw new InvalidOperationException("这个异常无法被捕获");
}

// ✅ 正确：async Task
public async Task SafeTaskMethod()
{
    await Task.Delay(1000);
    throw new InvalidOperationException("这个异常可以被捕获");
}
```

**async void 的问题：**
1. 无法被等待
2. 异常无法被捕获
3. 无法知道方法何时完成

**例外：事件处理程序**

```csharp
// ✅ 事件处理程序可以使用 async void
private async void Button_Click(object sender, EventArgs e)
{
    try
    {
        await LoadDataAsync();
    }
    catch (Exception ex)
    {
        MessageBox.Show($"错误：{ex.Message}");
    }
}
```

### 3.2 避免阻塞调用

```csharp
// ❌ 危险：阻塞调用
public void BlockingCalls()
{
    var result = GetDataAsync().Result; // 可能死锁
    GetDataAsync().Wait(); // 可能死锁
    GetDataAsync().GetAwaiter().GetResult(); // 可能死锁
}

// ✅ 正确：使用 async/await
public async Task NonBlockingCallsAsync()
{
    var result = await GetDataAsync();
}
```

### 3.3 避免在循环中创建任务

```csharp
// ❌ 低效：在循环中创建任务
public async Task InefficientLoopAsync()
{
    for (int i = 0; i < 100; i++)
    {
        await ProcessItemAsync(i); // 串行执行
    }
}

// ✅ 高效：并行执行
public async Task EfficientLoopAsync()
{
    var tasks = Enumerable.Range(0, 100)
        .Select(i => ProcessItemAsync(i));
    
    await Task.WhenAll(tasks); // 并行执行
}
```

### 3.4 正确处理取消

```csharp
// ❌ 不检查取消
public async Task BadCancellationAsync(CancellationToken token)
{
    await Task.Delay(5000); // 忽略取消令牌
}

// ✅ 正确检查取消
public async Task GoodCancellationAsync(CancellationToken token)
{
    token.ThrowIfCancellationRequested();
    
    await Task.Delay(5000, token); // 传递取消令牌
    
    token.ThrowIfCancellationRequested();
}
```

## 4. 性能优化

### 4.1 使用 ValueTask

`ValueTask` 可以减少堆分配：

```csharp
// ✅ 当结果可能已经可用时使用 ValueTask
public async ValueTask<int> GetValueAsync()
{
    if (_cache.TryGetValue(key, out var value))
    {
        return value; // 同步返回，无堆分配
    }
    
    return await FetchFromDatabaseAsync(); // 异步路径
}
```

### 4.2 避免不必要的状态机

```csharp
// ❌ 不必要的 async/await
public async Task<string> UnnecessaryAsync()
{
    return await GetStringAsync(); // 多余的 await
}

// ✅ 直接返回任务
public Task<string> DirectReturn()
{
    return GetStringAsync(); // 避免状态机开销
}
```

### 4.3 使用 ConfigureAwait(false)

```csharp
// ✅ 在库代码中使用 ConfigureAwait(false)
public async Task<string> LibraryMethodAsync()
{
    var data = await FetchDataAsync().ConfigureAwait(false);
    return ProcessData(data);
}
```

### 4.4 对象池化

```csharp
// ✅ 使用对象池减少分配
public class BufferPool
{
    private readonly ConcurrentBag<byte[]> _pool = new();
    
    public byte[] Rent(int size)
    {
        return _pool.TryTake(out var buffer) && buffer.Length >= size
            ? buffer
            : new byte[size];
    }
    
    public void Return(byte[] buffer)
    {
        if (buffer.Length <= 1024 * 1024) // 最大 1MB
        {
            _pool.Add(buffer);
        }
    }
}
```

### 4.5 异步编程的内存开销

异步编程会带来额外的内存开销：

| 开销来源 | 说明 | 优化建议 |
|----------|------|----------|
| 状态机 | 每个 `async` 方法生成一个状态机对象 | 避免不必要的 `async/await` |
| Task 对象 | 每个异步操作创建一个 Task 实例 | 使用 `ValueTask` 优化同步路径 |
| 捕获的变量 | `await` 之前的局部变量被捕获到堆上 | 减少 `await` 前的变量 |

**状态机开销示例**：

```csharp
// ❌ 不必要的状态机
public async Task<int> AddAsync(int a, int b)
{
    return a + b; // 没有 await，但生成了状态机
}

// ✅ 直接返回
public Task<int> AddAsync(int a, int b)
{
    return Task.FromResult(a + b); // 无状态机开销
}
```

## 5. 测试异步代码

### 5.1 单元测试

```csharp
[Test]
public async Task GetDataAsync_ShouldReturnData()
{
    // Arrange
    var service = new DataService();
    
    // Act
    var result = await service.GetDataAsync();
    
    // Assert
    Assert.IsNotNull(result);
    Assert.AreEqual("expected", result);
}

[Test]
public async Task GetDataAsync_ShouldThrowOnInvalidInput()
{
    // Arrange
    var service = new DataService();
    
    // Act & Assert
    await Assert.ThrowsAsync<ArgumentException>(() => 
        service.GetDataAsync(""));
}
```

### 5.2 测试取消

```csharp
[Test]
public async Task LongRunningTask_ShouldBeCancellable()
{
    // Arrange
    using var cts = new CancellationTokenSource();
    cts.CancelAfter(TimeSpan.FromSeconds(1));
    
    // Act & Assert
    await Assert.ThrowsAsync<OperationCanceledException>(() =>
        LongRunningTaskAsync(cts.Token));
}
```

### 5.3 测试超时

```csharp
[Test]
public async Task GetDataAsync_ShouldTimeout()
{
    // Arrange
    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
    
    // Act & Assert
    await Assert.ThrowsAsync<OperationCanceledException>(() =>
        GetDataAsync(cts.Token));
}
```

## 6. 调试异步代码

### 6.1 使用调试器

1. **设置断点**：在 `await` 表达式上设置断点
2. **查看任务状态**：使用"任务"窗口查看所有任务
3. **查看调用堆栈**：异步调用堆栈可能不连续

### 6.2 日志记录

```csharp
public async Task<string> GetDataAsync()
{
    var sw = Stopwatch.StartNew();
    
    _logger.LogInformation("开始获取数据");
    
    try
    {
        var result = await FetchDataAsync();
        
        _logger.LogInformation("数据获取完成，耗时 {Elapsed}ms", sw.ElapsedMilliseconds);
        
        return result;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "数据获取失败，耗时 {Elapsed}ms", sw.ElapsedMilliseconds);
        throw;
    }
}
```

### 6.3 使用 Activity 和 DiagnosticSource

```csharp
public class AsyncDiagnostics
{
    private static readonly ActivitySource _activitySource = new("MyApp.Async");
    
    public async Task<string> GetDataAsync()
    {
        using var activity = _activitySource.StartActivity("GetData");
        
        activity?.SetTag("operation", "fetch");
        
        try
        {
            var result = await FetchDataAsync();
            
            activity?.SetTag("result.success", true);
            
            return result;
        }
        catch (Exception ex)
        {
            activity?.SetTag("result.success", false);
            activity?.SetTag("error.message", ex.Message);
            
            throw;
        }
    }
}
```

### 6.4 异步调用堆栈解读

异步调用堆栈与同步调用堆栈不同，需要特别理解：

**同步调用堆栈**：
```
MethodA() → MethodB() → MethodC()
```

**异步调用堆栈**：
```
MethodA() → await → [状态机] → MethodB() → await → [状态机] → MethodC()
```

**调试技巧**：
1. 使用"任务"窗口查看所有待完成的任务
2. 使用"并行堆栈"窗口查看异步调用链
3. 在异常设置中勾选"中断时展开内部异常"

### 6.5 死锁诊断

当程序卡住不动时，可能是死锁：

```csharp
// 死锁诊断步骤：
// 1. 暂停调试器
// 2. 查看"线程"窗口，找到阻塞的线程
// 3. 查看调用堆栈，找到 .Result 或 .Wait() 调用
// 4. 检查是否有 SynchronizationContext
```

**常见死锁模式**：
- UI 线程调用 `.Result` 或 `.Wait()`
- 同步方法调用异步方法
- 嵌套的 `Task.Run` 中使用 `.Result`

## 7. 总结

本章介绍了异步编程的最佳实践：

1. **避免死锁**：
   - 全程使用 async/await
   - 在库代码中使用 ConfigureAwait(false)
   - 避免在同步上下文中调用异步方法

2. **异常处理**：
   - 正确捕获和处理异常
   - 处理 AggregateException
   - 使用异常过滤器

3. **避免常见陷阱**：
   - 避免 async void
   - 避免阻塞调用
   - 正确处理取消

4. **性能优化**：
   - 使用 ValueTask
   - 避免不必要的状态机
   - 使用对象池化

5. **测试和调试**：
   - 编写异步单元测试
   - 测试取消和超时
   - 使用日志和诊断工具
   - 理解异步调用堆栈
   - 诊断死锁问题

**下一步学习**：
- [异步编程高级主题](/basic/async/async-advanced) - 异步流、Channel、异步锁、自定义调度器
