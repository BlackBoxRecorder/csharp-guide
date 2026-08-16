---
title: 迭代器
description: C# 迭代器（Iterator）详解，包括 yield return/yield break 的使用、迭代器方法、延迟执行原理、状态机机制、异步迭代器（IAsyncEnumerable）等。
---


C# 中的迭代器（Iterator）允许你轻松地遍历集合或生成数据序列，而无需一次性将所有数据加载到内存中。

> 核心关键字：yield return 和 yield break。

yield return ：返回序列中的下一个元素，并暂停方法的执行，保存当前位置。当下一次请求元素时，方法会从暂停处继续执行。

yield break;：立即终止迭代，相当于告诉调用方“没有更多元素了”。

**迭代器的返回类型必须是以下四种之一：**

```csharp
System.Collections.IEnumerable
System.Collections.IEnumerator
System.Collections.Generic.IEnumerable<T>
System.Collections.Generic.IEnumerator<T>
```

最常见的方式是实现一个返回 IEnumerable<T> 的方法。

:::note
关于 IEnumerable和IEnumerator的区别查看[这里](./IEnumerable-and-IEnumerator)
:::

## 什么是迭代器方法

**方法体内包含 `yield` 关键字的方法就是迭代器方法**——它通常返回 `IEnumerable<T>`，把"逐个产生结果"的逻辑交给编译器处理：

- 调用迭代器方法时，方法体**不会立即执行**，而是返回一个编译器生成的状态机对象
- 每次遍历（`foreach` 或 `MoveNext()`）请求一个元素，状态机从上次 `yield return` 暂停的位置继续执行
- 局部变量在暂停期间被保留，下次从断点恢复——这就是"暂停-恢复"机制

普通方法"一口气跑完"，迭代器方法则"跑一段、停一下、再跑一段"，直到 `yield break` 或方法结束。

## 使用示例

示例 1，一个可遍历的类型，实现接口 IEnumerable：

```csharp
public class DaysOfTheWeek : IEnumerable
{
    private string[] days = { "Sun", "Mon", "Tue", "Wed", "Thr", "Fri", "Sat" };

    // 实现 GetEnumerator 方法，使其成为一个迭代器
    public IEnumerator GetEnumerator()
    {
        for (int i = 0; i < days.Length; i++)
        {
            yield return days[i];
        }
    }
}

// 使用方式
DaysOfTheWeek week = new();
foreach (string day in week)
{
    Console.Write(day + " ");
}
// 输出：Sun Mon Tue Wed Thr Fri Sat

```

示例 2，一个可遍历的类型，实现接口 IEnumerable<T>

```csharp
public class Warehouse : IEnumerable<string>
{
    private string[] _items = { "CPU", "GPU", "RAM" };

    public IEnumerator<string> GetEnumerator()
    {
        for (int i = 0; i < _items.Length; i++)
        {
            yield return _items[i]; // 状态自动保存
        }
    }
}

// 使用
Warehouse warehouse = new();
foreach(var item in warehouse) 
{
 Console.WriteLine(item); // 输出：CPU / GPU / RAM
}
```

### 延迟执行

这是迭代器最重要的特性。迭代器方法中的代码只有在遍历时才会执行。当你调用一个返回 IEnumerable<T> 的迭代器方法时，它并不会立即运行方法体，只有当 foreach 循环（或手动调用 MoveNext()）请求下一个元素时，代码才会执行到下一个 yield return 语句。

**优势：**

- 节省内存：可以处理非常大的甚至无限的数据序列，而无需一次性将所有数据加载到内存中。
- 提升性能：如果只需要序列中的一部分数据，迭代器只会计算到那一步为止。

```csharp
// 这个迭代器可以生成无限序列
public static IEnumerable<int> InfiniteSequence()
{
    int i = 0;
    while (true)
    {
        yield return i++;
    }
}

// 使用 .Take(5) 只会触发前5次计算，不会导致无限循环
foreach (var num in InfiniteSequence().Take(5))
{
    Console.WriteLine(num); // 输出 0, 1, 2, 3, 4
}
```

按行读取大文件

```csharp
public IEnumerable<string> ReadLines(string filePath)
{
    using (var reader = new StreamReader(filePath))
    {
        string line;
        while ((line = reader.ReadLine()) != null)
        {
            yield return line; // 文件会在遍历结束后自动关闭
        }
    }
}
```

数据库分页查询

