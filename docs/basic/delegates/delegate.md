---
title: 委托
description: C# 委托（Delegate）的完整学习指南，包括基本概念、多播委托、匿名方法、Lambda 表达式等。
---

C# 中的委托（Delegate）是一种 **类型安全的函数指针** ，它允许将方法作为参数传递给其他方法，是实现事件处理、回调函数、LINQ 查询等功能的基石。

## 委托的基本概念

委托是一种**引用类型变量**，用于存储对某个方法的引用。所有委托类型都派生自 `System.Delegate` 类，它提供类型安全保障，确保只有签名匹配的方法才能被引用。

**委托的核心价值**：

- **类型安全**：只有与委托签名匹配的方法才能赋值给委托
- **动态调用**：可以在运行时改变委托引用的方法
- **方法抽象**：将方法视为对象，可以传递、存储和操作

## 委托的使用

### 声明委托

```csharp
// 语法：public delegate 返回类型 委托名(参数列表);
public delegate int MathOperation(int x, int y);
public delegate void PrintDelegate(string message);
```

1. **声明委托类型**：定义方法签名
2. **创建委托对象**：实例化委托
3. **定义目标方法**：创建符合委托格式的方法
4. **绑定方法**：将方法赋值给委托

**示例**

```csharp
// 1. 声明委托
public delegate int CalculateDelegate(int x, int y);

// 2. 定义目标方法
public static int Add(int a, int b) => a + b;
public static int Multiply(int a, int b) => a * b;

// 3. 使用委托
static void Main()
{
    // 创建委托实例（三种方式）
    CalculateDelegate addDelegate = new CalculateDelegate(Add);  // 传统方式
    CalculateDelegate multiplyDelegate = Multiply;              // 简化方式
    CalculateDelegate lambdaDelegate = (x, y) => x + y;         // Lambda表达式
  
    // 调用委托（两种方式）
    int result1 = addDelegate(5, 3);           // 直接调用
    int result2 = multiplyDelegate.Invoke(5, 3); // 使用Invoke方法
}
```

## 多播委托（Multicast Delegate）

一个委托可以引用多个方法，通过 `+` 或 `+=` 运算符添加方法，通过 `-` 或 `-=` 移除方法。

### 多播委托示例

```csharp
public delegate void PrintMessage(string message);

static void Main()
{
    PrintMessage print = PrintUpperCase;
    print += PrintLowerCase;  // 添加第二个方法
  
    print("Hello");  // 依次执行：HELLO 和 hello
  
    print -= PrintLowerCase;  // 移除一个方法
    print("World");  // 只输出：WORLD
}

static void PrintUpperCase(string msg) => Console.WriteLine(msg.ToUpper());
static void PrintLowerCase(string msg) => Console.WriteLine(msg.ToLower());
```

### 多播委托的返回值问题

如果多播委托有返回值，调用时返回 **最后一个方法** 的返回值：

```csharp
public delegate int MyDelegate();

MyDelegate del1 = () => 100;
MyDelegate del2 = () => 200;
MyDelegate del = del1 + del2;

Console.WriteLine(del());  // 输出 200（最后添加的方法返回值）
```

## 委托的使用场景

### 回调函数（Callback）

委托可以作为方法参数，实现回调机制：

```csharp
public delegate void ProcessCallback(int result);

public static void LongRunningOperation(ProcessCallback callback)
{
    // 模拟耗时操作
    int result = 42;
    callback(result);  // 完成后调用回调
}

// 使用
LongRunningOperation(r => Console.WriteLine($"结果：{r}"));
```

### 事件机制

委托是 C# 事件机制的基石：

```csharp
public class Button
{
    // 定义事件（基于委托）
    public event EventHandler Click;
  
    public void OnClick()
    {
        Click?.Invoke(this, EventArgs.Empty);
    }
}

// 订阅事件
Button button = new Button();
button.Click += (sender, e) => Console.WriteLine("按钮被点击！");
button.OnClick();  // 触发事件
```

## 内置委托类型

.NET 提供了几种常用的预定义委托，简化了代码编写。

:::note
常见的委托是 `Func` 和 `Action` ，参考[这里](/basic/delegates/func-and-action)
事件 Event 也是一种特殊的委托，参考[这里](/basic/delegates/event)
:::

### Predicate 委托

用于返回 **bool** 值的方法，常用于条件判断：

```csharp
Predicate<int> isEven = x => x % 2 == 0;
Predicate<string> isUpperCase = s => s.Equals(s.ToUpper());

List<int> numbers = new() { 1, 2, 3, 4, 5 };
var evenNumbers = numbers.FindAll(isEven);  // 找到所有偶数
```

## 泛型委托

委托可以定义为泛型，提高类型安全性和复用性：

```csharp
public delegate T Operation<T>(T x, T y);

static void Main()
{
    Operation<int> intAdd = (a, b) => a + b;
    Operation<string> stringConcat = (s1, s2) => s1 + s2;
  
    Console.WriteLine(intAdd(10, 20));         // 30
    Console.WriteLine(stringConcat("C# ", "Delegate"));  // C# Delegate
}
```

## 注意事项

- **空值检查**：调用前检查委托是否为 null
- **匿名方法的限制**：匿名方法无法单独从多播委托中移除，可能导致内存泄漏[4]
- **优先使用内置委托**：Action、Func、Predicate 可简化代码
- **明确委托用途**：回调、事件、策略模式等不同场景使用不同的设计
- **注意性能**：频繁创建委托实例可能影响性能，可考虑缓存
