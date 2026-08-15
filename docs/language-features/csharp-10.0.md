---
title: C# 10.0 新特性
description: C# 10.0 版本引入的所有新特性详细说明，包含记录结构、插值字符串处理程序、全局using指令、文件范围命名空间等特性的代码示例和最佳实践
---

# C# 10.0 新特性

C# 10.0 于2021年11月与.NET 6一同发布，围绕提升开发者生产力、简化代码结构、优化性能三个核心方向，引入了一系列覆盖数据建模、代码组织、性能优化等多个领域的新特性。

## 主要特性总览

- **记录结构 (Record structs)**：将记录类型的特性扩展到值类型，支持高性能数据模型
- **内插字符串处理程序**：高性能低分配的字符串插值处理机制，提升字符串操作性能
- **`global using` 指令**：全局命名空间引用，避免每个文件重复写相同的using指令
- **文件范围命名空间声明**：简化命名空间声明，减少代码缩进层级
- **扩展属性模式**：更简洁的嵌套属性匹配语法，提升模式匹配表达力
- **Lambda 表达式改进**：增强类型推断，支持特性应用，提升Lambda灵活性

### 1. 记录结构 (Record structs)

记录结构将记录类型的特性扩展到值类型，提供了轻量级、高性能的基于值的数据模型：

```csharp
// 只读记录结构，属性不可变
public readonly record struct Point(int X, int Y);

// 可变记录结构，属性默认可修改
public record struct MutablePoint(int X, int Y);
```

使用记录结构：

```csharp
var p1 = new Point(1, 2);
var p2 = new Point(1, 2);
Console.WriteLine(p1 == p2); // 输出: True (基于值的相等性)

var mutableP = new MutablePoint(3, 4);
mutableP.X = 5; // 可变记录结构的属性可以修改
```

记录结构继承了记录类型的简洁语法、值相等性比较、ToString输出和解构功能，同时作为值类型在栈上分配，特别适合性能敏感场景下的数据建模。

### 2. 内插字符串处理程序 (Interpolated string handlers)

内插字符串处理程序提供了高性能、低分配的字符串插值处理机制，避免传统插值方式产生的不必要中间字符串分配：

```csharp
int value = 42;
// 高性能字符串构建，避免中间分配
string message = string.Create(null, $"The answer is {value}");
```

虽然普通开发者通常不需要自定义处理程序，但可以直接受益于.NET库中使用该特性实现的高性能API，特别是在日志记录、字符串模板等高频场景下性能提升明显。

### 3. `global using` 指令

`global using` 指令允许在单个文件中声明全局命名空间引用，在整个项目的所有源文件中都生效：

```csharp
// GlobalUsings.cs（文件名可自定义）
global using System;
global using System.Collections.Generic;
global using System.Linq;
```

声明后，项目中所有文件无需重复添加这些using指令即可直接使用对应命名空间的类型，大幅减少了文件顶部的重复代码，让代码更加整洁。

### 4. 文件范围命名空间声明

文件范围命名空间提供了更简洁的命名空间声明方式，无需嵌套花括号：

```csharp
// 旧写法，需要嵌套花括号
namespace MyApplication.Models
{
    public class Person { /* ... */ }
}

// 新写法，文件范围声明
namespace MyApplication.Models;

public class Person { /* ... */ }
```

这种方式减少了一层代码缩进，特别适合一个文件只包含少数类型的场景，让文件结构更加清晰，避免了不必要的花括号嵌套。

### 5. 扩展属性模式

C# 10简化了属性模式的嵌套语法，支持直接使用点号访问嵌套属性：

```csharp
public class Address { public string City { get; set; } }
public class Person { public string Name { get; set; } public Address Address { get; set; } }

Person person = new Person { Name = "John", Address = new Address { City = "London" } };

// 旧写法，需要嵌套
if (person is { Name: "John", Address: { City: "London" } }) { }

// 新写法，更简洁
if (person is { Name: "John", Address.City: "London" })
{
    Console.WriteLine("John from London");
}
```

扩展属性模式让深层属性匹配更加直观易读，进一步增强了模式匹配处理复杂对象图的能力。

### 6. Lambda 表达式改进

C# 10对Lambda表达式进行了多项增强，提升了其灵活性和表达力：

1. **更智能的类型推断**：编译器能够更准确地推断Lambda的参数类型和返回类型
2. **支持应用特性**：可以在Lambda表达式或其参数上应用特性：

   ```csharp
   Action<int> action = [MyAttribute] (x) => Console.WriteLine(x);
   ```

3. **显式类型指定**：支持显式指定Lambda表达式为委托或表达式树类型：

   ```csharp
   Expression<Func<int, int>> squareExpr = x => x * x;
   ```

这些改进让Lambda表达式在更多场景下都能便捷使用，为元编程、AOP等高级场景提供了更好的支持。

## 版本适配

C# 10.0 要求项目使用 **.NET 6 及以上版本**，所有新特性均可在.NET 6及后续版本中使用。
