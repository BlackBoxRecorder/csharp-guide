---
title: 异步编程高级主题
description: C# 异步编程高级主题，包括异步流、Channel、异步锁、自定义任务调度器、并发控制等。
---

# 异步编程高级主题

在掌握了异步编程的基础和最佳实践之后，我们可以探索一些高级主题，这些主题可以帮助你处理更复杂的异步场景。

> **前置阅读**：[异步编程最佳实践](/basic/async/async-best-practices) - 避免死锁、性能优化、测试调试

## 1. 异步流 (IAsyncEnumerable)

C# 8.0 引入了异步流，允许你异步地枚举一系列值。

### 1.1 基本用法

```csharp
public static async IAsyncEnumerable<int> GenerateNumbersAsync()
{
    for (int i = 0; i < 10; i++)
    {
        await Task.Delay(100); // 模拟异步操作
        yield return i;
    }
}

public static async Task ConsumeAsyncStream()
{
    await foreach (var number in GenerateNumbersAsync())
    {
        Console.WriteLine($"收到数字：{number}");
    }
}

static async Task Main()
{
    await ConsumeAsyncStream();
    // 输出：
    // 收到数字：0
    // 收到数字：1
    // ...
    // 收到数字：9
}
```

### 1.2 带取消的异步流

```csharp
public static async IAsyncEnumerable<int> GenerateNumbersAsync(
    [EnumeratorCancellation] CancellationToken cancellationToken = default)
{
    for (int i = 0; i < 100; i++)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        await Task.Delay(100, cancellationToken);
        yield return i;
    }
}

public static async Task ConsumeWithCancellation()
{
    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
    
    try
    {
        await foreach (var number in GenerateNumbersAsync(cts.Token))
        {
            Console.WriteLine($"收到数字：{number}");
        }
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine("流已被取消");
    }
}

static async Task Main()
{
    await ConsumeWithCancellation();
    // 输出：
    // 收到数字：0
    // 收到数字：1
    // ...
    // 收到数字：9
    // 流已被取消
}
```

### 1.3 异步流的转换

```csharp
public static async Task TransformStream()
{
    var numbers = GenerateNumbersAsync();
    
    // 使用 LINQ 操作（需要 System.Linq.Async）
    var evenNumbers = numbers.Where(n => n % 2 == 0);
    var doubled = evenNumbers.Select(n => n * 2);
    
    await foreach (var number in doubled)
    {
        Console.WriteLine($"偶数翻倍：{number}");
    }
}

// 使用 System.Linq.Async NuGet 包
// Install-Package System.Linq.Async
```

### 1.4 异步流的合并

```csharp
public static async IAsyncEnumerable<int> MergeStreamsAsync(
    IAsyncEnumerable<int> stream1,
    IAsyncEnumerable<int> stream2)
{
    var enumerator1 = stream1.GetAsyncEnumerator();
    var enumerator2 = stream2.GetAsyncEnumerator();
    
    try
    {
        bool hasMore1 = true;
        bool hasMore2 = true;
        
        while (hasMore1 || hasMore2)
        {
            if (hasMore1)
            {
                if (await enumerator1.MoveNextAsync())
                {
                    yield return enumerator1.Current;
                }
                else
                {
                    hasMore1 = false;
                }
            }
            
            if (hasMore2)
            {
                if (await enumerator2.MoveNextAsync())
                {
                    yield return enumerator2.Current;
                }
                else
                {
                    hasMore2 = false;
                }
            }
        }
    }
    finally
    {
        await enumerator1.DisposeAsync();
        await enumerator2.DisposeAsync();
    }
}
```

## 2. Channel 高级用法

`Channel` 是 .NET 中用于生产者-消费者模式的强大工具。

### 2.1 有界 Channel

