---
title: 函数与方法
description: C# 函数与方法详解，包括方法定义与调用、方法重载、扩展方法、参数传递（ref/out/in）、返回值、委托与 Lambda、局部函数、异步方法、虚方法与多态。
---

### C# 函数与方法详解

在 C# 中，"函数"的规范叫法是**方法（Method）**——它是类或结构体中一段可复用的代码块。除了方法，C# 还提供了委托、Lambda、局部函数、异步方法等多种"函数形态"，它们共同构成了 C# 的函数体系。

#### 一、函数的几种形态

在深入细节之前，先整体认识 C# 中的五种函数形态：

| 形态 | 本质 | 典型场景 |
| :--- | :--- | :--- |
| 方法（Method） | 类/结构体的命名成员 | 组织行为逻辑 |
| 委托（Delegate） | 类型安全的函数指针 | 回调、事件、策略注入 |
| Lambda 表达式 | 匿名函数的简洁语法 | LINQ 查询、内联逻辑 |
| 局部函数（Local Function） | 方法内部嵌套的私有函数 | 封装辅助逻辑 |
| 异步方法（async） | 返回 `Task` 的方法 | 网络请求、文件读写等 IO 操作 |

为什么需要函数？因为**复用**：把一段逻辑命名、封装后，可以在任意位置多次调用，避免重复代码。一个合格的函数应当只做一件事，并且通过参数接收输入、通过返回值给出结果。

#### 二、方法基础

方法是最基础的函数形态。一个方法由四部分组成：访问修饰符、返回类型、方法名、参数列表。

**定义与调用**：

```csharp
class Calculator
{
    // 实例方法：接收两个参数，返回它们的和
    public int Add(int a, int b) => a + b;

    // 静态方法：属于类本身，无需实例即可调用
    public static double Square(double x) => x * x;
}

var calc = new Calculator();
Console.WriteLine(calc.Add(2, 3));        // 输出：5
Console.WriteLine(Calculator.Square(4));  // 输出：16
```

**静态方法 vs 实例方法**：

- **实例方法**：属于某个对象，通过 `对象.方法()` 调用，可以访问实例字段
- **静态方法**：属于类本身，通过 `类名.方法()` 调用，不依赖对象状态（如 `Math.Sqrt`、`Console.WriteLine`）

**表达式体成员（C# 6+）**：

当方法体只有一条表达式时，可以用 `=>` 省略 `{ return ...; }` 样板代码。上面的 `Add` 就是表达式体写法，等价于：

```csharp
public int Add(int a, int b)
{
    return a + b;
}
```


#### 三、方法重载

**方法重载（Overload）**：同一个类中，方法名相同、但**参数类型或个数不同**的一组方法。调用时，编译器根据实参自动匹配最合适的一个：

```csharp
public class Calculator
{
    // 基础版本：两个整数相加
    public int Add(int a, int b) => a + b;

    // 重载①：参数个数不同
    public int Add(int a, int b, int c) => a + b + c;

    // 重载②：参数类型不同
    public double Add(double a, double b) => a + b;

    // 重载③：参数类型组合不同
    public double Add(int a, double b) => a + b;
}

var calc = new Calculator();
Console.WriteLine(calc.Add(1, 2));     // 输出：3
Console.WriteLine(calc.Add(1, 2, 3));  // 输出：6
Console.WriteLine(calc.Add(1.5, 2.5)); // 输出：4
Console.WriteLine(calc.Add(1, 2.5));   // 输出：3.5
```

**重载的规则**：

1. 仅靠**返回值**不同不能构成重载（`int Add(int, int)` 与 `double Add(int, int)` 同时存在会编译报错）
2. 仅靠**参数名**不同不能构成重载
3. 编译器依据实参的**类型和个数**选择调用哪个重载；`ref`/`out` 也参与签名区分

