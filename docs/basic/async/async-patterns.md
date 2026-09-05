---
title: 异步编程模式
description: C# 异步编程模式，包括并行执行、取消、进度报告、超时、重试、批量处理、容错模式等。
---

# 异步编程模式

在掌握了异步编程的基础之后，我们可以学习一些常用的异步编程模式，这些模式可以帮助我们更好地处理复杂的异步场景。

> **前置阅读**：[异步编程基础](/basic/async/async-basics) - async/await、Task、ValueTask 核心概念

## 1. 并行执行

并行执行允许多个异步操作同时运行，从而提高整体执行效率。

### 1.1 Task.WhenAll

`Task.WhenAll` 等待所有任务完成：

```csharp
public static async Task WhenAllExample()
{
    var task1 = SimulateWorkAsync("任务1", 2000);
    var task2 = SimulateWorkAsync("任务2", 1500);
    var task3 = SimulateWorkAsync("任务3", 1000);

    Console.WriteLine("所有任务开始...");
    
    // 并行执行所有任务
    await Task.WhenAll(task1, task2, task3);
    
    Console.WriteLine("所有任务完成！");
}

public static async Task SimulateWorkAsync(string name, int delay)
{
    Console.WriteLine($"{name} 开始，延迟 {delay}ms");
    await Task.Delay(delay);
    Console.WriteLine($"{name} 完成");
}

static async Task Main()
{
    await WhenAllExample();
    // 输出：
    // 所有任务开始...
    // 任务1 开始，延迟 2000ms
    // 任务2 开始，延迟 1500ms
    // 任务3 开始，延迟 1000ms
    // 任务3 完成
    // 任务2 完成
    // 任务1 完成
    // 所有任务完成！
}
```

### 1.2 Task.WhenAny

`Task.WhenAny` 等待任意一个任务完成：

```csharp
public static async Task WhenAnyExample()
{
    var task1 = SimulateWorkAsync("快速任务", 500);
    var task2 = SimulateWorkAsync("慢速任务", 2000);

    Console.WriteLine("等待第一个完成的任务...");
    
    // 等待第一个完成的任务
    Task completedTask = await Task.WhenAny(task1, task2);
    
    if (completedTask == task1)
        Console.WriteLine("快速任务先完成！");
    else
        Console.WriteLine("慢速任务先完成！");
    
    // 等待所有任务完成（避免资源泄漏）
    await Task.WhenAll(task1, task2);
}

static async Task Main()
{
    await WhenAnyExample();
    // 输出：
    // 等待第一个完成的任务...
    // 快速任务 开始，延迟 500ms
    // 慢速任务 开始，延迟 2000ms
    // 快速任务 完成
    // 快速任务先完成！
    // 慢速任务 完成
}
```

## 2. 取消操作

使用 `CancellationToken` 可以优雅地取消异步操作。

### 2.1 基本取消

```csharp
public static async Task BasicCancellationExample()
{
    using var cts = new CancellationTokenSource();
    
    // 启动任务
    var task = LongRunningTaskAsync(cts.Token);
    
    // 2秒后取消
    await Task.Delay(2000);
    cts.Cancel();
    
    try
    {
        await task;
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine("任务已被取消");
    }
}

public static async Task LongRunningTaskAsync(CancellationToken cancellationToken)
{
    for (int i = 0; i < 10; i++)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        Console.WriteLine($"处理步骤 {i + 1}...");
        await Task.Delay(500, cancellationToken);
    }
}

static async Task Main()
{
    await BasicCancellationExample();
    // 输出：
    // 处理步骤 1...
    // 处理步骤 2...
    // 处理步骤 3...
    // 处理步骤 4...
    // 任务已被取消
}
```

### 2.2 链式取消

```csharp
public static async Task LinkedCancellationExample()
{
    using var cts1 = new CancellationTokenSource();
    using var cts2 = new CancellationTokenSource();
    
    // 创建链接的取消令牌
    using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts1.Token, cts2.Token);
    
    var task = LongRunningTaskAsync(linkedCts.Token);
    
    // 取消任意一个源都会取消任务
    await Task.Delay(1500);
    cts1.Cancel(); // 这会取消链接的令牌
    
    try
    {
        await task;
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine("任务已被取消（通过 cts1）");
    }
}

static async Task Main()
{
    await LinkedCancellationExample();
    // 输出：
    // 处理步骤 1...
    // 处理步骤 2...
    // 处理步骤 3...
    // 任务已被取消（通过 cts1）
}
```

