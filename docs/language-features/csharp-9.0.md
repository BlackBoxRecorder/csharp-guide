---
title: C# 9.0 新特性
description: C# 9.0 版本引入的所有新特性详细说明，包含记录类型、init-only属性、顶级语句、模式匹配增强等特性的代码示例和最佳实践
---

# C# 9.0 新特性

C# 9.0 于2020年11月与.NET 5一同发布，围绕简化常见编码模式、增强不可变性支持、提升代码声明性三大方向，带来了一系列提高代码简洁性和表达力的新特性。

## 主要特性总览

- **记录类型 (Records)**：内置不可变性和值相等性的引用类型，简化数据模型定义
- **init-only 属性**：允许对象初始化时设置属性，之后变为只读，增强不可变性
- **顶级语句**：省略Program类和Main方法，简化控制台应用入口代码
- **模式匹配增强**：新增逻辑模式（and/or/not）和关系模式（<,>,<=,>=）
- **目标类型new表达式**：创建对象时可省略类型名称，编译器自动推断
- **属性模式增强**：更简洁的嵌套属性匹配语法

### 1. 记录类型 (Records)

记录类型是C# 9.0引入的核心特性，提供了一种简洁的方式创建不可变的数据模型，默认支持基于值的相等性比较：

```csharp
// 定义记录类型
public record Person(string FirstName, string LastName);

// 使用记录
var person1 = new Person("John", "Doe");
var person2 = new Person("John", "Doe");
Console.WriteLine(person1 == person2); // 输出: True (基于值的相等性)

// 析构记录
var (firstName, lastName) = person1;
Console.WriteLine($"{firstName} {lastName}");

// 使用with表达式创建修改后的副本
var person3 = person1 with { LastName = "Smith" };
Console.WriteLine(person3); // 输出: Person { FirstName = John, LastName = Smith }
```

记录类型自动生成Equals、GetHashCode、ToString等方法和拷贝构造函数，极大减少了数据传输对象（DTO）、值对象等场景的样板代码，非常适合需要不可变性的场景。

### 2. init-only 属性

`init`访问器允许属性在对象初始化期间赋值，初始化完成后变为只读，为创建不可变对象提供了更灵活的方式：

```csharp
public class Person
{
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public int Age { get; set; }
}

var person = new Person { FirstName = "John", LastName = "Doe", Age = 30 };
// person.FirstName = "Jane"; // 编译错误，init-only属性初始化后不可修改
person.Age = 31; // 普通set属性可以修改
```

记录类型的主构造函数参数默认生成init属性，两者通常搭配使用，简化不可变对象的创建。

### 3. 顶级语句 (Top-level statements)

顶级语句简化了控制台应用程序的入口代码，允许省略Program类和Main方法的显式定义：

```csharp
// 旧写法，需要完整的类和方法结构
using System;
class Program
{
    static void Main()
    {
        Console.WriteLine("Hello, World!");
    }
}

// 新写法，直接编写执行代码
using System;
Console.WriteLine("Hello, World!");
```

编译器会自动将顶级语句包装到生成的Program类和Main方法中，一个项目只能有一个文件包含顶级语句。这个特性特别适合编写小型工具、脚本和快速原型，降低了初学者的上手门槛。

### 4. 模式匹配增强

C# 9.0新增逻辑模式（and/or/not）和关系模式，大幅提升了模式匹配的表达能力：

```csharp
// 关系模式
public static string GetTemperatureDescription(int temperature) => temperature switch
{
    < 0 => "Freezing",
    >= 0 and < 20 => "Cold",
    >= 20 and < 30 => "Moderate",
    >= 30 => "Hot"
};

// 逻辑模式
public static bool IsLetterOrSeparator(char c) => 
    c is (>= 'a' and <= 'z') or (>= 'A' and <= 'Z') or '.' or ',';

// not模式
if (obj is not null)
{
    // 不为null时执行
}
```

这些新的模式类型让复杂的条件逻辑更加简洁易读，减少了冗长的布尔表达式和嵌套if-else语句。

### 5. 目标类型new表达式

允许在对象创建时省略类型名称，编译器根据赋值目标的类型自动推断：

```csharp
// 旧写法，需要重复写类型
Dictionary<string, List<int>> myDictionary = new Dictionary<string, List<int>>();

// 新写法，省略类型
Dictionary<string, List<int>> myDictionary = new();

// 同样适用于字段和方法参数
public class MyClass
{
    private List<string> _names = new(); // 推断为List<string>
    
    public void ProcessData(List<int> data = new()) // 推断为List<int>
    {
        // ...
    }
}
```

这个特性减少了重复的类型声明，尤其在使用复杂泛型类型时代码更加简洁。

### 6. 属性模式增强

C# 9.0简化了属性模式的语法，特别是嵌套属性的匹配：

```csharp
public class Address { public string City { get; set; } }
public class Person { public string Name { get; set; } public Address Address { get; set; } }

Person person = new Person { Name = "John", Address = new Address { City = "London" } };

if (person is { Name: "John", Address.City: "London" })
{
    Console.WriteLine("John from London");
}
```

属性模式可以深入匹配对象的内部属性，让条件判断更加直观和简洁。

> 注：`??=` 空合并赋值运算符已于C# 8.0引入，C# 9.0进一步扩展了其适用场景，支持在ref变量、属性和索引器访问等更多场景下使用，例如用于属性的延迟初始化：
>
> ```csharp
> public List<int> Numbers { get => _numbers ??= new List<int>(); }
> ```

## 版本适配

C# 9.0 要求项目使用 **.NET 5 及以上版本**，所有新特性均可在.NET 5及后续版本中使用。
