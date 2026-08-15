---
title: 迭代器
description: C# 迭代器（Iterator）详解，包括 yield return/yield break 的使用、延迟执行原理、状态机机制等。
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
关于 IEnumerable和IEnumerator的区别查看[这里](/dotnet/basic/IEnumerable-and-IEnumerator)
:::

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
class Program
{
    static void Main()
    {
        DaysOfTheWeek week = new DaysOfTheWeek();
        foreach (string day in week)
        {
            Console.Write(day + " ");
        }
        // 输出: Sun Mon Tue Wed Thr Fri Sat
    }
}

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

```cs
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

```cs
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

```cs
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

```cs
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