## 3. 进度报告

使用 `IProgress<T>` 接口可以报告异步操作的进度。

### 3.1 基本进度报告

```csharp
public static async Task ProgressReportingExample()
{
    var progress = new Progress<int>(percent =>
    {
        Console.Write($"\r进度：{percent}%");
    });
    
    await ProcessWithProgressAsync(progress);
    
    Console.WriteLine("\n处理完成！");
}

public static async Task ProcessWithProgressAsync(IProgress<int> progress)
{
    for (int i = 0; i <= 100; i += 10)
    {
        await Task.Delay(200); // 模拟工作
        progress.Report(i);
    }
}

static async Task Main()
{
    await ProgressReportingExample();
    // 输出：
    // 进度：0%
    // 进度：10%
    // ...
    // 进度：100%
    // 处理完成！
}
```

### 3.2 带取消的进度报告

```csharp
public static async Task ProgressWithCancellationExample()
{
    using var cts = new CancellationTokenSource();
    var progress = new Progress<int>(percent =>
    {
        Console.Write($"\r进度：{percent}%");
    });
    
    // 3秒后取消
    cts.CancelAfter(TimeSpan.FromSeconds(3));
    
    try
    {
        await ProcessWithProgressAndCancellationAsync(progress, cts.Token);
        Console.WriteLine("\n处理完成！");
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine("\n处理已被取消");
    }
}

public static async Task ProcessWithProgressAndCancellationAsync(
    IProgress<int> progress, 
    CancellationToken cancellationToken)
{
    for (int i = 0; i <= 100; i += 10)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        await Task.Delay(500, cancellationToken); // 模拟工作
        progress.Report(i);
    }
}

static async Task Main()
{
    await ProgressWithCancellationExample();
    // 输出：
    // 进度：0%
    // 进度：10%
    // 进度：20%
    // 进度：30%
    // 进度：40%
    // 处理已被取消
}
```

## 4. 超时处理

使用 `CancellationTokenSource` 可以设置操作的超时时间。

```csharp
public static async Task TimeoutExample()
{
    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
    
    try
    {
        await LongRunningTaskAsync(cts.Token);
        Console.WriteLine("任务完成！");
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine("任务超时！");
    }
}

public static async Task LongRunningTaskAsync(CancellationToken cancellationToken)
{
    Console.WriteLine("开始长时间任务...");
    await Task.Delay(5000, cancellationToken); // 5秒任务，2秒超时
    Console.WriteLine("任务完成");
}

static async Task Main()
{
    await TimeoutExample();
    // 输出：
    // 开始长时间任务...
    // 任务超时！
}
```

## 5. 重试模式

实现自动重试逻辑，处理暂时性故障。

```csharp
public static async Task RetryExample()
{
    int maxRetries = 3;
    int retryCount = 0;
    
    while (true)
    {
        try
        {
            retryCount++;
            Console.WriteLine($"尝试第 {retryCount} 次...");
            
            await UnreliableOperationAsync();
            
            Console.WriteLine("操作成功！");
            break;
        }
        catch (Exception ex) when (retryCount < maxRetries)
        {
            Console.WriteLine($"操作失败：{ex.Message}，{2 * retryCount}秒后重试...");
            await Task.Delay(2000 * retryCount); // 指数退避
        }
    }
}

public static async Task UnreliableOperationAsync()
{
    await Task.Delay(100);
    
    // 模拟随机失败
    if (new Random().Next(0, 3) != 0)
    {
        throw new InvalidOperationException("模拟暂时性故障");
    }
}

static async Task Main()
{
    await RetryExample();
    // 输出（可能）：
    // 尝试第 1 次...
    // 操作失败：模拟暂时性故障，2秒后重试...
    // 尝试第 2 次...
    // 操作成功！
}
```

## 6. 批量处理

处理大量数据时，可以使用批量处理来避免资源耗尽。

