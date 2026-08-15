---
title: C#中的结构体
description: C# 结构体（struct）详解，包括值类型特性、内存管理、与类的区别、适用场景和最佳实践。
---

C# 中的结构体（`struct`）是一种轻量级的**值类型**，主要用于封装与特定概念相关的数据和少量功能。它在设计上与类（`class`）相似，但在内存管理、赋值行为和适用场景上存在根本区别。

### 核心特性

1. **值类型**
    结构体是值类型，变量直接包含结构体的数据。当将一个结构体变量赋值给另一个变量，或将其作为参数传递给方法时，系统会创建该结构体实例的一个完整副本。因此，对副本的修改不会影响原始结构体。

2. **内存分配**
    结构体实例通常在**栈（Stack）**上分配内存，或者在包含它的对象内部（例如作为类的字段）分配。这与在**堆（Heap）**上分配并由垃圾回收器（GC）管理的类不同。频繁创建和销毁小型结构体通常比类更高效，因为它不会产生 GC 压力。

3. **默认值**
    创建结构体时，其所有字段都会被初始化为其各自的默认值（例如，数字为 `0`，布尔值为 `false`，引用类型为 `null`）。

### 语法与用法

你可以使用 `struct` 关键字来定义一个结构体。

```csharp
public struct Coords
{
    // 公共字段
    public double X;
    public double Y;

    // 带参数的构造函数
    public Coords(double x, double y)
    {
        X = x;
        Y = y;
    }

    // 实例方法
    public double DistanceToOrigin()
    {
        return Math.Sqrt(X * X + Y * Y);
    }

    // 重写 ToString 方法
    public override string ToString() => $"({X}, {Y})";
}
```

#### 初始化方式

**使用 `new` 运算符和构造函数**：这是最常见的方式。

```csharp
Coords p1 = new Coords(3.0, 4.0);
```

**不使用 `new` 运算符**：你可以声明一个结构体变量而不使用 `new`。但在这种情况下，你必须在使用前显式初始化其所有字段，否则编译器会报错。

```csharp
Coords p2;
p2.X = 1.0;
p2.Y = 2.0; // 必须初始化所有字段后才能使用
```

**对象初始化器**：使用对象初始化器来设置公共字段和属性。

```csharp
Coords p3 = new Coords { X = 5.0, Y = 6.0 };
```

#### 不可变性 (`readonly struct`)

为了提高性能和保证线程安全，你可以将结构体声明为 `readonly`，使其不可变。

```csharp
public readonly struct Point
{
    public int X { get; }
    public int Y { get; }

    public Point(int x, int y)
    {
        X = x;
        Y = y;
    }
}
```

`readonly` 结构体的所有实例字段都必须是只读的，这保证了结构体一旦创建，其状态就不能被修改。

#### 结构体中的方法

创建结构体实例后，就可以直接调用它的实例方法。

```csharp
public struct Rectangle
{
    // 字段
    public double Width;
    public double Height;

    // 构造函数
    public Rectangle(double width, double height)
    {
        Width = width;
        Height = height;
    }

    // 实例方法：计算面积
    public double GetArea()
    {
        return Width * Height;
    }

    // 实例方法：计算周长
    public double GetPerimeter()
    {
        return 2 * (Width + Height);
    }
}
```

使用结构体中的方法：

```csharp
// 1. 创建结构体实例
Rectangle rect = new Rectangle(5.0, 3.0);

// 2. 调用实例方法
double area = rect.GetArea();
double perimeter = rect.GetPerimeter();

Console.WriteLine($"面积: {area}, 周长: {perimeter}");
// 输出: 面积: 15, 周长: 16
```

#### 结构体中的静态方法

静态方法属于结构体**类型本身**，而不是某个具体的实例。你不需要创建结构体的对象，就可以直接通过结构体的名称来调用这些方法。

