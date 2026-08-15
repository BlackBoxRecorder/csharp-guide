---
title: C# 8.0 新特性
description: C# 8.0 版本引入的所有新特性详细说明，包含可空引用类型、异步流、默认接口方法等特性的代码示例和最佳实践
---

# C# 8.0 新特性

C# 8.0 于2019年9月发布，是第一个专门面向.NET Core的C#主要版本。该版本引入了多项重要的语言特性，旨在提升代码的可读性、可维护性和性能，标志着C#向更现代化、更安全的编程范式迈进的重要一步。

## 主要特性总览

- **可空引用类型**：在编译时检查空引用，消除空引用异常
- **异步流**：支持异步创建和使用数据流，简化I/O密集型操作
- **默认接口方法**：允许在接口中提供方法实现，支持API向后兼容演进
- **模式匹配增强**：新增switch表达式、属性模式、元组模式等
- **`using` 声明**：更简洁的资源管理方式，自动释放IDisposable对象
- **静态本地函数**：防止本地函数捕获外部变量，提升代码安全性
- **可处置的 `ref` 结构**：允许ref结构实现IDisposable，支持栈上资源管理
- **索引和范围**：提供`^`和`..`运算符，简化序列的切片操作
- **空合并赋值**：新增`??=`运算符，简化空值检查和赋值操作
- **非托管构造类型**：支持泛型结构体作为非托管类型使用
- **嵌套表达式中的 `stackalloc`**：允许在更多场景下使用栈内存分配
- **插值逐字字符串增强**：支持`@$""`和`$@""`两种写法

### 1. 可空引用类型 (Nullable Reference Types)

可空引用类型是C# 8.0引入的核心特性，旨在帮助开发者消除空引用异常。在启用可空注释上下文的项目中，引用类型默认不可为空，如果可能为null需要使用`?`显式声明：

```csharp
string notNull = "hello"; // 默认不可为空
string? mayBeNull = null; // 显式声明可为空
```

编译器通过流分析确保不可为空引用在使用前已被初始化，并且在访问可为空引用前进行空检查，从而在编译时捕获潜在的空引用错误，大幅提升代码健壮性。

### 2. 异步流 (Asynchronous Streams)

异步流允许以异步方式创建和使用数据流，核心是`IAsyncEnumerable<T>`接口，特别适合处理I/O密集型操作：

```csharp
// 生成异步流
async IAsyncEnumerable<int> GenerateNumbersAsync()
{
    for (int i = 0; i < 10; i++)
    {
        await Task.Delay(100);
        yield return i;
    }
}

// 消费异步流
await foreach (var number in GenerateNumbersAsync())
{
    Console.WriteLine(number);
}
```

异步流避免了传统异步编程中的回调地狱，使得处理异步产生的大量数据更加高效自然。

### 3. 默认接口方法 (Default Interface Methods)

默认接口方法允许在接口中为方法提供具体实现，这对于API的演进非常有用，可以向现有接口添加新方法而不破坏现有实现：

```csharp
public interface ILogger
{
    void Log(string message);
    
    // 新增方法，提供默认实现
    void LogWarning(string message) => Log($"WARNING: {message}");
}

// 现有实现类不需要修改即可使用新方法
public class ConsoleLogger : ILogger
{
    public void Log(string message) => Console.WriteLine(message);
}
```

这个特性解决了接口版本演进的向后兼容性问题，使得API设计更加灵活。

### 4. 模式匹配增强 (Enhanced Pattern Matching)

C# 8.0大幅增强了模式匹配能力，引入了多种新的模式类型：

#### Switch表达式

```csharp
var result = shape switch
{
    Rectangle r => $"矩形，宽：{r.Width}，高：{r.Height}",
    Circle c => $"圆形，半径：{c.Radius}",
    _ => "未知形状"
};
```

#### 属性模式

```csharp
if (person is { Age: > 18, Name: string name })
{
    Console.WriteLine($"成年人：{name}");
}
```

#### 元组模式

```csharp
var (x, y) = (5, 10);
var quadrant = (x, y) switch
{
    (> 0, > 0) => "第一象限",
    (< 0, > 0) => "第二象限",
    (< 0, < 0) => "第三象限",
    (> 0, < 0) => "第四象限",
    _ => "坐标轴"
};
```

这些增强使得代码更简洁、更具表达力，减少了冗长的if-else语句链。

### 5. `using` 声明 (`using` Declarations)

