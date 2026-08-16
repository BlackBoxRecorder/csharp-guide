---
title: ref、in、out 参数传递
description: C# 中 ref、in、out 参数修饰符的详细对比与使用场景，涵盖值传递与引用传递的区别、out 的 TryParse 模式以及 in 的只读引用传递性能优化。
---

在 C# 中，`ref`、`out` 和 `in` 是方法参数的修饰符，它们都实现了**按引用传递**：方法接收到的不是变量的副本，而是变量本身的内存地址。虽然三者都按引用传递，但在初始化要求、读写权限和使用场景上有明显区别，先通过一张对比表快速了解：

| 修饰符 | 调用前初始化 | 方法内可读 | 方法内可写 | 主要用途 |
| :--- | :--- | :--- | :--- | :--- |
| `ref` | 必须 | 可以 | 可以 | 双向传递：方法读取并修改调用方的变量 |
| `out` | 不必 | 不可以 | 必须赋值 | 单向输出：方法负责给变量赋值（如 `TryParse` 模式） |
| `in` | 必须 | 可以 | 禁止 | 只读引用传递：避免大型结构体的复制开销 |

### 值传递和引用传递

在了解 `ref`、`out`、`in` 的作用前，先弄清楚什么是值传递，什么是引用传递。

**值传递（Pass by Value）**

值传递时，方法接收的是实际参数的**副本**，方法内部的修改不会影响原始变量。

- 对于值类型（如 `int`、`float`、`struct` 等），传递的是值的副本；
- 对于引用类型，传递的是**对象引用的副本**。副本仍然指向堆上的同一个对象，因此通过副本修改对象的状态（如给 `List` 添加元素），会影响到原始对象。

**引用传递（Pass by Reference）**

使用 `ref`、`out` 或 `in` 关键字就是引用传递，方法直接操作调用方的变量本身，而不是它的副本。

- `ref`：参数在调用前必须已经初始化；
- `out`：参数无需预先初始化，但方法在返回前必须为其赋值；
- `in`：参数在调用前必须已经初始化，方法内只能读取，不能修改。

### ref：双向数据传递

`ref` 侧重于“传递”，方法可以读取并修改调用方的变量。

- **调用前**：变量**必须**初始化；
- **方法内**：可以读取，也可以修改。

**使用场景**：方法需要基于一个已有变量的值进行计算，并直接修改这个变量。

```csharp
public void AddTen(ref int number)
{
    // 可以读取原始值，然后修改它
    number += 10;
}

// 调用处
int value = 5; // 必须赋初值
AddTen(ref value);
Console.WriteLine(value); // 输出：15
```

注意：`ref` 在方法声明和调用处都必须显式书写，代码中哪些变量会被方法修改，一眼就能看出来。

### out：单向输出参数

`out` 侧重于"输出"结果，变量的值由方法内部负责赋值。

- **调用前**：变量**无需**初始化；
- **方法内**：**必须**在方法返回前对该变量赋值，否则会导致编译错误。

**使用场景**：最典型的是 `TryParse` 模式——用 `bool` 返回值表示操作是否成功，同时通过 `out` 参数输出结果。

```csharp
public bool TryParseId(string input, out int id)
{
    if (int.TryParse(input, out id))
    {
        return true; // 成功时，id 已被赋值
    }

    id = 0; // 失败时，也必须给一个默认值
    return false;
}

// 调用处
int result; // 无需初始化
if (TryParseId("123", out result))
{
    Console.WriteLine(result); // 输出：123
}
```

补充：C# 7.0 起支持在调用处内联声明 `out` 变量，上面的调用可以简化为：

```csharp
if (TryParseId("123", out int result))
{
    Console.WriteLine(result); // 输出：123
}
```

### in：只读引用传递

`in` 是 C# 7.2 引入的特性，可以理解为**只读版本的 `ref`**。

- **初始化要求**：与 `ref` 一样，参数在调用前必须已初始化；
- **读写权限**：方法内可以读取参数的值，但禁止任何形式的修改（包括重新赋值和修改成员），编译器会进行严格检查；
- **调用约定**：调用处可以省略 `in` 关键字，`ShowProduct(product)` 与 `ShowProduct(in product)` 等价。