**重载 vs 重写**：两者名字相近，但机制完全不同。**重载**是"同名不同参"，编译期根据实参选择；**重写（override）**是"同签名换实现"，运行期根据对象的实际类型选择（见第十章）。

**典型应用**：`Console.WriteLine` 提供了 19 个重载，`int.Parse`、`string.Substring` 等也靠重载提供各种便捷变体。相比**可选参数**，重载更显式，且在二进制库演进时新增重载不会破坏旧调用。

#### 四、扩展方法

**扩展方法（Extension Method）**：允许**不修改原类型**（尤其是第三方库和 BCL 类型），就为其"添加"新方法。它本质上是静态类中的静态方法，靠 `this` 修饰第一个参数声明：

```csharp
// 扩展方法必须放在静态类中
public static class StringExtensions
{
    // this string 表示：为 string 类型扩展一个 WordCount 方法
    public static int WordCount(this string text) =>
        text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
}

var sentence = "Hello C# world";
Console.WriteLine(sentence.WordCount()); // 输出：3
```

调用时 `sentence.WordCount()` 看起来像实例方法，编译器实际会翻译成静态调用 `StringExtensions.WordCount(sentence)`。

**最常见的扩展方法：LINQ**。`Where`、`Select`、`OrderBy` 等都是 `System.Linq.Enumerable` 静态类中的扩展方法，这正是 `IEnumerable<T>` 能"自带"丰富查询能力的原因：

```csharp
public static class EnumerableExtensions
{
    // 为 IEnumerable<int> 扩展：统计偶数个数
    public static int CountEven(this IEnumerable<int> numbers) =>
        numbers.Count(n => n % 2 == 0);
}

int[] nums = { 1, 2, 3, 4, 5, 6 };
Console.WriteLine(nums.CountEven()); // 输出：3
```

**要点**：

1. 扩展方法必须定义在**静态类**中，且本身是静态方法
2. 第一个参数用 `this` 修饰，表示被扩展的类型；调用时该参数不需要传，所以实参个数比声明参数少 1
3. **实例方法优先**：如果类型本身已有同签名的方法，实例方法总是胜出，扩展方法会被忽略
4. 无法访问类型的私有成员，也不能"覆盖"已有方法——它是"锦上添花"，不是"修改源码"
5. 适用场景：为第三方/BCL 类型补充辅助方法、封装重复的链式调用；滥用会让方法来源难查，应保持克制

#### 五、参数传递

参数是函数的输入。C# 默认**按值传递**，并通过 `ref`、`out`、`in` 三个关键字提供引用传递的变体。

| 关键字 | 传入 | 传出 | 调用前需初始化 | 典型场景 |
| :--- | :--- | :--- | :--- | :--- |
| （无） | 复制值/引用 | 否 | 是 | 默认传值 |
| `ref` | 引用 | 可以 | 是 | 修改调用方的变量 |
| `out` | 无需传入有效值 | 必须赋值 | 否 | 多返回值（如 `TryParse`） |
| `in` | 只读引用 | 否 | 是 | 大结构体传参优化 |

**示例**：

```csharp
// ref：方法内修改会同步到调用方
void Increment(ref int value) => value++;

// out：方法负责赋值，调用前无需初始化
bool TryParseInt(string text, out int result)
{
    result = 0;
    return int.TryParse(text, out result);
}

// in：只读引用，禁止修改，避免大结构体复制
void Describe(in Vector3 v) => Console.WriteLine($"({v.X}, {v.Y}, {v.Z})");

int count = 1;
Increment(ref count);
Console.WriteLine(count); // 输出：2

if (TryParseInt("42", out int parsed))
    Console.WriteLine(parsed); // 输出：42

Describe(new Vector3(1, 2, 3)); // 输出：(1, 2, 3)
```

**要点**：

