---
title: record
description: C# record 类型详解，包括不可变数据模型、值相等性、非破坏性修改、模式匹配等核心特性。
---

在 C# 9.0 及更高版本中，​`record` 是一种特殊的引用类型主要用于简化不可变数据模型的创建和操作。它的核心作用是提供**值语义的不可变数据结构**，默认支持值相等性、非破坏性修改和模式匹配等特性。

### 核心特性与作用

1. ​**不可变性 (Immutability)​**​
    - Record 的属性默认是只读的（通过 `init` 访问器），初始化后无法修改
    - 线程安全，适合并发场景
2. ​**值语义的相等性 (Value-based Equality)​**​
    - 自动生成 `Equals()` 和 `GetHashCode()`，比较所有属性的值（而非引用地址）
    - 例如：`recordA == recordB` 会比较所有属性值
3. ​**非破坏性修改 (`with` 表达式)​**​
    - 使用 `with` 基于已有对象创建新副本，并可修改部分属性

```cs
var newCar = oldCar with { Year = 2023 };
```

### 使用方法

#### 基础声明 (Positional Syntax)

```cs
public record Person(string FirstName, string LastName, int Age);
```

等效于：

```cs
public record Person 
{
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public int Age { get; init; }
    
    // 编译器自动生成构造函数、ToString()、Equals() 等
}
```

​**使用示例：​**​

```cs
var person1 = new Person("John", "Doe", 30);
var person2 = new Person("John", "Doe", 30);

Console.WriteLine(person1 == person2); // true（值相等）
Console.WriteLine(person1); // 自动生成：Person { FirstName = John, LastName = Doe, Age = 30 }
```

#### 使用 `with` 表达式修改副本

```cs
var updatedPerson = person1 with { Age = 31 };
Console.WriteLine(updatedPerson); 
// Person { FirstName = John, LastName = Doe, Age = 31 }
```

#### 添加自定义方法/属性

```cs
public record Student(string Id, string Name)
{
    public string Greet() => $"Hello, {Name}!";
    public string Department { get; init; } = "Computer Science";
}

var student = new Student("S001", "Alice") { Department = "Math" };
Console.WriteLine(student.Greet()); // Hello, Alice!
```

#### 继承（仅支持 `record` 间继承）

```cs
public record Employee(string Id, string Name) : Person(Name, "Employee", 0);
```

### 与传统类的区别

|特性|`class`|`record`|
|---|---|---|
|相等性|引用相等|值相等（按所有属性）|
|可变性|默认可变|默认不可变|
|克隆|需手动实现|内置 `with` 支持|
|`ToString()`|返回类型名|返回属性键值对|
|继承|支持任意继承|仅继承其他 record|

### 适用场景

✅ DTO（数据传输对象）  
✅ 不可变数据模型（如配置、状态）  
✅ 需要值语义的领域对象  
✅ 模式匹配中的模式定义  
✅ 替代元组 (Tuple) 的复杂数据结构

> ⚠️ ​**注意**​：如果需要频繁修改对象内部状态，优先使用 `class`；如需数据不可变性、值语义，则选择 `record`。

通过 `record`，C# 大幅减少了模板代码，使开发者能专注于业务逻辑而非数据结构的实现细节。
