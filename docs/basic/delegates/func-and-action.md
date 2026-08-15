---
title: Func 和 Action
description: C# 中 Func 和 Action 委托的详细用法，包括泛型参数、Lambda 表达式、实际应用场景。
---

Func 和 Action 是两种预定义的泛型委托类型，用于简化委托的声明和使用。

- Action：无返回值，适用于“做某事”的操作。

- Func：有返回值，适用于“计算并返回结果”的逻辑。

两者都是泛型委托，最多支持 16 个参数，极大减少了代码中自定义委托的数量。

配合 Lambda 表达式，可以写出非常简洁、表达力强的代码。

:::note
什么是委托，参考[这里]()
什么是 Lambda 表达式，参考[这里]()
:::

## Func

`Func` 表示一个有返回值的方法。最后一个是返回值类型，前面是输入参数类型（最多 16 个输入参数）。

`Func<T, TResult>` 表示接受一个类型为 `T` 的参数并返回类型为 `TResult` 的结果的委托。
`Func<T1, T2, TResult>` 表示接受两个不同类型的参数 `T1` 和 `T2` 并返回类型为 `TResult` 的结果的委托。
以此类推，最多可以有16个输入参数。

```csharp
public delegate TResult Func<out TResult>();                     // 无参数，返回 TResult
public delegate TResult Func<in T, out TResult>(T arg);         // 1 个参数，返回 TResult
public delegate TResult Func<in T1, in T2, out TResult>(T1 arg1, T2 arg2);
// ... 直到 Func<T1,...,T16,TResult>
```

### Func 使用示例

#### 无参数 Func

```csharp
Func<DateTime> getCurrentTime = () => DateTime.Now;
DateTime now = getCurrentTime();
```

#### 带参数 Func

```csharp
Func<int, int, int> add = (a, b) => a + b;
int sum = add(3, 5); // 8
```

#### 方法组转换

```csharp
int Double(int x) => x * 2;
Func<int, int> doubler = Double;
int result = doubler(5); // 10
```

#### 封装计算逻辑

```csharp
public static int Compute(int a, int b, Func<int, int, int> operation)
{
    return operation(a, b);
}

// 调用
int sum = Compute(3, 4, (x, y) => x + y);
int product = Compute(3, 4, (x, y) => x * y);
```

#### 使用 Func 作为方法参数

假设我们有一个方法需要对列表中的每个元素应用一个函数，然后返回处理后的列表：

```csharp
List<int> ApplyFunctionToList(List<int> list, Func<int, int> function)
{
    List<int> newList = new List<int>();
    foreach (var item in list)
    {
        newList.Add(function(item));
    }
    return newList;
}

// 使用示例
List<int> numbers = new List<int> { 1, 2, 3, 4 };
List<int> squaredNumbers = ApplyFunctionToList(numbers, x => x * x);
// squaredNumbers 现在是 { 1, 4, 9, 16 }
```

### 示例 5: 使用 Func 与 LINQ

`Func` 常用于 LINQ 查询表达式中，例如 `Where`、`Select` 等方法：

```csharp
List<int> numbers = new List<int> { 1, 2, 3, 4, 5, 6 };

// 使用 Func<int, bool> 作为 Where 方法的参数
var evenNumbers = numbers.Where(number => number % 2 == 0).ToList();
// evenNumbers 现在是 { 2, 4, 6 }
```

### 常见使用场景

- **LINQ 操作**：`Where`, `Select`, `OrderBy` 等大量使用 `Func` 。
- **延迟执行**：封装一个计算逻辑，稍后调用。
- **依赖注入/策略模式**：传递算法或行为。

```csharp
var names = new[] { "Alice", "Bob", "Charlie" };
var longNames = names.Where(name => name.Length > 3);
var upperNames = names.Select(name => name.ToUpper());
// Where 的签名：Func<TSource, bool> predicate
// Select 的签名：Func<TSource, TResult> selector
```

## Action

`Action` 表示一个 **没有返回值** 的方法（返回类型为 `void`）。它可以有 0 到 16 个输入参数。

```csharp
public delegate void Action();                 // 无参数
public delegate void Action<in T>(T obj);      // 1 个参数
public delegate void Action<in T1, in T2>(T1 arg1, T2 arg2); // 2 个参数
// ... 直到 Action<T1,...,T16>
```

### 使用示例

#### 无参数 Action

```csharp
Action sayHello = () => Console.WriteLine("Hello!");
sayHello(); // 输出 Hello!
```

#### 带参数 Action

```csharp
Action<string, int> log = (msg, code) => Console.WriteLine($"[{code}] {msg}");
log("File not found", 404);
```

#### 方法组转换

```csharp
void PrintMessage(string msg) => Console.WriteLine(msg);
Action<string> print = PrintMessage;
print("Hello from method group");
```

### 使用 Action 作为方法参数

假设我们有一个方法需要对列表中的每个元素执行一个操作（例如打印）：

```csharp
void ProcessList(List<int> list, Action<int> action)
{
    foreach (var item in list)
    {
        action(item);
    }
}

// 使用示例
List<int> numbers = new List<int> { 1, 2, 3, 4 };
ProcessList(numbers, x => Console.WriteLine(x));
// 将输出:
// 1
// 2
// 3
// 4
```

### 使用 Action 与 LINQ

虽然 `Action` 不能直接用于 LINQ 查询表达式（因为 LINQ 方法通常期望 `Func` 以处理返回值），但我们可以在 `ForEach` 方法中使用 `Action` 来遍历集合并对每个元素执行操作：

```csharp
List<int> numbers = new List<int> { 1, 2, 3, 4 };

// 使用 Action<int> 作为 ForEach 方法的参数
numbers.ForEach(number => Console.WriteLine(number * number));
// 将输出:
// 1
// 4
// 9
// 16
```

#### 异步回调

```csharp
public void DoWorkAsync(Action<string> onComplete)
{
    Task.Run(() =>
    {
        Thread.Sleep(1000);
        onComplete("Work done");
    });
}

DoWorkAsync(msg => Console.WriteLine(msg));
```

### 常见场景

- **回调函数**：执行某些操作后调用，不需要返回值。
- **`List<T>.ForEach`**：对每个元素执行一个 Action。
- **`Task.Run`**：启动一个无返回值的后台任务。
- **事件处理器**（通常用 `EventHandler`，但 `Action` 也可用于简单场景）。

## 进阶：协变与逆变

- **协变 (`out`)**：`Func` 的返回值支持协变，可以返回派生类型。
- **逆变 (`in`)**：`Action` 和 `Func` 的参数支持逆变，可以接受基类型。

```csharp
// 协变示例
Func<object> getString = () => "Hello";  // 返回 string 赋值给 Func<object>，合法
object obj = getString();

// 逆变示例
Action<object> printObj = (obj) => Console.WriteLine(obj);
Action<string> printStr = printObj;      // 因为 string 是 object 的子类
printStr("Hello");
```

`Action<in T>` 的逆变意味着你可以将 `Action<object>` 赋值给 `Action<string>`，因为任何期望 `object` 的方法都可以接受 `string` 参数。

```csharp
Action<object> broadAction = (obj) => Console.WriteLine(obj?.ToString());
Action<string> narrowAction = broadAction;  // 逆变允许
narrowAction("Hello");  // 正常调用
```