1. `ref` 和 `out` 都是引用传递，区别在于：`ref` 要求调用前已赋值、可进可出；`out` 只出不进，方法内必须赋值
2. `in` 是 C# 7.2+ 的只读引用，主要面向大型 `struct` 的性能优化，`class` 传参本身只是复制引用，无需使用
3. `TryParse` 系列是 `out` 最经典的用法：返回值表示是否成功，`out` 参数带出解析结果

`ref`、`out`、`in` 三种参数的完整对比与常见疑问见[ref、in、out 参数传递](./../types/ref-in-out)。

#### 六、返回值

方法的返回类型决定了它能返回什么。

**可以返回的类型**：

- **值类型**：`int`、`double`、`struct`、`enum` 等
- **引用类型**：`class`、`string`、`interface`、`record` 等
- **`void`**：无返回值，仅执行操作
- **`Task`/`Task<T>`**：异步操作的"容器"（见第九章）

**返回多个值**：现代 C# 推荐使用**元组（Tuple）**，比 `out` 参数更清晰：

```csharp
// 返回命名元组（C# 7+）
(int Sum, int Count) GetStats(int[] numbers)
{
    return (numbers.Sum(), numbers.Length);
}

var stats = GetStats(new[] { 1, 2, 3, 4 });
Console.WriteLine($"总和 {stats.Sum}，共 {stats.Count} 个"); // 输出：总和 10，共 4 个
```

**返回接口而非具体类型**：方法可以返回接口（如 `IEnumerable<T>`），让调用方只依赖契约而不依赖实现，这是解耦和可测试的关键。接口的完整讲解见[接口](./interface)。

**返回迭代器：yield 逐个产出结果**：方法还可以返回 `IEnumerable<T>`，并在方法体内用 `yield return` **逐个产出**结果，这就是**迭代器方法**。与普通方法不同，调用时方法体不会立即执行，而是每次遍历请求元素时才推进到下一个 `yield`：

```csharp
// 迭代器方法：yield 逐个返回偶数
public static IEnumerable<int> GetEvens(int max)
{
    for (int i = 0; i <= max; i += 2)
    {
        yield return i; // 产出当前元素，暂停执行
    }
}

foreach (var n in GetEvens(10))
{
    Console.Write($"{n} "); // 输出：0 2 4 6 8 10
}
```

普通方法用 `return` 返回 `List<T>` 会一次性构建完所有数据；迭代器则按需计算、按需消费，适合惰性生成序列。延迟执行原理、状态机机制与异步迭代器的完整讲解见[迭代器](./../linq/Iterator)。

**返回"函数"本身**：方法的返回类型也可以是委托或 Lambda，让方法"产出"一段可复用的逻辑，这就是**高阶函数**（返回函数的函数）：

```csharp
// 返回 Action 委托：把"打印动作"作为返回值
Action CreateLogger(string prefix) => () => Console.WriteLine($"[{prefix}] 记录日志");

// 返回 Func 委托：Lambda 表达式作为返回值
Func<int, int> CreateAdder(int addend) => x => x + addend;

var log = CreateLogger("INFO");
log(); // 输出：[INFO] 记录日志

var addFive = CreateAdder(5);
Console.WriteLine(addFive(10)); // 输出：15
```

委托、Lambda 与闭包的完整讲解见下一节。

#### 七、委托与 Lambda

委托（Delegate）是 C# 的**类型安全的函数指针**：把"函数"本身当作值来存储、传递和调用。委托的声明、多播、内置泛型委托等完整讲解见[委托](./../delegates/delegate)。

**自定义委托**：

```csharp
// 1. 声明委托类型：定义了函数的签名
public delegate int BinaryOperation(int a, int b);

// 2. 一个普通方法
public static int Add(int a, int b) => a + b;

// 3. 用方法组赋值给委托
BinaryOperation op = Add;
Console.WriteLine(op(3, 4)); // 输出：7
```

**内置泛型委托：`Func` 与 `Action`**：

日常开发中，优先使用 BCL 预定义的内置委托，而不是自定义委托：

