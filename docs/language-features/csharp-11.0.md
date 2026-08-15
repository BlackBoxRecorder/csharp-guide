---
title: C# 11.0 新特性
description: C# 11.0 版本引入的所有新特性详细说明，包含原始字符串字面量、泛型数学支持、泛型属性、UTF-8字符串字面量等特性的代码示例和最佳实践
---

# C# 11.0 新特性

C# 11.0 于2022年11月与.NET 7一同发布，围绕提升开发者生产力、强化类型安全、优化性能三大核心目标，引入了一系列覆盖字符串处理、泛型编程、模式匹配、代码组织等多个领域的重要特性，部分特性可带来显著的性能提升。

## 主要特性总览

- **原始字符串字面量**：支持多行字符串无需转义，简化JSON、XML等结构化文本编写
- **泛型数学支持**：基于`INumber<T>`等接口实现类型安全的通用数学运算
- **泛型属性**：支持泛型参数的属性类，提升元编程的类型安全性
- **UTF-8字符串字面量**：直接创建UTF-8编码的字节序列，减少编码转换开销
- **列表模式**：支持数组和列表的模式匹配，简化序列数据的处理逻辑
- **文件本地类型**：新增`file`访问修饰符，限制类型仅在当前文件可见
- **必需成员**：新增`required`修饰符，强制对象初始化时必须赋值关键属性
- **自动默认结构**：结构体未显式初始化的字段自动设置为默认值，减少样板代码
- **字符串插值表达式支持换行**：插值表达式内部支持多行编写，提升长表达式可读性

### 1. 原始字符串字面量 (Raw string literals)

原始字符串字面量使用至少三个双引号`"""`包裹，无需转义特殊字符，完整保留字符串的格式和缩进，特别适合编写JSON、HTML、正则表达式等包含大量特殊字符的内容：

```csharp
// 基本用法
string message = """
    This is a raw string literal.
    It can span multiple lines.
        It preserves indentation.
    It can contain "double quotes" and 'single quotes' without escaping.
    """;
```

支持插值功能，使用`$$`前缀，插值表达式使用`{{}}`包裹：

```csharp
string firstName = "John";
string lastName = "Doe";
string jsonMessage = $$"""
    {
        "firstName": "{{firstName}}",
        "lastName": "{{lastName}}"
    }
    """;
```

原始字符串字面量彻底消除了转义字符的困扰，大幅提升了复杂字符串的可读性和可维护性。

### 2. 泛型数学支持 (Generic math support)

基于静态抽象接口成员特性，C# 11提供了泛型数学支持，允许编写适用于所有数值类型的通用数学算法：

```csharp
using System.Numerics;

public static class MathOperations
{
    // 泛型加法，支持所有实现INumber<T>的数值类型
    public static T Add<T>(T a, T b) where T : INumber<T>
    {
        return a + b;
    }
}

// 使用示例
int intResult = MathOperations.Add(10, 20); // 30
double doubleResult = MathOperations.Add(10.5, 20.3); // 30.8
Complex complexResult = MathOperations.Add(new Complex(1,2), new Complex(3,4)); // (4, 6)
```

泛型数学避免了为不同数值类型编写重复代码，同时保持了类型安全和高性能，特别适合数学库、科学计算等场景。

### 3. 泛型属性 (Generic attributes)

支持定义泛型属性类，替代传统的`Type`参数传递方式，提供更好的类型安全：

```csharp
// 旧写法，使用Type参数
public class TypeAttribute : Attribute
{
    public TypeAttribute(Type t) => ParamType = t;
    public Type ParamType { get; }
}
[TypeAttribute(typeof(string))] // 需要typeof
public string OldMethod() => default;

// 新写法，泛型属性
public class GenericAttribute<T> : Attribute { }
[GenericAttribute<string>()] // 直接指定类型参数
public string NewMethod() => default;
```

泛型属性让元编程代码更加简洁安全，编译器会在编译时检查类型正确性，避免运行时错误。

### 4. UTF-8字符串字面量 (UTF-8 string literals)

使用`u8`后缀直接创建UTF-8编码的字节序列，避免运行时编码转换开销：

```csharp
// 直接得到UTF-8编码的ReadOnlySpan<byte>
ReadOnlySpan<byte> utf8String = "Hello, World!"u8;
```

C#字符串默认是UTF-16编码，UTF-8字符串字面量特别适合Web开发、网络通信等场景，直接与外部系统交互无需额外转换。

### 5. 列表模式 (List patterns)

新增列表模式，支持对数组和列表进行模式匹配，结合切片模式`..`可以实现强大的序列处理能力：

```csharp
int[] numbers = { 1, 2, 3, 4, 5 };

// 匹配精确序列
if (numbers is [1, 2, 3]) { }

// 匹配结尾
if (numbers is [.., 4, 5]) { }

// 捕获中间部分
if (numbers is [1, .. var middle, 5])
{
    Console.WriteLine($"Middle: {string.Join(", ", middle)}"); // 输出: 2, 3, 4
}

// 匹配首元素
if (numbers is [var first, ..])
{
    Console.WriteLine($"First: {first}"); // 输出: 1
}
```

列表模式让序列处理代码更加简洁直观，避免了大量的索引计算和长度检查。

### 6. 文件本地类型 (File-local types)

新增`file`访问修饰符，限制类型仅在当前源文件中可见，避免命名冲突：

```csharp
// 该类型仅在当前文件中可见，不会与其他文件的同名类型冲突
file class Helper
{
    // 辅助类实现
}
```

文件本地类型特别适合源代码生成器和大型项目，可以生成大量辅助类型而不用担心命名冲突。

### 7. 必需成员 (Required members)

新增`required`修饰符，强制对象初始化时必须为标记的属性赋值：

```csharp
public class Person
{
    public required string FirstName { get; init; } // 必须初始化
    public required string LastName { get; init; } // 必须初始化
    public int Age { get; init; } // 可选
}

// 正确用法
var person = new Person { FirstName = "John", LastName = "Doe" };
// var person2 = new Person { FirstName = "John" }; // 编译错误，LastName未初始化
```

必需成员确保对象在创建时就处于有效状态，避免了因属性未初始化导致的运行时错误。

### 8. 自动默认结构 (Auto-default structs)

结构体构造函数无需显式初始化所有字段，未初始化的字段会自动设置为其类型的默认值：

```csharp
public struct Point
{
    public int X;
    public int Y;

    public Point(int x)
    {
        X = x;
        // Y会被自动初始化为0，无需显式赋值
    }
}
```

自动默认结构减少了结构体定义的样板代码，简化了只需要初始化部分字段的场景。

### 9. 字符串插值表达式支持换行

允许插值表达式`{}`内部包含换行符，长表达式可以拆分成多行编写，提升可读性：

```csharp
var result = $"Total: {items
    .Where(x => x.Active)
    .Sum(x => x.Price)}";
```

### 其他重要特性

- **`ref`字段和`scoped ref`**：支持在`ref`结构中声明引用字段，提升高性能场景的灵活性
- **常量字符串模式匹配`Span<char>`**：支持直接对`Span<char>`进行常量字符串匹配，无需转换为`string`
- **扩展的`nameof`作用域**：允许在更多上下文中使用`nameof`获取成员名称
- **数值`IntPtr`/`UIntPtr`**：`nint`和`nuint`支持泛型数学运算，更适合原生互操作场景

## 版本适配

C# 11.0 要求项目使用 **.NET 7.0 及以上版本**，所有新特性均可在.NET 7及后续版本中使用。