```csharp
public static async Task BoundedChannelExample()
{
    var options = new BoundedChannelOptions(10)
    {
        FullMode = BoundedChannelFullMode.Wait, // 等待空间
        SingleReader = false,
        SingleWriter = false
    };
    
    var channel = Channel.CreateBounded<int>(options);
    
    // 生产者
    var producer = Task.Run(async () =>
    {
        for (int i = 0; i < 20; i++)
        {
            await channel.Writer.WriteAsync(i);
            Console.WriteLine($"生产：{i}，队列大小：{channel.Reader.Count}");
            await Task.Delay(100);
        }
        channel.Writer.Complete();
    });
    
    // 消费者
    var consumer = Task.Run(async () =>
    {
        await foreach (var item in channel.Reader.ReadAllAsync())
        {
            Console.WriteLine($"消费：{item}");
            await Task.Delay(300); // 消费比生产慢
        }
    });
    
    await Task.WhenAll(producer, consumer);
    Console.WriteLine("Channel 处理完成！");
}

static async Task Main()
{
    await BoundedChannelExample();
    // 输出：
    // 生产：0，队列大小：1
    // 消费：0
    // 生产：1，队列大小：1
    // ...
    // Channel 处理完成！
}
```

### 2.2 多消费者

```csharp
public static async Task MultipleConsumersExample()
{
    var channel = Channel.CreateUnbounded<string>();
    
    // 多个生产者
    var producers = Enumerable.Range(0, 3).Select(id => Task.Run(async () =>
    {
        for (int i = 0; i < 5; i++)
        {
            var message = $"生产者{id}-消息{i}";
            await channel.Writer.WriteAsync(message);
            Console.WriteLine($"生产：{message}");
            await Task.Delay(200);
        }
    })).ToArray();
    
    // 多个消费者
    var consumers = Enumerable.Range(0, 2).Select(id => Task.Run(async () =>
    {
        await foreach (var message in channel.Reader.ReadAllAsync())
        {
            Console.WriteLine($"消费者{id} 收到：{message}");
            await Task.Delay(300);
        }
    })).ToArray();
    
    // 等待所有生产者完成
    await Task.WhenAll(producers);
    channel.Writer.Complete();
    
    // 等待所有消费者完成
    await Task.WhenAll(consumers);
    
    Console.WriteLine("多消费者示例完成！");
}

static async Task Main()
{
    await MultipleConsumersExample();
    // 输出：
    // 生产：生产者0-消息0
    // 生产：生产者1-消息0
    // 生产：生产者2-消息0
    // 消费者0 收到：生产者0-消息0
    // ...
    // 多消费者示例完成！
}
```

### 2.3 Channel 与异步流结合

```csharp
public static async Task ChannelAsStreamExample()
{
    var channel = Channel.CreateUnbounded<int>();
    
    // 将 Channel 作为异步流
    var stream = channel.Reader.ReadAllAsync();
    
    // 生产者
    var producer = Task.Run(async () =>
    {
        for (int i = 0; i < 10; i++)
        {
            await channel.Writer.WriteAsync(i);
            await Task.Delay(100);
        }
        channel.Writer.Complete();
    });
    
    // 消费者（使用异步流）
    var consumer = Task.Run(async () =>
    {
        await foreach (var item in stream)
        {
            Console.WriteLine($"从流中读取：{item}");
        }
    });
    
    await Task.WhenAll(producer, consumer);
    Console.WriteLine("Channel 作为流示例完成！");
}

static async Task Main()
{
    await ChannelAsStreamExample();
    // 输出：
    // 从流中读取：0
    // 从流中读取：1
    // ...
    // Channel 作为流示例完成！
}
```

## 3. 异步锁

### 3.1 SemaphoreSlim

```csharp
public class AsyncSemaphoreExample
{
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(3); // 最多3个并发
    private readonly List<string> _results = new();
    
    public async Task ProcessWithSemaphoreAsync()
    {
        var tasks = Enumerable.Range(1, 10).Select(async i =>
        {
            await _semaphore.WaitAsync();
            try
            {
                Console.WriteLine($"任务 {i} 开始，当前并发数：{3 - _semaphore.CurrentCount}");
                await Task.Delay(1000);
                
                lock (_results)
                {
                    _results.Add($"任务{i}完成");
                }
                
                Console.WriteLine($"任务 {i} 完成");
            }
            finally
            {
                _semaphore.Release();
            }
        });
        
        await Task.WhenAll(tasks);
        
        Console.WriteLine($"所有任务完成，结果数量：{_results.Count}");
    }
}

static async Task Main()
{
    var example = new AsyncSemaphoreExample();
    await example.ProcessWithSemaphoreAsync();
    // 输出：
    // 任务 1 开始，当前并发数：1
    // 任务 2 开始，当前并发数：2
    // 任务 3 开始，当前并发数：3
    // 任务 1 完成
    // 任务 4 开始，当前并发数：3
    // ...
    // 所有任务完成，结果数量：10
}
```

