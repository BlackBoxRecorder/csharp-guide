---
title: readonly struct 和 record 的区别
description: C# 中 readonly struct 和 record 的详细对比，包括性能、不可变性、值语义、适用场景等方面的区别。
---

**`readonly struct` 的核心是“性能”和“强制不可变”**，而 **`record` 的核心是“数据建模”和“值相等性”**。

下面我们从几个关键维度来详细对比它们的区别。

### 核心区别对比

| 特性 | `readonly struct` | `record` / `record struct` |
| :--- | :--- | :--- |
| **类型** | 值类型 | 引用类型 (`record class`) 或 值类型 (`record struct`) |
| **核心目标** | 极致性能、内存效率、强制不可变 | 简化数据模型、值相等性、不可变性 |
| **相等性** | 默认基于字段的值相等性，但需手动实现或依赖编译器默认行为 | **自动生成**基于值的相等性 (`Equals`, `GetHashCode`) |
| **不可变性** | **强制执行**。所有字段必须是 `readonly`。 | **默认支持**。通过 `init` 访问器和 `with` 表达式实现。 |
| **语法糖** | 无特殊语法，只是普通的结构体加上 `readonly` 修饰符。 | 丰富。包括主构造函数、`with` 表达式、格式化 `ToString`、解构等。 |
| **继承** | 不支持。 | `record class` 支持继承，`record struct` 不支持。 |

---

### 详细解析

#### 1. 相等性 (Equality)

这是两者最显著的区别之一。

* **`record`**: 编译器会自动为你生成 `Equals()` 和 `GetHashCode()` 方法，实现**基于值的相等性**。这意味着只要两个 `record` 对象的所有属性值都相同，它们就被认为是相等的。

    ```csharp
    public record Person(string Name, int Age);
    var p1 = new Person("Alice", 30);
    var p2 = new Person("Alice", 30);
    Console.WriteLine(p1 == p2); // 输出: True
    ```

* **`readonly struct`**: 虽然结构体是值类型，但编译器不会像 `record` 那样为你生成复杂的值相等性逻辑。对于简单的字段，它可能进行逐字段比较，但对于包含引用类型字段的情况，行为可能不符合直觉。如果你想确保正确的值相等性，通常需要手动实现 `IEquatable<T>` 接口。

#### 2. 不可变性 (Immutability)

两者都支持不可变性，但实现方式和严格程度不同。

* **`readonly struct`**: 它**强制**不可变性。一旦结构体被声明为 `readonly`，其所有实例字段都必须是 `readonly`。这从编译器层面保证了对象创建后无法被修改，为高性能场景提供了安全保障。

    ```csharp
    public readonly struct Point
    {
        public double X { get; } // 必须是只读的
        public double Y { get; }
        public Point(double x, double y) { X = x; Y = y; }
    }
    ```

* **`record`**: 它通过语法糖**方便地实现**不可变性。主构造函数的参数默认生成 `init` 属性，你只能通过构造函数或对象初始化器设置一次。要“修改”它，需要使用 `with` 表达式创建一个新副本。

    ```csharp
    var p3 = p1 with { Age = 31 }; // 创建一个新对象，Age 变为 31
    ```

#### 3. 性能 (Performance)

* **`readonly struct`**: 作为值类型，它在栈上分配（或内联在包含它的对象中），避免了堆内存分配和垃圾回收（GC）的压力。`readonly` 修饰符还能让编译器进行更多优化，例如避免不必要的防御性复制，因此在性能要求极高的场景下（如游戏开发、科学计算）是首选。

* **`record class`**: 作为引用类型，它在堆上分配，会有 GC 开销。性能与普通类相似。

* **`record struct`**: 结合了 `record` 的语法糖和 `struct` 的性能优势。它是值类型，性能优于 `record class`。如果你需要 `record` 的值相等性语法，同时又追求性能，`record struct` 是一个很好的折中选择。

### 如何选择？

#### 使用 `readonly struct` 当

* **性能是首要考虑因素**，你需要极致的内存效率和最小的 GC 压力。
* 你正在定义一个非常小的、简单的数据结构，如 `Point`、`Vector`、`Color`、`Coordinate`。
* 你需要从编译器层面**强制**保证类型的不可变性。

#### 使用 `record` (或 `record struct`) 当

* 你主要想**简化数据模型**的代码，不想手写 `Equals`、`GetHashCode` 和 `ToString`。
* **基于值的相等性**对你的业务逻辑至关重要，例如在 DTO（数据传输对象）、VO（值对象）或事件溯源中。
* 你需要方便地创建不可变对象，并使用 `with` 表达式进行非破坏性更新。
* 你需要 `record class` 的继承特性来构建数据模型的层次结构。