```csharp
IEnumerable<User> FetchUsers(int pageSize)
{
    int pageIndex = 0;
    while(true)
    {
        var users = dbContext.Users
                   .Skip(pageIndex * pageSize)
                   .Take(pageSize)
                   .ToList();
                   
        if(!users.Any()) yield break;
        
        foreach(var user in users)
            yield return user;
            
        pageIndex++;
    }
}

// 使用（每次取5条）
foreach(var user in FetchUsers(5))
{
    ProcessUser(user); // 内存高效的分页处理
}
```

实时数据流处理

```csharp
IEnumerable<StockPrice> GetRealTimePrices(string symbol)
{
    while(true)
    {
        var price = StockApi.FetchLatestPrice(symbol);
        yield return price;
        Thread.Sleep(1000); // 每秒获取最新价格
    }
}

// 使用（无限序列）
foreach(var price in GetRealTimePrices("MSFT"))
{
    UpdateDashboard(price); // 持续更新仪表盘
}
```

复杂算法生成

```csharp
IEnumerable<int> Fibonacci()
{
    int a = 0, b = 1;
    while(true)
    {
        yield return a;
        (a, b) = (b, a + b); // 元组解构
    }
}

// 获取前10个斐波那契数
foreach(var num in Fibonacci().Take(10))
{
    Console.Write(num + " "); // 0 1 1 2 3 5 8 13 21 34
}
```

目录深度遍历

```csharp
IEnumerable<string> TraverseDirectories(string path)
{
    yield return path;
    
    foreach(var dir in Directory.GetDirectories(path))
    {
        foreach(var subDir in TraverseDirectories(dir)) // 递归迭代
            yield return subDir;
    }
}

// 使用
foreach(var dir in TraverseDirectories(@"C:\Projects"))
{
    Console.WriteLine(dir); // 深度优先扫描
}
```

## 异步迭代器（C# 8+）

**异步迭代器**用于每个元素都需要异步等待的场景（分页查库、读网络流）：方法用 `async` 修饰、返回 `IAsyncEnumerable<T>`，内部用 `yield return` 逐个产生结果，消费端用 `await foreach` 遍历。

```csharp
// 异步迭代器：await 后逐个 yield 结果
public static async IAsyncEnumerable<int> GenerateAsync(int count)
{
    for (int i = 0; i < count; i++)
    {
        await Task.Delay(100); // 模拟异步 IO，期间不阻塞线程
        yield return i;
    }
}

// 消费端：await foreach 逐个取结果
await foreach (var num in GenerateAsync(3))
{
    Console.WriteLine(num); // 输出：0 / 1 / 2
}
```

**与同步迭代器的对比**：

| 特性 | 同步迭代器 | 异步迭代器 |
| :--- | :--- | :--- |
| 返回类型 | `IEnumerable<T>` / `IEnumerator<T>` | `IAsyncEnumerable<T>` / `IAsyncEnumerator<T>` |
| 方法声明 | 普通方法 | `async` 修饰 |
| 元素生产 | 同步计算 | 可 `await` 异步 IO |
| 消费方式 | `foreach` | `await foreach` |
| 引入版本 | C# 2.0 | C# 8.0 |

**要点**：

1. `await foreach` 遇到 `await` 时释放线程，数据到达后恢复执行——异步迭代器天然适合长时间运行的数据流（如实时消息推送、日志流）
2. `IAsyncEnumerable<T>` 与 `IEnumerable<T>` 互不兼容，LINQ 运算符需要 [`System.Linq.Async`](https://www.nuget.org/packages/System.Linq.Async) 包
3. 取消支持：方法增加 `[EnumeratorCancellation] CancellationToken` 参数（`System.Runtime.CompilerServices` 命名空间），调用方用 `WithCancellation(token)` 传入
4. `yield` 只能出现在迭代器方法中：普通 `async Task` 方法不能用 `yield return`，异步迭代器也不能用 `ref`/`out` 参数

## 参考链接

- [迭代器（C# 编程指南）](https://learn.microsoft.com/zh-cn/dotnet/csharp/programming-guide/concepts/iterators)
- [yield 关键字（C# 参考）](https://learn.microsoft.com/zh-cn/dotnet/csharp/language-reference/statements/yield)
- [IAsyncEnumerable<T> 接口（.NET API）](https://learn.microsoft.com/zh-cn/dotnet/api/system.collections.generic.iasyncenumerable-1)
```
