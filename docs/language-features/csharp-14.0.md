---
title: C# 14.0 新特性（预览版）
description: C# 14.0 版本（预计2025年11月随.NET 10发布）的新特性预览，包含主构造函数改进、集合表达式增强等最新特性说明
---

# C# 14.0 新特性（预览版）

> ⚠️ 注意：C# 14 目前处于公开预览阶段，部分特性可能会在正式发布前进行调整或修改。本文档基于当前最新的预览版内容编写，正式发布时可能会有变化。

C# 14.0 预计将于2025年11月与.NET 10一同正式发布，该版本在 C# 13 的基础上进一步增强了语言表达能力、性能和开发效率，重点围绕简化企业级应用开发、提升运行时性能、增强开发体验三个方向进行了大量改进。

## 主要特性总览

- **主构造函数改进**：支持在主构造函数中定义字段和属性的可见性
- **集合表达式增强**：支持更多集合类型和更灵活的初始化方式
- **异步流改进**：提升异步流的性能和易用性
- **泛型特性增强**：支持泛型特性的更多使用场景
- **内联数组增强**：扩展内联数组的功能和性能
- **源代码生成器改进**：提供更强大的代码生成能力
- **性能优化特性**：新增多个面向性能的语言特性
- **模式匹配增强**：扩展模式匹配的使用场景和能力
- **Lambda 表达式改进**：支持 Lambda 表达式的更多功能
- **可空引用类型改进**：增强可空性分析的准确性
- **字符串处理增强**：新增多个字符串处理相关的语法糖
- **原生AOT支持增强**：更好地支持原生AOT编译场景

### 1. 主构造函数改进

C# 14 对 C# 12 引入的主构造函数进行了大量改进，支持更灵活的成员定义和可见性控制：

```csharp
// C# 14 支持在主构造函数参数上指定可见性
public class Person(public string Name, public int Age, private string _internalId)
{
    // 无需显式定义字段，主构造函数参数自动生成为对应可见性的成员
    public string GetInternalId() => _internalId;
}

// 使用
var person = new Person("John", 30, "id-123");
Console.WriteLine(person.Name); // John
Console.WriteLine(person.Age); // 30
```

主构造函数参数现在可以直接指定 `public`、`private`、`protected`、`internal` 等可见性修饰符，编译器会自动生成对应的字段或属性，大大减少了样板代码。

### 2. 集合表达式增强

C# 14 扩展了集合表达式的能力，支持更多集合类型和更灵活的初始化方式：

```csharp
// 支持初始化自定义集合类型
public class MyCustomCollection<T> : List<T> { }

var collection = new MyCustomCollection<int> { 1, 2, 3, 4, 5 };

// 支持在集合表达式中使用条件表达式
var numbers = [ 1, 2, if (DateTime.Now.DayOfWeek == DayOfWeek.Monday) 3, 4, 5 ];

// 支持更灵活的展开运算符使用
var list1 = [1, 2, 3];
var list2 = [4, 5, 6];
var combined = [.. list1, .. list2, 7, 8, 9];
```

集合表达式现在可以用于任何支持集合初始化器的类型，并且支持条件元素和更灵活的展开操作，让集合初始化更加简洁。

### 3. 异步流改进

C# 14 对异步流（`IAsyncEnumerable<T>`）进行了多项性能和易用性改进：

```csharp
// 支持异步流的 LINQ 查询增强
var result = await myAsyncStream
    .Where(x => x > 10)
    .Select(x => x * 2)
    .Take(5)
    .ToListAsync();

// 支持异步流的并行处理
var parallelResult = await myAsyncStream
    .AsParallel()
    .WithDegreeOfParallelism(4)
    .ProcessAsync(ProcessItem);
```

改进包括更低的内存分配、更高的吞吐量，以及更多的 LINQ 操作符支持，让异步流的使用更加高效和方便。

### 4. 泛型特性增强

C# 14 扩展了 C# 11 引入的泛型特性的使用场景：

```csharp
// 支持在更多位置使用泛型特性
[GenericAttribute<string>("value")]
public class MyClass
{
    [GenericAttribute<int>(42)]
    public void MyMethod() { }
}

// 支持泛型特性的类型推理
[GenericAttribute("string value")] // 自动推断为 GenericAttribute<string>
public void AnotherMethod() { }
```