### 3.2 AsyncLock

实现一个简单的异步锁：

```csharp
public class AsyncLock
{
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
    private readonly Task<IDisposable> _releaser;
    
    public AsyncLock()
    {
        _releaser = Task.FromResult<IDisposable>(new Releaser(this));
    }
    
    public Task<IDisposable> LockAsync()
    {
        var waitTask = _semaphore.WaitAsync();
        
        return waitTask.IsCompleted
            ? _releaser
            : waitTask.ContinueWith(
                (_, state) => (IDisposable)new Releaser((AsyncLock)state!),
                this,
                CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);
    }
    
    private sealed class Releaser : IDisposable
    {
        private readonly AsyncLock _lock;
        
        internal Releaser(AsyncLock @lock) => _lock = @lock;
        
        public void Dispose() => _lock._semaphore.Release();
    }
}

public static async Task AsyncLockExample()
{
    var asyncLock = new AsyncLock();
    var sharedResource = 0;
    
    var tasks = Enumerable.Range(1, 5).Select(async i =>
    {
        using (await asyncLock.LockAsync())
        {
            Console.WriteLine($"任务 {i} 获得锁");
            sharedResource++;
            await Task.Delay(500);
            Console.WriteLine($"任务 {i} 释放锁，资源值：{sharedResource}");
        }
    });
    
    await Task.WhenAll(tasks);
    Console.WriteLine($"最终资源值：{sharedResource}");
}

static async Task Main()
{
    await AsyncLockExample();
    // 输出：
    // 任务 1 获得锁
    // 任务 1 释放锁，资源值：1
    // 任务 2 获得锁
    // 任务 2 释放锁，资源值：2
    // ...
    // 最终资源值：5
}
```

### 3.3 ReaderWriterLockSlim 的异步版本

```csharp
public class AsyncReaderWriterLock
{
    private readonly SemaphoreSlim _readLock = new SemaphoreSlim(int.MaxValue, int.MaxValue);
    private readonly SemaphoreSlim _writeLock = new SemaphoreSlim(1, 1);
    private int _readerCount = 0;
    
    public async Task<IDisposable> ReaderLockAsync()
    {
        await _readLock.WaitAsync();
        
        Interlocked.Increment(ref _readerCount);
        
        if (_readerCount == 1)
        {
            await _writeLock.WaitAsync();
        }
        
        _readLock.Release();
        
        return new Releaser(() =>
        {
            Interlocked.Decrement(ref _readerCount);
            
            if (_readerCount == 0)
            {
                _writeLock.Release();
            }
        });
    }
    
    public async Task<IDisposable> WriterLockAsync()
    {
        await _writeLock.WaitAsync();
        
        return new Releaser(() => _writeLock.Release());
    }
    
    private sealed class Releaser : IDisposable
    {
        private readonly Action _release;
        
        public Releaser(Action release) => _release = release;
        
        public void Dispose() => _release();
    }
}
```

## 4. 任务调度器

### 4.1 内置任务调度器

.NET 提供了几种内置的任务调度器：

| 调度器 | 说明 | 使用场景 |
|--------|------|----------|
| `TaskScheduler.Default` | 线程池调度器（默认） | 大多数异步操作 |
| `TaskScheduler.Current` | 当前同步上下文调度器 | UI 线程回调 |
| `TaskScheduler.FromCurrentSynchronizationContext()` | 从当前同步上下文创建 | UI 线程更新 |

**使用示例**：

```csharp
// 使用默认调度器（线程池）
await Task.Factory.StartNew(() => DoWork(), CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);

// 在 UI 线程上执行
await Task.Factory.StartNew(() => UpdateUI(), CancellationToken.None, TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
```

### 4.2 自定义任务调度器