```csharp
Func<int, int, int> add = (a, b) => a + b;         // 有返回值，最后一个泛型参数是返回类型
Action<string> log = msg => Console.WriteLine(msg); // 无返回值

Console.WriteLine(add(3, 4)); // 输出：7
log("Hello");                 // 输出：Hello
```

**Lambda 表达式与闭包**：

Lambda 是匿名函数的简洁语法，它**可以捕获外部变量**，这种能力称为闭包（Closure）：

```csharp
// 返回一个"函数"：接收 factor，返回一个把输入乘以 factor 的函数
Func<int, int> CreateMultiplier(int factor) => x => x * factor;

var doubleIt = CreateMultiplier(2);
Console.WriteLine(doubleIt(5)); // 输出：10
```

闭包是 C# 支持**高阶函数**（函数返回函数）的关键。注意：捕获的变量生命周期会被延长（编译器将其提升到隐藏类中），带来堆分配开销，在极高频的循环中需谨慎使用。

委托与事件的完整讲解见[委托](./../delegates/delegate)、[Func 与 Action](./../delegates/func-and-action)、[事件](./../delegates/event)。

#### 八、局部函数

局部函数（Local Function，C# 7+）是定义在方法**内部**的私有函数，用于封装仅被本方法使用的辅助逻辑。

**场景对比**：一个辅助方法只被另一个方法调用，旧写法会把它暴露在整个类中：

```csharp
// ❌ 旧写法：Calculate 只服务于 ProcessData，却污染类作用域
public void ProcessData(IEnumerable<int> data)
{
    var result = Calculate(data);
    Console.WriteLine(result);
}

private int Calculate(IEnumerable<int> items) => items.Sum();
```

```csharp
// ✅ 现代写法：局部函数的作用域仅限于 ProcessData 内部
public void ProcessData(IEnumerable<int> data)
{
    int Calculate(IEnumerable<int> items) => items.Sum();

    var result = Calculate(data);
    Console.WriteLine(result);
}
```

**局部函数 vs Lambda**：

1. **性能**：局部函数不捕获变量时不会生成闭包类，通常比 Lambda 更省内存
2. **能力**：局部函数支持递归、泛型、`yield return`，Lambda 均不支持
3. **可读性**：辅助逻辑就在使用它的地方，无需上下翻找

#### 九、异步方法

异步方法（`async`/`await`）用于网络请求、文件读写等 IO 场景：调用时**不阻塞当前线程**，操作完成后自动恢复执行。

```csharp
// 模拟网络请求：返回 Task<string>
public async Task<string> DownloadAsync(string url)
{
    await Task.Delay(100);          // 模拟 IO 等待，期间线程被释放
    return $"已下载 {url}，长度 {url.Length}";
}

Console.WriteLine("开始下载");
var content = await DownloadAsync("https://example.com");
Console.WriteLine(content); // 输出：已下载 https://example.com，长度 20
```

**要点**：

1. 异步方法返回 `Task`（无结果）或 `Task<T>`（有结果），命名以 `Async` 结尾
2. **禁止 `async void`**：它无法被 `await`、异常无法捕获，会直接导致进程崩溃；仅事件处理器可以使用
3. 编译器会把异步方法改写为**状态机**：执行到 `await` 时若操作未完成，立即返回 `Task` 并释放线程，操作完成后恢复执行——这就是"异步"不阻塞线程的原理

#### 十、虚方法与多态

虚方法（Virtual Method）是面向对象的核心机制：基类声明 `virtual` 方法提供默认实现，派生类用 `override` 重写，从而通过**基类引用**调用**派生类实现**——这就是多态。

**完整示例**：