```csharp
public static async Task BatchProcessingExample()
{
    var items = Enumerable.Range(1, 100).ToList();
    int batchSize = 10;
    
    for (int i = 0; i < items.Count; i += batchSize)
    {
        var batch = items.Skip(i).Take(batchSize).ToList();
        
        Console.WriteLine($"处理批次 {i / batchSize + 1}，包含 {batch.Count} 个项目");
        
        // 并行处理批次中的项目
        var tasks = batch.Select(item => ProcessItemAsync(item));
        await Task.WhenAll(tasks);
        
        // 批次间延迟，避免过载
        if (i + batchSize < items.Count)
        {
            await Task.Delay(1000);
        }
    }
    
    Console.WriteLine("所有批次处理完成！");
}

public static async Task ProcessItemAsync(int item)
{
    await Task.Delay(100); // 模拟处理
    if (item % 20 == 0)
    {
        Console.WriteLine($"  处理项目 {item} 完成");
    }
}

static async Task Main()
{
    await BatchProcessingExample();
    // 输出：
    // 处理批次 1，包含 10 个项目
    //   处理项目 20 完成
    // 处理批次 2，包含 10 个项目
    //   处理项目 40 完成
    // ...
    // 所有批次处理完成！
}
```

## 7. 容错模式

### 7.1 熔断模式（Circuit Breaker）

当服务持续失败时，暂时停止调用，避免资源浪费：

```csharp
public class CircuitBreaker
{
    private enum State { Closed, Open, HalfOpen }
    
    private State _state = State.Closed;
    private int _failureCount = 0;
    private DateTime _lastFailureTime;
    private readonly int _failureThreshold = 5;
    private readonly TimeSpan _timeout = TimeSpan.FromSeconds(30);
    
    public async Task<T> ExecuteAsync<T>(Func<Task<T>> action)
    {
        if (_state == State.Open)
        {
            if (DateTime.UtcNow - _lastFailureTime > _timeout)
            {
                _state = State.HalfOpen;
            }
            else
            {
                throw new InvalidOperationException("熔断器已打开，拒绝请求");
            }
        }
        
        try
        {
            var result = await action();
            
            if (_state == State.HalfOpen)
            {
                _state = State.Closed;
                _failureCount = 0;
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _failureCount++;
            _lastFailureTime = DateTime.UtcNow;
            
            if (_failureCount >= _failureThreshold)
            {
                _state = State.Open;
            }
            
            throw;
        }
    }
}
```

### 7.2 降级模式（Fallback）

当主服务失败时，使用备用方案：

```csharp
public async Task<string> GetDataWithFallbackAsync()
{
    try
    {
        return await PrimaryServiceAsync();
    }
    catch (Exception ex)
    {
        _logger.LogWarning(ex, "主服务失败，使用备用服务");
        return await FallbackServiceAsync();
    }
}
```

### 7.3 组合模式

将重试、熔断、降级组合使用：

```csharp
public async Task<string> GetDataWithResilienceAsync()
{
    return await _circuitBreaker.ExecuteAsync(async () =>
    {
        return await RetryAsync(async () =>
        {
            try
            {
                return await PrimaryServiceAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "主服务失败，使用备用服务");
                return await FallbackServiceAsync();
            }
        });
    });
}
```

## 8. 总结

本章介绍了 C# 异步编程中的常用模式：

1. **并行执行**：使用 `Task.WhenAll` 和 `Task.WhenAny` 同时执行多个任务
2. **取消操作**：使用 `CancellationToken` 优雅地取消异步操作
3. **进度报告**：使用 `IProgress<T>` 报告操作进度
4. **超时处理**：设置操作超时时间
5. **重试模式**：实现自动重试逻辑
6. **批量处理**：分批处理大量数据
7. **容错模式**：熔断、降级、组合使用

**高级主题**（异步队列、异步锁等）请参考 [异步编程高级主题](/basic/async/async-advanced)。

**下一步学习**：
- [异步编程最佳实践](/basic/async/async-best-practices) - 避免死锁、性能优化、测试调试
- [异步编程高级主题](/basic/async/async-advanced) - Channel、异步锁、自定义调度器