```csharp
public class LimitedConcurrencyTaskScheduler : TaskScheduler
{
    private readonly int _maxConcurrency;
    private readonly LinkedList<Task> _tasks = new();
    private int _runningTasks;
    
    public LimitedConcurrencyTaskScheduler(int maxConcurrency)
    {
        _maxConcurrency = maxConcurrency;
    }
    
    protected override IEnumerable<Task> GetScheduledTasks()
    {
        lock (_tasks)
        {
            return _tasks.ToArray();
        }
    }
    
    protected override void QueueTask(Task task)
    {
        lock (_tasks)
        {
            _tasks.AddLast(task);
            
            if (_runningTasks < _maxConcurrency)
            {
                _runningTasks++;
                ThreadPool.QueueUserWorkItem(_ => TryExecuteNext());
            }
        }
    }
    
    private void TryExecuteNext()
    {
        while (true)
        {
            Task task;
            lock (_tasks)
            {
                if (_tasks.Count == 0)
                {
                    _runningTasks--;
                    return;
                }
                
                task = _tasks.First!.Value;
                _tasks.RemoveFirst();
            }
            
            TryExecuteTask(task);
        }
    }
    
    protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
    {
        return false;
    }
}

public static async Task CustomSchedulerExample()
{
    var scheduler = new LimitedConcurrencyTaskScheduler(2);
    var factory = new TaskFactory(scheduler);
    
    var tasks = Enumerable.Range(1, 5).Select(i =>
        factory.StartNew(async () =>
        {
            Console.WriteLine($"任务 {i} 开始，线程：{Environment.CurrentManagedThreadId}");
            await Task.Delay(1000);
            Console.WriteLine($"任务 {i} 完成");
        }).Unwrap());
    
    await Task.WhenAll(tasks);
    Console.WriteLine("自定义调度器示例完成！");
}

static async Task Main()
{
    await CustomSchedulerExample();
    // 输出：
    // 任务 1 开始，线程：4
    // 任务 2 开始，线程：5
    // 任务 1 完成
    // 任务 3 开始，线程：4
    // 任务 2 完成
    // 任务 4 开始，线程：5
    // ...
    // 自定义调度器示例完成！
}
```

### 4.2 单线程调度器

```csharp
public class SingleThreadTaskScheduler : TaskScheduler
{
    private readonly BlockingCollection<Task> _tasks = new();
    private readonly Thread _thread;
    
    public SingleThreadTaskScheduler()
    {
        _thread = new Thread(Run)
        {
            IsBackground = true,
            Name = "SingleThreadTaskScheduler"
        };
        _thread.Start();
    }
    
    private void Run()
    {
        foreach (var task in _tasks.GetConsumingEnumerable())
        {
            TryExecuteTask(task);
        }
    }
    
    protected override IEnumerable<Task> GetScheduledTasks()
    {
        return _tasks.ToArray();
    }
    
    protected override void QueueTask(Task task)
    {
        _tasks.Add(task);
    }
    
    protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
    {
        return Thread.CurrentThread == _thread && TryExecuteTask(task);
    }
    
    public void Complete() => _tasks.CompleteAdding();
}

public static async Task SingleThreadSchedulerExample()
{
    var scheduler = new SingleThreadTaskScheduler();
    var factory = new TaskFactory(scheduler);
    
    var tasks = Enumerable.Range(1, 5).Select(i =>
        factory.StartNew(() =>
        {
            Console.WriteLine($"任务 {i} 在线程 {Environment.CurrentManagedThreadId} 上执行");
            return i * i;
        }));
    
    var results = await Task.WhenAll(tasks);
    
    scheduler.Complete();
    
    Console.WriteLine($"结果：{string.Join(", ", results)}");
}

static async Task Main()
{
    await SingleThreadSchedulerExample();
    // 输出：
    // 任务 1 在线程 4 上执行
    // 任务 2 在线程 4 上执行
    // 任务 3 在线程 4 上执行
    // 任务 4 在线程 4 上执行
    // 任务 5 在线程 4 上执行
    // 结果：1, 4, 9, 16, 25
}
```

## 5. 异步事件聚合器

### 5.1 类型安全的事件聚合器