```csharp
// 基类：声明虚方法，提供默认实现
public class Animal
{
    public virtual void MakeSound() => Console.WriteLine("动物的叫声");
}

// 派生类：重写虚方法，给出各自的实现
public class Dog : Animal
{
    public override void MakeSound() => Console.WriteLine("汪汪！");
}

public class Cat : Animal
{
    public override void MakeSound() => Console.WriteLine("喵喵！");
}

// 多态：基类引用指向派生类对象
Animal myDog = new Dog();
Animal myCat = new Cat();
myDog.MakeSound(); // 输出：汪汪！
myCat.MakeSound(); // 输出：喵喵！

// 遍历时，每个对象调用自己的实现
Animal[] animals = new[] { myDog, myCat };
foreach (Animal animal in animals)
{
    animal.MakeSound(); // 输出：汪汪！/ 喵喵！
}
```

**原理**：`myDog` 的静态类型是 `Animal`，但实际类型是 `Dog`。调用 `MakeSound` 时，运行时根据对象的**实际类型**查找并调用 `Dog` 的重写实现，而不是 `Animal` 的默认实现。这让我们可以编写"面向基类"的通用代码（如 `foreach` 遍历），具体行为由各派生类自行决定。

**虚方法 vs 抽象方法**：

| 特性 | 虚方法（`virtual`） | 抽象方法（`abstract`） |
| :--- | :--- | :--- |
| 是否有默认实现 | 有 | 无（仅声明） |
| 派生类是否必须重写 | 可选 | 必须 |
| 所在类 | 普通类即可 | 必须位于抽象类 |
| 用途 | 提供扩展点，允许覆盖 | 强制契约，必须补齐 |

另外，被 `override` 重写的虚方法还可以用 `sealed override` 冻结，禁止后续子类再重写。抽象类的完整讲解见[抽象类](./abstract)。

#### 十一、最佳实践

1. **短小精悍**：方法只做一件事（单一职责），过长时用局部函数拆分辅助逻辑
2. **表达意图**：单表达式方法用表达式体成员（`=>`），多返回值用元组而非 `out` 堆砌
3. **优先内置委托**：用 `Func`/`Action`，仅在语义需要（如 `Predicate<T>`）时自定义委托
4. **注意闭包开销**：高频循环内避免 Lambda 捕获变量，改用局部函数或普通方法
5. **异步到底**：异步方法永远返回 `Task`/`Task<T>`，绝不使用 `async void`
6. **用虚方法提供扩展点**：基类预留可重写的行为用 `virtual`，强制实现用 `abstract`
7. **返回接口而非具体类型**：对外暴露 `IEnumerable<T>` 等接口，保护内部实现
8. **用重载提供便捷变体**：同一操作的不同参数个数/类型用重载表达，但别仅靠返回值区分
9. **克制使用扩展方法**：只在无法修改原类型（第三方/BCL）时使用，并给出清晰的命名，避免方法来源难查
10. **用迭代器惰性生成序列**：需要按需、分批产出数据时用 `yield` 迭代器方法，避免一次性构建大集合

#### 十二、参考链接

- [方法（C# 编程指南）](https://learn.microsoft.com/zh-cn/dotnet/csharp/programming-guide/classes-and-structs/methods)
- [扩展方法（C# 编程指南）](https://learn.microsoft.com/zh-cn/dotnet/csharp/programming-guide/classes-and-structs/extension-methods)
- [迭代器（C# 编程指南）](https://learn.microsoft.com/zh-cn/dotnet/csharp/programming-guide/concepts/iterators)
- [yield 关键字（C# 参考）](https://learn.microsoft.com/zh-cn/dotnet/csharp/language-reference/statements/yield)
- [virtual 关键字（C# 参考）](https://learn.microsoft.com/zh-cn/dotnet/csharp/language-reference/keywords/virtual)
- [Lambda 表达式（C# 参考）](https://learn.microsoft.com/zh-cn/dotnet/csharp/language-reference/operators/lambda-expressions)
- [async 关键字（C# 参考）](https://learn.microsoft.com/zh-cn/dotnet/csharp/language-reference/keywords/async)