```csharp
public struct Point
{
    public int X;
    public int Y;

    // 构造函数
    public Point(int x, int y)
    {
        X = x;
        Y = y;
    }

    // 1. 静态工厂方法：创建一个原点 (0,0)
    public static Point CreateOrigin()
    {
        return new Point(0, 0);
    }

    // 2. 静态工具方法：计算两个点之间的距离
    // 注意：这里只操作传入的参数，不操作当前实例
    public static double GetDistance(Point p1, Point p2)
    {
        double dx = p1.X - p2.X;
        double dy = p1.Y - p2.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }
}
```

调用静态方法时，直接通过结构体名称访问：

```csharp
// 调用静态工厂方法
Point origin = Point.CreateOrigin(); 
Console.WriteLine($"原点: {origin.X}, {origin.Y}"); // 输出: 0, 0

// 调用静态工具方法
Point p1 = new Point(0, 0);
Point p2 = new Point(3, 4);
double distance = Point.GetDistance(p1, p2);

Console.WriteLine($"距离: {distance}"); // 输出: 5
```

#### 重要注意事项：值类型行为

虽然结构体可以拥有方法，但请务必记住它仍然是**值类型**。当你将一个结构体变量传递给方法时，传递的是它的一个**副本**，在这个方法内部修改了结构体的字段，这些修改**不会**影响到原始的结构体变量。如果你希望在方法调用后原始结构体的值被改变，需要使用 `ref` 关键字按引用传递。

参考以下代码：

```csharp
public struct Counter
{
    public int Count;

    // 这个实例方法会修改结构体自身的状态
    public void Increment()
    {
        Count++;
    }
}

void Main()
{
    Counter myCounter = new Counter { Count = 0 };
    myCounter.Increment(); // 直接调用，myCounter.Count 变为 1
    Console.WriteLine(myCounter.Count); // 输出是 1

    ModifyCounter(myCounter); // 传递副本
    Console.WriteLine(myCounter.Count); // 输出仍然是 1，而不是 2

    ModifyCounterByRef(ref myCounter); // 传递引用
    Console.WriteLine(myCounter.Count); // 输出是 2
}

// 这个方法接收的是 myCounter 的一个副本
void ModifyCounter(Counter c)
{
    c.Increment(); // 修改的是副本 c，原始的 myCounter 不受影响
}

void ModifyCounterByRef(ref Counter c)
{
    c.Increment(); // 通过引用修改的是原始的 myCounter
}

```

### 结构体与类的区别

理解结构体和类之间的区别至关重要，它决定了你应该在何时使用哪一种。

| 特性 | 结构体 (struct) | 类 (class) |
| :--- | :--- | :--- |
| **类型** | 值类型 | 引用类型 |
| **内存分配** | 栈或内联 | 堆 |
| **赋值行为** | 复制整个实例 | 复制引用 |
| **继承** | 不支持继承，但可实现接口 | 支持继承和多态 |
| **默认构造函数** | 隐式存在，无法自定义 | 可自定义或隐式提供 |
| **可为 null** | 默认不可为 null (可使用 `Nullable<T>`) | 可为 null |
| **适用场景** | 轻量级、数据为中心的对象 | 复杂、行为为中心的对象 |

### 最佳实践

**何时使用结构体？**

* **表示轻量级对象**：当你需要表示一个简单的数据容器，如坐标点（`Point`）、矩形（`Rectangle`）、颜色（`Color`），结构体是理想选择。
* **性能敏感**：当你需要创建大量的小型对象（例如，一个包含数千个点的数组）时，使用结构体可以避免大量的堆内存分配和垃圾回收，从而提升性能。

**何时避免使用结构体？**

* **对象体积过大**：复制大型结构体会带来高昂的性能开销。在这种情况下，使用类（引用类型）更高效。
* **需要继承或多态**：结构体不支持继承。
* **需要频繁修改状态**：可变的结构体在作为方法参数传递时，可能会因为隐式的副本创建而导致意想不到的行为。如果必须修改，应使用 `ref` 关键字按引用传递。
* **包含大量引用类型字段**：虽然结构体本身在栈上，但如果它包含引用类型字段，这些字段指向的对象仍在堆上。复制结构体时，只会复制引用，而不是引用指向的对象，这可能导致共享状态和意外的副作用。
