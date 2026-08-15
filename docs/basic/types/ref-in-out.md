---
title: ref、in、out 参数传递
description: C# 中 ref、in、out 参数修饰符的详细对比与使用场景，包括值传递与引用传递的区别。
---

在 C# 中，ref、out 和 in 是用于参数传递的修饰符，它们都实现了“按引用传递”。意味着方法接收到的不是变量的副本，而是变量本身的内存地址。虽然它们都按引用传递，但在使用规则和应用场景上还是有一些区别。

| 修饰符 | 调用前是否需初始化 | 方法内是否可读 | 方法内是否可写 | 主要用途 |
| :--- | :--- | :--- | :--- | :--- |
| `ref` | 必须 | 可以 | 可以 | 双向数据传递，方法内外共享变量 |
| `out` | 不必 | 不可以 | 必须 | 从方法中返回多个值 |
| `in` | 必须 | 可以 | 禁止 | 高性能场景下传递大型结构体 |

### 值传递和引用传递

在了解 ref、out 的作用前，我们要知道什么是值传递，什么是引用传递。

**值传递 (Pass by Value)**:
当使用值传递时，方法接收的是实际参数的副本。这意味着如果方法内部修改了这个参数，这些更改不会影响到原始变量。

- 对于值类型（如int, float, struct等），传递的是该类型的值的副本。
- 对于引用类型，传递的是对象引用的副本。虽然你传递的是引用的副本，但这个副本仍然指向堆上的同一个对象。如果你通过这个引用来修改对象的状态（例如添加或移除列表中的元素），那么这些修改会影响原始对象。

**引用传递 (Pass by Reference)**:
使用`ref`或`out`关键字可以实现引用传递。这允许方法直接操作调用方的变量本身，而不仅仅是它的副本。

- `ref`要求传递给方法的参数必须已经初始化。
- `out`则不需要参数预先初始化，但是方法必须保证在退出之前为`out`参数赋值。

### ref 和 out 详解

**使用场景：** 当你需要方法基于一个已有变量的值进行计算并修改它时。

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

#### out：单向输出参数

`out` 关键字侧重于“输出”结果。它表示变量的值将由方法内部负责赋值。

- **调用前**：变量**无需**被初始化。
- **方法内**：**必须**在方法返回前对该变量进行赋值，否则会导致编译错误。

**使用场景：** 最典型的用法是 `TryParse` 模式，用于返回多个值或表示操作是否成功的同时输出结果。

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

### 最佳实践

虽然 `ref` 和 `out` 功能强大，但在现代 C# 开发中应谨慎使用，以保持代码的清晰性和可维护性。

1. **优先考虑返回值或元组 (Tuple)**
    当一个方法需要返回多个值时，使用元组 `(int area, int perimeter)` 通常比使用 `out` 参数更清晰、更易读。

2. **避免滥用 ref**
    `ref` 会产生副作用，即方法会直接修改调用者的变量，这可能使代码流程变得难以追踪。仅在确实需要修改原始变量或为了性能优化（如避免大型结构体复制）时才使用它。

3. **遵循 Try 模式**
    `out` 参数在 `TryXXX` 模式（如 `int.TryParse`）中是标准且推荐的做法，用于清晰地表达“尝试操作并输出结果”的语义。

### in 关键字

in 关键字是 C# 7.2 引入的特性，用于实现“只读引用传递”，可以认为是只读版本的 ref。

**初始化要求：**
与 ref 一样，传递给 in 参数的变量在调用前必须已初始化。

**读写权限：**
方法内部可以读取参数的值，但绝对禁止对其进行任何形式的修改（包括字段、属性等）。编译器会在编译时进行严格检查。

**使用场景：**

in 的主要用途是性能优化。当需要向方法传递一个大型、不可变的结构体时，使用 in 可以避免复制整个结构体带来的巨大开销，同时又通过只读语义保证了数据安全。明确表达方法不会修改传入数据的意图，增强代码可读性。

```csharp
struct Product
{
    public int ProductId { get; set; }
    public string ProductName { get; set; }
}

public static void Modify(in Product product)
{
    //product = new Product(); //错误 无法分配到 变量 'in Product'，因为它是只读变量
    //product.ProductName = "测试商品"; //不能分配到 变量 'in Product' 的成员，因为它是只读变量
    Console.WriteLine($"Id: {product.ProductId}, Name: {product.ProductName}"); // OK
}

```

### 关于方法参数作用域

关于以下代码：

```csharp
void Main()
{
 {
  var list = new List<int>();
  list.Add(1);
  var task = PrintMessage(list);
  Task.Run(async () =>
  {
   await task;
  });
 }
 Console.WriteLine($"END");
}

private async Task PrintMessage(List<int> list)
{
 await Task.Delay(1000);
 Console.WriteLine(list[0]);
}
```

在这段代码中，`list`是一个引用类型（`List<int>`），所以当它作为参数传递给`PrintMessage`方法时，实际上传递的是`list`对象的引用的副本。这个副本仍然指向原来的`List<int>`实例。即使`list`变量在`Main`方法的作用域结束时被销毁，`PrintMessage`方法中持有的引用仍然有效，因为它指向的是堆上的对象，而不是栈上的`list`变量本身。

由于`PrintMessage`任务是在`Task.Run`中异步执行的，并且它持有对`list`的引用，所以在`Console.WriteLine("END")`被执行时，`list`对象并不会立即被垃圾回收，因为还有活跃的引用指向它。只有当所有对`list`的引用都不再存在时，它才会成为垃圾回收的候选对象。

因此，在这段代码中，`PrintMessage`方法仍然可以访问并打印`list`中的值，尽管`Main`方法中的`list`变量已经超出了其作用域。

---

### 参考

<https://www.cnblogs.com/ittranslator/p/13919691.html>