**使用场景**：性能优化。当需要向方法传递一个大型结构体时，`in` 可以避免复制整个结构体的开销，同时通过只读语义保证数据安全，明确表达"方法不会修改传入数据"的意图。

```csharp
struct Product
{
    public int ProductId { get; set; }
    public string ProductName { get; set; }
}

public static void ShowProduct(in Product product)
{
    // product = new Product(); // 编译错误：无法分配到变量 'in Product'，因为它是只读变量
    // product.ProductName = "测试商品"; // 编译错误：不能分配到变量 'in Product' 的成员，因为它是只读变量
    Console.WriteLine($"Id: {product.ProductId}, Name: {product.ProductName}"); // 只能读取
}

// 调用处
var product = new Product { ProductId = 1, ProductName = "示例商品" };
ShowProduct(product); // 调用处可以省略 in 关键字
```

### 常见疑问：引用类型参数与对象生命周期

掌握了三种修饰符的用法后，还有一个常见疑问：**引用类型作为参数传递时，如果调用方的变量离开了作用域，方法里还能访问对象吗？** 答案是肯定的。看下面的例子：

```csharp
Task task;
{
    var list = new List<int> { 1 };
    task = PrintMessage(list); // 传递的是 list 引用的副本
} // list 变量超出作用域，但对象仍被 PrintMessage 持有

Console.WriteLine("END");
await task; // 等待异步任务执行完毕

static async Task PrintMessage(List<int> data)
{
    await Task.Delay(1000);
    Console.WriteLine(data[0]); // 输出：1
}
```

`list` 是引用类型，作为参数传递时实际传递的是**引用的副本**，这个副本仍然指向堆上原来的 `List<int>` 实例。即使 `list` 变量在作用域结束时被销毁，`PrintMessage` 中持有的引用依然有效，因为它指向的是堆上的对象，而不是栈上的 `list` 变量本身。

由于 `PrintMessage` 是异步执行的，并且它持有对 `list` 对象的引用，所以当 `Console.WriteLine("END")` 执行时，对象并不会被垃圾回收——只要还有活跃的引用指向它，它就不会成为回收的候选对象。

因此运行结果是先输出 `END`，一秒后再输出 `1`。这正是“引用类型的值传递”的体现：这里的参数传递与 `ref` 无关，方法拿到的是引用副本，而不是变量本身。

### 最佳实践

`ref` 和 `out` 功能强大，但在现代 C# 开发中应谨慎使用，以保持代码的清晰性和可维护性。

1. **优先考虑返回值或元组（Tuple）**
   当一个方法需要返回多个值时，元组 `(int area, int perimeter)` 通常比 `out` 参数更清晰、更易读。

2. **避免滥用 `ref`**
   `ref` 会产生副作用——方法会直接修改调用者的变量，使代码流程变得难以追踪。仅在确实需要修改原始变量，或为了性能优化（如避免大型结构体复制）时才使用它。

3. **遵循 Try 模式**
   `out` 参数在 `TryXXX` 模式（如 `int.TryParse`）中是标准且推荐的做法，能清晰表达"尝试操作并输出结果"的语义。

另外注意：`async` 方法不能使用 `ref`、`out`、`in` 参数（编译错误 CS1988）。如果异步方法需要返回多个值，元组是最自然的替代方案。

### 参考

- [ref 关键字（Microsoft Learn）](https://learn.microsoft.com/zh-cn/dotnet/csharp/language-reference/keywords/ref)
- [out 关键字（Microsoft Learn）](https://learn.microsoft.com/zh-cn/dotnet/csharp/language-reference/keywords/out)
- [in 关键字（Microsoft Learn）](https://learn.microsoft.com/zh-cn/dotnet/csharp/language-reference/keywords/in)
- [C# 中 ref、out、in 的用法与区别](https://www.cnblogs.com/ittranslator/p/13919691.html)