`using`声明提供了更简洁的资源管理方式，无需嵌套代码块，编译器会自动在作用域结束时调用Dispose方法：

```csharp
// 旧写法
using (var file = new StreamWriter("file.txt"))
{
    file.WriteLine("Hello, World!");
}

// 新写法
using var file = new StreamWriter("file.txt");
file.WriteLine("Hello, World!");
// file会在作用域结束时自动释放
```

这个特性减少了代码嵌套层级，提升了可读性，特别适合处理文件流、数据库连接等需要及时释放的资源。

### 6. 静态本地函数 (Static Local Functions)

允许将`static`修饰符应用于本地函数，确保其不会捕获封闭作用域中的变量：

```csharp
public void OuterMethod()
{
    int outerVariable = 10;
    
    static void StaticLocalFunction(int param)
    {
        // Console.WriteLine(outerVariable); // 编译错误：不能访问外部变量
        Console.WriteLine(param);
    }
    
    StaticLocalFunction(20);
}
```

静态本地函数防止意外捕获外部变量导致的内存泄漏，让代码行为更易于理解和推理。

### 7. 可处置的 `ref` 结构 (Disposable ref structs)

允许`ref`结构类型实现`IDisposable`接口，支持在`using`语句中使用：

```csharp
ref struct Buffer
{
    public void Dispose()
    {
        // 释放资源
    }
}

// 使用
using var buffer = new Buffer();
// 使用buffer...
// 作用域结束时自动调用Dispose
```

`ref`结构始终分配在栈上，这个特性增强了其在高性能场景下的实用性，方便进行精细的资源管理。

### 8. 索引和范围 (Indices and Ranges)

引入`^`（从末尾索引）和`..`（范围运算符），简化序列的切片操作：

```csharp
int[] numbers = { 0, 1, 2, 3, 4, 5 };

// 索引
Console.WriteLine(numbers[^1]); // 输出最后一个元素：5
Console.WriteLine(numbers[^2]); // 输出倒数第二个元素：4

// 范围
var subArray = numbers[1..4]; // 获取索引1到3的元素：[1,2,3]
var firstHalf = numbers[..3]; // 前3个元素：[0,1,2]
var secondHalf = numbers[3..]; // 从索引3到末尾：[3,4,5]
var all = numbers[..]; // 整个数组
```

索引和范围支持数组、字符串、Span<T>等所有序列类型，极大地简化了切片操作。

### 9. 空合并赋值 (Null-coalescing Assignment)

新增`??=`运算符，仅在变量为null时才进行赋值：

```csharp
string name = null;
name ??= "Default Name";
Console.WriteLine(name); // 输出: Default Name

name = "John Doe";
name ??= "Another Name";
Console.WriteLine(name); // 输出: John Doe (赋值未发生)
```

这个运算符简化了空值检查和默认值设置，避免了不必要的if语句。

### 10. 非托管构造类型 (Unmanaged Constructed Types)

允许泛型结构体在所有字段均为非托管类型时，作为非托管类型使用：

```csharp
public struct Point<T> where T : unmanaged
{
    public T X;
    public T Y;
}

// Point<int>现在是非托管类型，可以用于stackalloc等场景
Span<Point<int>> points = stackalloc Point<int>[10];
```

这个特性扩展了非托管类型的适用范围，在泛型上下文中进行低级内存操作更加灵活。

### 11. 嵌套表达式中的 `stackalloc`

允许在更多表达式场景中使用`stackalloc`关键字，包括嵌套表达式：

```csharp
// 可以在条件表达式中使用
Span<byte> buffer = condition ? stackalloc byte[128] : stackalloc byte[256];
```

`stackalloc`在栈上分配内存，无需GC回收，适合高性能场景下的临时缓冲区使用。需要注意避免分配过大的内存导致栈溢出。

### 12. 插值逐字字符串增强

支持`@$""`和`$@""`两种写法，顺序可以互换：

```csharp
string name = "Alice";
string path1 = $@"C:\Users\{name}\Documents"; // 旧写法
string path2 = @$"C:\Users\{name}\Documents"; // 新写法，同样合法
```

这个小改进提升了语言的一致性和灵活性，允许开发者按自己的习惯选择写法。

## 版本适配

C# 8.0 要求项目使用 **.NET Core 3.0 及以上版本**，部分特性（如默认接口方法）依赖CLR的新增功能，仅在支持的运行时中可用。
