---
title: Lazy<T> 懒加载
description: C# Lazy<T> 懒加载详解，包括基本用法、线程安全模式、异常缓存行为与典型应用场景。
---

有些对象的创建代价很高（比如加载配置文件、建立数据库连接、解析大文件），但并非每次启动都会用到。**懒加载（Lazy Loading）** 把初始化推迟到第一次真正使用时才执行，避免无谓的开销。`Lazy<T>` 是 .NET 内置的懒加载容器，除了"延迟"之外，它还额外解决了一个手写懒加载容易踩的坑：**多线程下如何保证只初始化一次**。

### 基本用法

`Lazy<T>` 的核心是 `Value` 属性：第一次访问时执行初始化并缓存结果，之后的访问直接返回缓存值。

```csharp
Lazy<ExpensiveObject> lazy = new Lazy<ExpensiveObject>();

Console.WriteLine($"创建后是否已初始化：{lazy.IsValueCreated}"); // 输出：创建后是否已初始化：False

var obj = lazy.Value; // 第一次访问，此刻才真正执行初始化
Console.WriteLine($"首次访问后是否已初始化：{lazy.IsValueCreated}"); // 输出：首次访问后是否已初始化：True

var same = lazy.Value; // 后续访问直接返回同一个实例
Console.WriteLine(ReferenceEquals(obj, same)); // 输出：True
```

`IsValueCreated` 用来判断初始化是否已经发生，可用于调试和日志。

带参数的初始化可以通过 `valueFactory` 委托传入：

```csharp
Lazy<ExpensiveObject> lazy = new Lazy<ExpensiveObject>(() =>
    new ExpensiveObject(configFile: "app.json"));
```

`valueFactory` 只在第一次访问 `Value` 时执行一次，并且返回值会被缓存，所以无需担心重复执行。

### 线程安全模式

手写懒加载最常见的问题是多线程：两个线程同时发现"还没初始化"，各自初始化一份，得到两个不同的实例。`Lazy<T>` 通过 `LazyThreadSafetyMode` 解决了这个问题，共有三种模式：

| 模式 | 是否线程安全 | 初始化次数 | 说明 |
| :--- | :--- | :--- | :--- |
| `ExecutionAndPublication`（默认） | ✅ | 仅一次 | 所有线程阻塞等待，只执行一次初始化，且异常会被缓存 |
| `PublicationOnly` | ✅ | 可能多次 | 允许并发初始化，但只发布第一个成功的结果 |
| `None` | ❌ | 不保证 | 完全不提供同步，仅限单线程场景 |

```csharp
// 默认模式：线程安全，多个线程同时访问也只初始化一次
var safe = new Lazy<ExpensiveObject>(
    () => new ExpensiveObject(),
    LazyThreadSafetyMode.ExecutionAndPublication);

// 允许重复计算，但只发布第一个成功的结果
var publishOnly = new Lazy<ExpensiveObject>(
    () => new ExpensiveObject(),
    LazyThreadSafetyMode.PublicationOnly);

// 不提供任何线程安全保证，仅限单线程使用
var singleThread = new Lazy<ExpensiveObject>(
    () => new ExpensiveObject(),
    LazyThreadSafetyMode.None);
```

什么时候选 `PublicationOnly`？它的代价是"可能重复执行初始化"，收益是**初始化失败后其他线程可以继续重试**（见下文异常缓存），适合初始化失败可恢复的场景。而默认的 `ExecutionAndPublication` 适合大多数情况：宁可让其他线程阻塞，也要保证只算一次。

> 提示：`None` 模式几乎省掉了所有同步开销，但一旦多线程同时访问，行为未定义（可能初始化多次），只在明确单线程的环境中使用。

### 异常缓存行为

一个容易忽略的细节：**默认模式下，初始化抛出的异常也会被缓存**。也就是说，第一次访问失败后，后续每次访问都会重新抛出同一个异常，而不会重新尝试初始化。

```csharp
var lazy = new Lazy<int>(() => throw new InvalidOperationException("初始化失败"));

for (int i = 1; i <= 2; i++)
{
    try
    {
        _ = lazy.Value;
    }
    catch (InvalidOperationException)
    {
        Console.WriteLine($"第 {i} 次访问：抛出异常（初始化未重试）");
    }
}
// 输出：
// 第 1 次访问：抛出异常（初始化未重试）
// 第 2 次访问：抛出异常（初始化未重试）
```

如果初始化可能暂时失败、希望下次访问时重试，需要换用 `PublicationOnly` 模式（异常不会被缓存），或者干脆重建 `Lazy<T>` 实例。

### 典型使用场景

**线程安全的单例**。`Lazy<T>` 是实现线程安全单例的简洁方式，无需手写锁：

```csharp
public sealed class ConfigManager
{
    private static readonly Lazy<ConfigManager> _instance =
        new(() => new ConfigManager());

    public static ConfigManager Instance => _instance.Value;

    private ConfigManager()
    {
        // 加载配置文件
    }
}
```

**昂贵的资源延迟创建**。比如数据库连接、正则表达式编译、大文件解析等，只有真正用到时才初始化：

```csharp
class ReportService
{
    private readonly Lazy<byte[]> _reportTemplate = new(LoadTemplate);

    public byte[] Render()
    {
        var template = _reportTemplate.Value; // 首次调用 Render 时才加载模板
        return template;
    }

    private static byte[] LoadTemplate() => File.ReadAllBytes("template.xlsx");
}
```

### 注意事项

1. 初始化中递归访问 `Value`：在 `valueFactory` 内部访问同一个 `Lazy<T>` 的 `Value`，默认模式下会抛出 `InvalidOperationException`（死锁保护）。
2. 懒加载不是万能的：初始化代价很低的对象直接创建即可，`Lazy<T>` 本身有委托调用和同步检查的开销，滥用反而更慢。
3. 异常缓存：默认模式下初始化失败后无法通过再次访问重试，需要时换 `PublicationOnly` 或重建实例。
4. 单线程场景优先用 `None` 模式，避免无谓的同步开销。

### 延伸阅读

- `LazyInitializer`：静态辅助类，同样实现懒加载但避免闭包分配，适合高频调用、追求极致性能的场景。
- 异步懒加载：`Lazy<T>` 本身不支持异步初始化，需要 `AsyncLazy` 可自行封装（基于 `Task<T>` 或引用第三方库实现）。