泛型特性现在支持类型推理，并且可以在更多的代码元素上使用，包括方法、参数、返回值等。

### 5. 内联数组增强

C# 14 进一步增强了内联数组的功能，支持更多操作和更好的性能：

```csharp
// 内联数组支持实现接口
[InlineArray(10)]
public struct MyBuffer : IEnumerable<int>
{
    private int _element;
    
    public IEnumerator<int> GetEnumerator() => /* 实现 */;
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

// 内联数组支持更多的运算
var buffer = new MyBuffer();
buffer.Fill(42); // 填充所有元素为42
var slice = buffer[2..5]; // 切片支持更多操作
```

内联数组现在可以实现接口，并且支持更多的内置操作，让其使用场景更加广泛。

### 6. 源代码生成器改进

C# 14 为源代码生成器提供了更强大的 API：

- 支持增量生成的更多控制
- 可以访问更多的编译时信息
- 支持生成器之间的依赖关系
- 更好的调试体验

这些改进让源代码生成器可以实现更复杂的功能，同时提升生成效率。

### 7. 性能优化特性

C# 14 新增了多个面向性能的语言特性：

- **栈分配对象增强**：支持在栈上分配更多类型的对象
- **值类型优化**：提升值类型的性能，减少不必要的拷贝
- **方法内联改进**：编译器可以更智能地进行方法内联
- **SIMD 支持增强**：更方便地编写 SIMD 代码

```csharp
// 栈分配对象示例
Span<MyStruct> structs = stackalloc MyStruct[100];
// 直接在栈上初始化结构体数组，无需堆分配
```

这些特性让开发者可以更容易地编写高性能的代码，尤其在游戏开发、大数据处理等性能敏感场景下非常有用。

### 8. 模式匹配增强

C# 14 扩展了模式匹配的能力，支持更多的模式类型：

```csharp
// 支持集合元素的属性模式匹配
int[] numbers = { 1, 2, 3, 4, 5 };
if (numbers is [> 0, < 3, var third, ..])
{
    Console.WriteLine(third); // 3
}

// 支持更复杂的属性模式嵌套
if (person is { Address: { City: "Beijing", Street.Length: > 10 } })
{
    // 匹配逻辑
}
```

模式匹配现在支持更多的场景，让条件判断代码更加简洁和易读。

### 9. Lambda 表达式改进

C# 14 对 Lambda 表达式进行了多项改进：

```csharp
// 支持 Lambda 表达式的静态抽象接口实现
public interface ICalculator
{
    static abstract int Add(int a, int b);
}

// Lambda 实现静态抽象接口
ICalculator calculator = static (a, b) => a + b;

// 支持 Lambda 表达式的参数默认值
var add = (int a, int b = 1) => a + b;
Console.WriteLine(add(5)); // 6
```

Lambda 现在可以实现静态抽象接口，并且支持参数默认值，让 Lambda 的使用更加灵活。

### 10. 可空引用类型改进

C# 14 增强了可空引用类型的分析能力：

- 更准确的流分析，减少误报
- 支持更多的模式匹配场景的可空性分析
- 更好的泛型方法可空性推理
- 新增的可空性特性

这些改进让可空引用类型的使用体验更好，帮助开发者更早地发现潜在的空引用错误。

### 11. 字符串处理增强

C# 14 新增了多个字符串处理相关的语法糖：

```csharp
// 多行原始字符串的缩进控制
var text = """
        第一行
        第二行
            第三行
    """ with { TrimIndentation = false }; // 保留原始缩进

// 字符串插值的格式增强
var value = 42;
var str = $"值为：{value:X2}"; // 格式化为16进制，两位，输出 2A
```

这些改进让字符串的处理更加方便，尤其是多行字符串的使用。

### 12. 原生AOT支持增强

C# 14 更好地支持原生AOT编译场景：

- 更多的语言特性支持在AOT环境下使用
- 减少反射的使用，提升AOT编译后的性能
- 更好的裁剪支持，减小应用体积
- 新增的 AOT 友好的 API

这些改进让使用 C# 开发原生AOT应用更加方便，尤其适合需要高性能、小体积、快速启动的场景。

## 版本适配

C# 14 要求项目使用 **.NET 10 及以上版本**，所有新特性均可在 .NET 10 及后续版本中使用。由于目前处于预览阶段，部分特性可能需要在项目中开启预览功能才能使用。
