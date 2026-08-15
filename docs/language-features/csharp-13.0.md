---
title: C# 13.0 新特性
description: C# 13.0 版本引入的所有新特性详细说明，包含扩展属性、any 类型别名、params 集合表达式等特性的代码示例和最佳实践
---

# C# 13.0 新特性

C# 13.0 于2024年11月与.NET 9一同正式发布，带来了一系列旨在提升开发效率、代码简洁性和运行时性能的新特性。该版本围绕着简化常见编码模式、增强语言表达能力、提升性能这三个核心方向进行了大量改进，同时也为未来的语言特性打下了基础。

## 主要特性总览

- **扩展属性**：无需修改原有类即可为其添加新属性
- **`any` 类型别名**：统一表示任意类型，简化泛型约束
- **扩展用户自定义运算符**：支持为现有类型扩展运算符
- **`params` 集合表达式**：直接在 params 参数中使用集合表达式
- **`lock` 语句支持任意对象**：不再局限于引用类型
- **分部属性访问器**：支持将属性的 get 和 set 访问器分别定义在不同文件
- **内联数组优化**：大幅提升内联数组的使用体验和性能
- **新的转义序列 `\e`**：表示 ESC 字符，简化终端控制代码编写
- **只读局部变量和参数**：增强不可变性支持
- **类型推理增强**：编译器可以更智能地推断类型
- **接口静态抽象成员默认实现**：为接口静态成员提供默认实现
- **文件级命名空间支持嵌套**：更灵活的命名空间组织方式

### 1. 扩展属性（Extension Properties）

C# 13 引入了期待已久的扩展属性特性，允许开发者在不修改原有类定义的情况下，为其添加新的属性。这一特性极大地增强了代码的可扩展性，尤其在处理第三方库类型时非常有用。

```csharp
// 定义扩展属性
public static class StringExtensions
{
    public static int WordCount(this string str)
    {
        if (string.IsNullOrWhiteSpace(str)) return 0;
        return str.Split(new[] {' ', '\t', '\n'}, StringSplitOptions.RemoveEmptyEntries).Length;
    }
    
    // 支持带 set 的扩展属性
    public static string AppendSuffix(this string str, string suffix)
    {
        return str + suffix;
    }
}

// 使用扩展属性
string text = "Hello World";
Console.WriteLine(text.WordCount); // 输出: 2
```

扩展属性可以是只读的，也可以支持写入，并且可以像普通属性一样使用，极大地提升了代码的可读性。

### 2. `any` 类型别名

C# 13 新增了 `any` 关键字作为 `object?` 的别名，用于表示任意类型（包括值类型和引用类型，可空和不可空）。这一特性简化了泛型约束的编写，同时让代码意图更加清晰。

```csharp
// 使用 any 表示任意类型
any value = 42;
value = "string";
value = DateTime.Now;

// 在泛型约束中使用
public void Process<T>(T input) where T : any
{
    // 可以处理任意类型的输入
}
```

`any` 和 `object` 的区别在于 `any` 明确表示可空，并且语义上更清晰地表达"任意类型"的含义。

### 3. 扩展用户自定义运算符

C# 13 允许开发者为现有类型扩展自定义运算符，这使得在不修改原有类型的情况下，可以为其添加运算符支持。

```csharp
public static class IntExtensions
{
    // 为 int 类型扩展自定义 ++ 运算符（后缀）
    public static int operator ++(int value)
    {
        return value + 2; // 每次加 2
    }
}

// 使用扩展运算符
int num = 1;
num++;
Console.WriteLine(num); // 输出: 3
```

这一特性在处理领域模型时非常有用，可以为值类型添加符合业务逻辑的运算符支持。

### 4. `params` 集合表达式

C# 13 扩展了 `params` 参数的使用场景，允许直接传入集合表达式作为参数，无需显式创建数组。

```csharp
// 定义 params 参数方法
public void PrintNumbers(params int[] numbers)
{
    foreach (var num in numbers) Console.WriteLine(num);
}

// 使用集合表达式调用
PrintNumbers([1, 2, 3, 4, 5]);
PrintNumbers([10, 20, .. new[] {30, 40}, 50]); // 支持展开运算符
```

这一特性让方法调用更加简洁，避免了不必要的数组创建代码。

### 5. `lock` 语句支持任意对象