```csharp
public class AsyncEventAggregator
{
    private readonly Dictionary<Type, List<Delegate>> _handlers = new();
    
    public void Subscribe<TEvent>(Func<TEvent, Task> handler)
    {
        var eventType = typeof(TEvent);
        
        if (!_handlers.ContainsKey(eventType))
        {
            _handlers[eventType] = new List<Delegate>();
        }
        
        _handlers[eventType].Add(handler);
    }
    
    public async Task PublishAsync<TEvent>(TEvent eventData)
    {
        var eventType = typeof(TEvent);
        
        if (_handlers.TryGetValue(eventType, out var handlers))
        {
            var tasks = handlers
                .Cast<Func<TEvent, Task>>()
                .Select(handler => handler(eventData));
            
            await Task.WhenAll(tasks);
        }
    }
}

public class DataLoadedEvent
{
    public string Data { get; set; } = string.Empty;
}

public class ErrorEvent
{
    public string Message { get; set; } = string.Empty;
    public Exception? Exception { get; set; }
}

public static async Task TypeSafeEventAggregatorExample()
{
    var aggregator = new AsyncEventAggregator();
    
    // 订阅事件
    aggregator.Subscribe<DataLoadedEvent>(async evt =>
    {
        Console.WriteLine($"处理器1 收到数据：{evt.Data}");
        await Task.Delay(100);
    });
    
    aggregator.Subscribe<DataLoadedEvent>(async evt =>
    {
        Console.WriteLine($"处理器2 收到数据：{evt.Data}");
        await Task.Delay(200);
    });
    
    aggregator.Subscribe<ErrorEvent>(async evt =>
    {
        Console.WriteLine($"错误处理器：{evt.Message}");
        await Task.Delay(50);
    });
    
    // 发布事件
    await aggregator.PublishAsync(new DataLoadedEvent { Data = "测试数据" });
    await aggregator.PublishAsync(new ErrorEvent { Message = "测试错误" });
    
    Console.WriteLine("类型安全事件聚合器示例完成！");
}

static async Task Main()
{
    await TypeSafeEventAggregatorExample();
    // 输出：
    // 处理器1 收到数据：测试数据
    // 处理器2 收到数据：测试数据
    // 错误处理器：测试错误
    // 类型安全事件聚合器示例完成！
}
```

## 6. 异步管道

实现异步管道模式，处理数据流：

```csharp
public class AsyncPipeline<T>
{
    private readonly List<Func<T, Task<T>>> _stages = new();
    
    public AsyncPipeline<T> AddStage(Func<T, Task<T>> stage)
    {
        _stages.Add(stage);
        return this;
    }
    
    public async Task<T> ExecuteAsync(T input)
    {
        var current = input;
        
        foreach (var stage in _stages)
        {
            current = await stage(current);
        }
        
        return current;
    }
}

public static async Task AsyncPipelineExample()
{
    var pipeline = new AsyncPipeline<string>()
        .AddStage(async input =>
        {
            Console.WriteLine($"阶段1：处理 {input}");
            await Task.Delay(100);
            return input.ToUpper();
        })
        .AddStage(async input =>
        {
            Console.WriteLine($"阶段2：处理 {input}");
            await Task.Delay(100);
            return $"[{input}]";
        })
        .AddStage(async input =>
        {
            Console.WriteLine($"阶段3：处理 {input}");
            await Task.Delay(100);
            return $"{input} - 已处理";
        });
    
    var result = await pipeline.ExecuteAsync("hello");
    
    Console.WriteLine($"最终结果：{result}");
}

static async Task Main()
{
    await AsyncPipelineExample();
    // 输出：
    // 阶段1：处理 hello
    // 阶段2：处理 HELLO
    // 阶段3：处理 [HELLO]
    // 最终结果：[HELLO] - 已处理
}
```

## 7. 异步缓存

实现异步缓存，避免重复计算：

