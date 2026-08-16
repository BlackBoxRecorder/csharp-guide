---
title: 常用内置类型
description: C# 常用内置类型详解，包括 BigInteger、Complex、Half、Int128 等数值类型，时间类型、Guid、Uri、Range 与 Index、Nullable、Lazy 的用法。
---

除了 [结构体](./struct) 和 [record](./record) 这类可自定义的类型，C# 还内置了一批开箱即用的**值类型与工具类型**。它们覆盖数值扩展、时间处理、标识生成和惰性初始化等高频需求，了解它们可以避免重复造轮子。

### 1. 扩展数值类型

| 类型 | 说明 | 典型场景 |
| --- | --- | --- |
| `BigInteger` | **任意精度整数**，没有上下限 | 大数运算（加密、天文计算） |
| `Complex` | 复数，含实部与虚部 | 数学、信号处理 |
| `Half` | **半精度浮点**（16 位），精度低但省内存 | 机器学习张量、图形计算 |
| `Int128` / `UInt128`（.NET 7+） | 128 位有符号/无符号整数 | 超大数据量统计 |

```csharp
using System.Numerics;

// BigInteger 可以精确计算超出 long 范围的大数
BigInteger huge = BigInteger.Pow(2, 100);
Console.WriteLine(huge);
// 输出：1267650600228229401496703205376

// Complex 直接支持复数运算
var a = new Complex(3, 4);
Console.WriteLine(a.Magnitude); // 输出：5（模长）
```

### 2. 时间类型

| 类型 | 说明 | 与 `DateTime` 的区别 |
| --- | --- | --- |
| `DateTimeOffset` | 带**时区偏移**的时间 | 明确记录 UTC 偏移，适合跨时区 |
| `DateOnly`（.NET 6+） | 只表示**日期** | 不再携带无意义的时分秒 |
| `TimeOnly`（.NET 6+） | 只表示**时间** | 适合营业时间、闹钟等场景 |

```csharp
// DateOnly / TimeOnly 让"只关心日期"的代码更清晰
DateOnly today = DateOnly.FromDateTime(DateTime.Now);
TimeOnly opening = new(9, 0);       // 09:00
TimeOnly closing = new(18, 0);      // 18:00

Console.WriteLine(today);     // 输出：2026-08-16（示例）
Console.WriteLine(opening < closing); // 输出：True

// DateTimeOffset 保留 UTC 偏移，跨时区比较更可靠
var local = new DateTimeOffset(2026, 8, 16, 10, 0, 0, TimeSpan.FromHours(8));
Console.WriteLine(local.UtcDateTime); // 输出：2026/8/16 2:00:00
```

### 3. 标识与资源类型

- **`Guid`**：全局唯一标识符，用于生成几乎不可能重复的 ID。
- **`Uri`**：统一资源标识符，封装了解析、拼接和比较逻辑，比直接操作字符串更安全。
- **`Version`**：版本号（主.次.修订），支持比较运算。

```csharp
// Guid：数据库主键、会话 ID 等
var id = Guid.NewGuid();
Console.WriteLine(id.ToString("N").Length); // 输出：32

// Uri：自动处理转义与相对路径解析
var baseUri = new Uri("https://example.com/api/");
var full = new Uri(baseUri, "users/1");
Console.WriteLine(full.AbsoluteUri); // 输出：https://example.com/api/users/1

// Version：可直接比较大小
var v1 = new Version(2, 1, 0);
var v2 = new Version(2, 1, 5);
Console.WriteLine(v1 < v2); // 输出：True
```

### 4. 语言配套类型

- **`Nullable<T>`（`T?`）**：让值类型可以表示"空"，配合可空引用类型构成 C# 的空安全体系。
- **`Range` 与 `Index`（.NET 6+）**：表达区间与"从末尾数"的索引，配合 `..` 和 `^` 运算符。
- **`Lazy<T>`**：**惰性初始化**包装器，首次访问时才创建值，且默认线程安全。

```csharp
// Range / Index：切片与倒数索引
int[] nums = [10, 20, 30, 40, 50];
Console.WriteLine(nums[^1]);        // 输出：50（倒数第一个）
Console.WriteLine(string.Join(',', nums[1..^1])); // 输出：20,30,40

// Lazy<T>：首次访问时执行工厂，且线程安全
Lazy<List<string>> cache = new(() => new List<string> { "初始化" });
Console.WriteLine(cache.Value[0]);  // 输出：初始化
Console.WriteLine(cache.IsValueCreated); // 输出：True
```

### 5. 参考

- 集合相关的值类型（`Span`、`Tuple` 等）见 [常用数据结构](../collections/basic)。
- 自定义数据模型类型见 [record](./record) 与 [结构体](./struct)。