在之前的 C# 版本中，`lock` 语句只能锁定引用类型的对象。C# 13 放宽了这一限制，允许锁定任意类型的对象，包括值类型。

```csharp
// 现在可以锁定值类型对象
int lockObj = 0;
lock (lockObj)
{
    // 临界区代码
    Console.WriteLine("Locked execution");
}
```

这一改进简化了多线程同步代码的编写，尤其在需要轻量级锁的场景下非常有用。

### 6. 分部属性访问器

C# 13 支持将属性的 `get` 和 `set` 访问器分别定义在不同的文件中，这对于源代码生成器场景非常有用。

```csharp
// File1.cs
public partial class Person
{
    public partial string Name { get; set; }
}

// File2.cs (生成的代码)
public partial class Person
{
    private string _name;
    
    public partial string Name
    {
        get => _name;
        set => _name = value ?? throw new ArgumentNullException(nameof(value));
    }
}
```

这一特性让源代码生成器可以生成属性的实现逻辑，而开发者可以在另一个文件中定义属性的签名。

### 7. 内联数组优化

C# 13 对内联数组进行了大量优化，提升了性能并且扩展了使用场景：

- 支持更多的操作：索引访问、切片、枚举等
- 更好的编译器优化，减少内存拷贝
- 支持作为泛型类型参数使用

```csharp
[System.Runtime.CompilerServices.InlineArray(10)]
public struct Buffer<T>
{
    private T _element;
}

// 使用更方便
var buffer = new Buffer<int>();
for (int i = 0; i < 10; i++)
{
    buffer[i] = i;
}

// 支持切片
var slice = buffer[2..5];
```

### 8. 新的转义序列 `\e`

C# 13 新增了 `\e` 转义序列，用于表示 ESC 字符（ASCII 码 27），这大大简化了终端控制代码的编写。

```csharp
// 使用 \e 表示 ESC 字符，设置终端文本颜色为红色
Console.WriteLine("\e[31mThis text is red\e[0m");
```

在之前的版本中，需要使用 `\u001b` 或 `\x1b` 来表示 ESC 字符，现在 `\e` 更加简洁易读。

### 9. 只读局部变量和参数

C# 13 新增了 `readonly` 关键字用于修饰局部变量和参数，确保它们在初始化后不会被修改。

```csharp
public void Process(readonly int input)
{
    // input = 10; // 编译错误：不能修改 readonly 参数
    
    readonly int localVar = 20;
    // localVar = 30; // 编译错误：不能修改 readonly 局部变量
}
```

这一特性增强了代码的不可变性，避免意外修改不需要变化的变量，提升了代码的健壮性。

### 10. 类型推理增强

C# 13 大幅提升了类型推理的能力，编译器现在可以在更多场景下正确推断类型：

- 更智能的泛型方法类型推理
- 集合表达式的类型推理增强
- Lambda 表达式返回类型推理改进

```csharp
// 之前需要显式指定类型，现在编译器可以自动推断
var list = new[] {1, 2, 3}.Select(x => x * 2).ToList();
```

类型推理的增强减少了不必要的类型显式声明，让代码更加简洁。

### 11. 接口静态抽象成员默认实现

C# 11 引入了接口静态抽象成员，C# 13 进一步支持为这些成员提供默认实现。

```csharp
public interface IFactory<T>
{
    static abstract T Create();
    
    // C# 13 支持默认实现
    static virtual T CreateDefault() => Activator.CreateInstance<T>();
}

// 实现类不需要强制实现 CreateDefault 方法
public class MyClass : IFactory<MyClass>
{
    public static MyClass Create() => new();
}

// 使用默认实现
var instance = IFactory<MyClass>.CreateDefault();
```

这一特性让接口设计更加灵活，避免实现类需要实现大量通用方法。

### 12. 文件级命名空间支持嵌套

C# 10 引入了文件级命名空间，C# 13 进一步支持嵌套的文件级命名空间声明。

```csharp
// 之前需要这样写
namespace MyApp.Models;
namespace MyApp.Models.Entities;

// 现在可以这样写，更简洁
namespace MyApp.Models.Entities;
```

这一改进让命名空间的组织更加灵活，符合常见的项目结构。

## 版本适配

C# 13 要求项目使用 **.NET 9 及以上版本**，所有新特性均可在 .NET 9 及后续版本中使用。
