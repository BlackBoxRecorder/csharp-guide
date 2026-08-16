---
title: record
description: C# record 类型详解：不可变数据模型、值相等性、非破坏性修改等核心特性，以及与 readonly struct 的对比和选型建议。
---

在 C# 9.0 及更高版本中，`record` 是一种特殊的引用类型，用于简化**不可变数据模型**的创建和操作。它的核心价值在于提供值语义的不可变数据结构，默认支持值相等性、非破坏性修改和格式化的字符串输出等特性。

与 `readonly struct` 相比：**`record` 的核心是"数据建模"和"值相等性"**，而 **`readonly struct` 的核心是"性能"和"强制不可变"**。本文将先介绍 `record` 的完整用法，再与 `readonly struct` 对比，帮助你在合适的场景中做出选择。

### 核心特性

1. **不可变性（Immutability）**
   - record 的属性默认是只读的（通过 `init` 访问器），初始化后无法修改
   - 天然线程安全，适合并发场景
2. **值相等性（Value-based Equality）**
   - 编译器自动生成 `Equals()` 和 `GetHashCode()`，按所有属性的值比较（而非引用地址）
   - 例如 `recordA == recordB` 会比较所有属性值
3. **非破坏性修改（`with` 表达式）**
   - 基于已有对象创建新副本，可同时修改部分属性，原对象保持不变
4. **格式化的 `ToString()`**
   - 自动生成形如 `Person { FirstName = John, LastName = Doe, Age = 30 }` 的输出，便于调试与日志

### 基础声明

#### 位置语法（Positional Syntax）

record 支持主构造函数，一行即可完成声明：

```csharp
public record Person(string FirstName, string LastName, int Age);
```

等效于传统写法：

```csharp
public record Person
{
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public int Age { get; init; }

    // 编译器自动生成构造函数、ToString()、Equals() 等
}
```

**使用示例：**

```csharp
var person1 = new Person("John", "Doe", 30);
var person2 = new Person("John", "Doe", 30);

Console.WriteLine(person1 == person2); // true（值相等）
Console.WriteLine(person1);
// Person { FirstName = John, LastName = Doe, Age = 30 }
```

#### 使用 `with` 表达式修改副本

```csharp
var updatedPerson = person1 with { Age = 31 };
Console.WriteLine(updatedPerson);
// Person { FirstName = John, LastName = Doe, Age = 31 }
```

#### 添加自定义方法/属性

```csharp
public record Student(string Id, string Name)
{
    public string Greet() => $"Hello, {Name}!";
    public string Department { get; init; } = "Computer Science";
}

var student = new Student("S001", "Alice") { Department = "Math" };
Console.WriteLine(student.Greet()); // Hello, Alice!
```

#### 继承（仅支持 record 之间继承）

```csharp
public record Employee(string Id, string Name) : Person(Name, "Employee", 0);
```

### record struct：值类型的 record

从 C# 10 起，record 也可以声明为结构体，结合了 record 的语法糖与 struct 的性能优势：

```csharp
// 值类型 record，同样支持值相等性、with 表达式和解构
public record struct Point(double X, double Y);

var p1 = new Point(1.0, 2.0);
var p2 = p1 with { X = 3.0 };
Console.WriteLine(p1 == p2); // False
```

还可以与 `readonly` 组合，得到**强制不可变的值类型**：

```csharp
public readonly record struct Point(double X, double Y);
```

### 与 readonly struct 的区别

`readonly struct` 和 `record` 都支持不可变性与值语义，但侧重点截然不同，先通过对比表快速了解：

| 特性 | `readonly struct` | `record` / `record struct` |
| :--- | :--- | :--- |
| **类型** | 值类型 | 引用类型（`record class`）或值类型（`record struct`） |
| **核心目标** | 极致性能、内存效率、强制不可变 | 简化数据模型、值相等性、不可变性 |
| **相等性** | 默认逐字段比较，复杂场景需手动实现 `IEquatable<T>` | **自动生成**基于值的相等性（`Equals`、`GetHashCode`） |
| **不可变性** | **强制执行**，所有字段必须是 `readonly` | **默认支持**，通过 `init` 访问器和 `with` 表达式实现 |
| **语法糖** | 无特殊语法，普通 struct 加 `readonly` 修饰符 | 丰富：主构造函数、`with` 表达式、格式化 `ToString`、解构等 |
| **继承** | 不支持 | `record class` 支持，`record struct` 不支持 |