```csharp
public class AsyncCache<TKey, TValue> where TKey : notnull
{
    private readonly ConcurrentDictionary<TKey, Lazy<Task<TValue>>> _cache = new();
    private readonly Func<TKey, Task<TValue>> _valueFactory;
    
    public AsyncCache(Func<TKey, Task<TValue>> valueFactory)
    {
        _valueFactory = valueFactory;
    }
    
    public Task<TValue> GetOrAddAsync(TKey key)
    {
        return _cache.GetOrAdd(key, k => new Lazy<Task<TValue>>(() => _valueFactory(k))).Value;
    }
    
    public bool TryRemove(TKey key)
    {
        return _cache.TryRemove(key, out _);
    }
    
    public void Clear() => _cache.Clear();
}

public static async Task AsyncCacheExample()
{
    var cache = new AsyncCache<int, string>(async key =>
    {
        Console.WriteLine($"计算 {key} 的值...");
        await Task.Delay(1000); // 模拟耗时计算
        return $"值_{key}";
    });
    
    // 第一次访问，会计算
    var value1 = await cache.GetOrAddAsync(1);
    Console.WriteLine($"第一次访问：{value1}");
    
    // 第二次访问，从缓存获取
    var value2 = await cache.GetOrAddAsync(1);
    Console.WriteLine($"第二次访问：{value2}");
    
    // 不同的键
    var value3 = await cache.GetOrAddAsync(2);
    Console.WriteLine($"不同键：{value3}");
}

static async Task Main()
{
    await AsyncCacheExample();
    // 输出：
    // 计算 1 的值...
    // 第一次访问：值_1
    // 第二次访问：值_1
    // 计算 2 的值...
    // 不同键：值_2
}
```

## 8. 总结

本章介绍了 C# 异步编程的高级主题：

1. **异步流 (IAsyncEnumerable)**：
   - 异步枚举一系列值
   - 支持取消和 LINQ 操作
   - 适用于流式数据处理

2. **Channel 高级用法**：
   - 有界和无界 Channel
   - 多消费者模式
   - 与异步流结合

3. **异步锁**：
   - SemaphoreSlim
   - AsyncLock
   - ReaderWriterLockSlim 的异步版本

4. **自定义任务调度器**：
   - 有限并发度调度器
   - 单线程调度器

5. **异步事件聚合器**：
   - 类型安全的事件订阅和发布

6. **异步管道**：
   - 处理数据流的管道模式

7. **异步缓存**：
   - 避免重复计算的缓存机制

8. **并发控制**：
   - 限流（Rate Limiting）
   - 背压（Backpressure）

这些高级主题可以帮助你构建更复杂、更高效的异步应用程序。记住，异步编程的核心是正确处理并发、取消和异常，同时保持代码的可读性和可维护性。

## 并发控制

### 限流（Rate Limiting）

限制单位时间内的请求数量：

```csharp
public class RateLimiter
{
    private readonly SemaphoreSlim _semaphore;
    private readonly TimeSpan _interval;
    private readonly Queue<DateTime> _timestamps = new();
    
    public RateLimiter(int maxRequests, TimeSpan interval)
    {
        _semaphore = new SemaphoreSlim(maxRequests, maxRequests);
        _interval = interval;
    }
    
    public async Task<T> ExecuteAsync<T>(Func<Task<T>> action)
    {
        await _semaphore.WaitAsync();
        try
        {
            return await action();
        }
        finally
        {
            await Task.Delay(_interval);
            _semaphore.Release();
        }
    }
}
```

### 背压（Backpressure）

当生产者速度超过消费者速度时，使用有界 Channel 实现背压：

```csharp
public static async Task BackpressureExample()
{
    // 有界 Channel，容量为 10
    var channel = Channel.CreateBounded<int>(new BoundedChannelOptions(10)
    {
        FullMode = BoundedChannelFullMode.Wait // 生产者等待
    });
    
    // 生产者
    var producer = Task.Run(async () =>
    {
        for (int i = 0; i < 100; i++)
        {
            await channel.Writer.WriteAsync(i); // 会自动等待
            Console.WriteLine($"生产：{i}");
        }
        channel.Writer.Complete();
    });
    
    // 消费者（慢速）
    var consumer = Task.Run(async () =>
    {
        await foreach (var item in channel.Reader.ReadAllAsync())
        {
            await Task.Delay(500); // 消费慢
            Console.WriteLine($"消费：{item}");
        }
    });
    
    await Task.WhenAll(producer, consumer);
}
```