#### 相等性

**record**：编译器自动生成 `Equals()` 和 `GetHashCode()`，实现基于值的相等性。只要两个 record 对象的所有属性值相同，就被认为相等。

```csharp
public record Person(string Name, int Age);
var p1 = new Person("Alice", 30);
var p2 = new Person("Alice", 30);
Console.WriteLine(p1 == p2); // True
```

**readonly struct**：编译器不会像 record 那样自动生成基于值的相等性逻辑。默认的 `ValueType.Equals` 进行逐字段比较，性能较低；且不会自动生成 `==` 运算符，包含引用类型字段时比较的是引用而非内容，行为可能不符合直觉。需要保证值相等性时，通常要手动实现 `IEquatable<T>`。

#### 不可变性

**readonly struct**：编译器**强制**不可变性。一旦结构体声明为 `readonly`，其所有实例字段都必须是只读的，从编译器层面保证创建后无法被修改：

```csharp
public readonly struct Point
{
    public double X { get; } // 必须是只读的
    public double Y { get; }
    public Point(double x, double y) { X = x; Y = y; }
}
```

**record**：通过语法糖**方便地**实现不可变性。主构造函数的参数默认生成 `init` 属性，只能通过构造函数或对象初始化器赋值一次；要"修改"它，需要使用 `with` 表达式创建新副本：

```csharp
var p3 = p1 with { Age = 31 }; // 创建新对象，Age 变为 31
```

#### 性能

- **readonly struct**：值类型，在栈上分配（或内联在包含它的对象中），避免了堆内存分配和垃圾回收（GC）压力；`readonly` 还能让编译器避免不必要的防御性复制。适合游戏开发、科学计算等性能要求极高的场景。
- **record class**：引用类型，在堆上分配，有 GC 开销，性能与普通类相似。
- **record struct**：值类型，性能优于 `record class`。如果需要 record 的语法糖和值相等性，又追求性能，`record struct` 是很好的折中选择。

### 如何选择？

#### 使用 `readonly struct` 当

- 性能是首要考虑因素，需要极致的内存效率和最小的 GC 压力；
- 正在定义非常小且简单的数据结构，如 `Point`、`Vector`、`Color`、`Coordinate`；
- 需要从编译器层面**强制**保证类型的不可变性。

#### 使用 `record`（或 `record struct`）当

- 想简化数据模型的代码，不想手写 `Equals`、`GetHashCode` 和 `ToString`；
- 基于值的相等性对业务逻辑至关重要，例如 DTO（数据传输对象）、值对象（VO）或事件溯源；
- 需要方便地创建不可变对象，并使用 `with` 表达式进行非破坏性更新；
- 需要 `record class` 的继承特性来构建数据模型的层次结构。

### 与传统类的区别

| 特性 | `class` | `record` |
| :--- | :--- | :--- |
| 相等性 | 引用相等 | 值相等（按所有属性） |
| 可变性 | 默认可变 | 默认不可变 |
| 克隆 | 需手动实现 | 内置 `with` 支持 |
| `ToString()` | 返回类型名 | 返回属性键值对 |
| 继承 | 支持任意继承 | 仅继承其他 record |

### 适用场景

✅ DTO（数据传输对象）  
✅ 不可变数据模型（如配置、状态）  
✅ 需要值语义的领域对象  
✅ 模式匹配中的模式定义  
✅ 替代元组（Tuple）的复杂数据结构

> ⚠️ **注意**：如果需要频繁修改对象内部状态，优先使用 `class`；如果追求数据不可变性和值语义，优先选择 `record`；如果数据结构很小且性能敏感，考虑 `readonly struct` 或 `readonly record struct`。

通过 `record`，C# 大幅减少了模板代码，使开发者能专注于业务逻辑而非数据结构的实现细节。
