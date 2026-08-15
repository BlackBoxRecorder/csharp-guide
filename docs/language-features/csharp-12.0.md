---
title: C# 12.0 新特性
description: C# 12.0 版本引入的所有新特性详细说明，包含主构造函数、集合表达式、内联数组、Lambda可选参数等特性的代码示例和最佳实践
---

# C# 12.0 新特性

C# 12 作为 .NET 8 的一部分，引入了一系列旨在提升代码可读性、简化编程模型、增强类型系统以及提高开发效率的新特性。这些特性覆盖了从对象初始化、集合操作到内存管理和 lambda 表达式等多个方面，为开发者提供了更现代化、更高效的编程工具。本章节将详细介绍 C# 12 中引入的各项新特性，包括其基本概念、简单用法以及相关的代码示例，以便开发者能够快速理解并应用这些新功能。

### 5.1 主构造函数 (Primary constructors)

**主构造函数是 C# 12 中一个备受关注的特性，它最初在 C# 9 中针对记录类型（record types）引入，现在 C# 12 将此功能扩展到了所有类和结构体（class and struct）**。这一特性的主要目的是允许开发者在类或结构体的声明中直接定义构造函数参数，这些参数在整个类或结构体的主体范围内都处于作用域内，从而简化了代码，减少了样板代码的编写，特别是在需要初始化成员字段或属性的场景下。主构造函数的参数最常见的用途包括：作为基类构造函数（`base()`）调用的参数、初始化成员字段或属性，以及在实例成员中引用构造函数参数。

在 C# 12 中，声明主构造函数的方式是将参数列表直接放在类名或结构体名后面的括号中。例如，定义一个 `Person` 类，并为其添加 `name` 和 `age` 作为主构造函数参数，然后可以在类体内直接使用这些参数来初始化属性：

```csharp
public class Person(string name, int age)
{
    public string Name => name;
    public int Age => age;
}
```

在这个例子中，`name` 和 `age` 是主构造函数的参数，它们被用来初始化 `Name` 和 `Age` 这两个只读属性。这种方式相比传统的构造函数定义，代码更加简洁。传统的写法需要显式声明字段并赋值，而主构造函数则避免了这种冗余。值得注意的是，编译器仅在记录类型（`record class` 或 `record struct`）中为主构造函数参数生成公共属性。对于非记录类的类和结构体，编译器通常不会自动生成属性，除非开发者显式定义。

主构造函数的参数作用域是整个类体，这意味着它们可以在类的任何方法、属性或字段初始化器中使用。如果类中还定义了其他的显式构造函数，那么这些显式构造函数必须通过 `this()` 语法来调用主构造函数，以确保所有主构造函数参数都被明确赋值。例如：

```csharp
public class MyController(IHttpClientFactory clientFactory)
{
    public MyController() : this(CreateDefaultClientFactory())
    { }
    // ... 其他成员
}
```

在这个例子中，无参数的构造函数通过 `this(CreateDefaultClientFactory())` 调用了主构造函数，确保了 `clientFactory` 参数被初始化。此外，主构造函数也支持参数绑定到类的属性或字段，并且可以设置访问修饰符，例如 `init` 访问器，使得属性可以在对象初始化器中设置。

主构造函数在处理依赖注入场景时也显示出其优势，可以简化服务注入的代码。例如，在 ASP.NET Core 控制器中，可以直接将依赖的服务作为主构造函数的参数：

```csharp
class MyController(IHttpClientFactory clientFactory)
{
  private readonly IHttpClientFactory _clientFactory = clientFactory;
  // ... 其他成员
}
```

或者，也可以使用主构造函数参数来初始化一个属性：

```csharp
class MyController(IHttpClientFactory clientFactory)
{
    private IHttpClientFactory ClientFactory { get; } = clientFactory;
    // ... 其他成员
}
```

如果参数被传递给基类并且被基类体捕获，派生类不应在其自身体内使用该构造函数参数，因为这会导致基类和派生类各自存储参数的一个副本，编译器会对此发出 CS9107 警告。在这种情况下，派生类应该通过基类暴露的字段或属性来访问该参数。

主构造函数也支持添加特性（attributes）。默认情况下，放在类或结构体声明前的特性会作用于类型本身。如果要将特性应用于主构造函数，可以使用 `method:` 前缀。例如：

```csharp
[method: Obsolete("Use another constructor.")]
class MyController(IHttpClientFactory clientFactory)
{
    // ... 类体
}
```

这个特性表明该主构造函数已过时，并建议使用其他构造函数。主构造函数为 C# 开发者提供了一种更简洁、更具表达力的方式来定义和初始化对象，尤其适用于 DTO（数据传输对象）的设计和实现，能够提升代码的简洁性和可维护性。

### 5.2 集合表达式 (Collection expressions)

**集合表达式是 C# 12 引入的一项旨在简化集合初始化的新语法特性**。它提供了一种更简洁、更具表现力的方式来直接指定集合的元素，从而统一了不同类型集合的初始化语法，并减少了样板代码。在 C# 12 之前，初始化不同类型的集合（如数组、`List<T>`、`Span<T>`）需要使用不同的语法。集合表达式通过使用方括号 `[]` 来包围集合元素，使得初始化过程更加直观和统一。

集合表达式支持多种集合类型，包括数组类型（例如 `int[]`）、`System.Span<T>` 和 `System.ReadOnlySpan<T>`，以及支持常见集合初始值设定项的类型，如 `System.Collections.Generic.List<T>`。例如，初始化一个整数数组、一个字符串列表和一个字符 `Span` 可以分别写成：

```csharp
int[] a = [1, 2, 3, 4, 5];
List<string> b = ["one", "two", "three"];
Span<char> c = ['a', 'b', 'c', 'd', 'e'];
```

这种语法不仅简洁，而且提高了代码的可读性。集合表达式还支持嵌套，例如初始化一个二维数组或一个包含列表的列表：

```csharp
int[][] twoD = [[1, 2, 3], [4, 5, 6], [7, 8, 9]];
List<List<string>> names3 = [["one", "two"], ["three", "four"]];
```

一个非常强大的特性是使用展开运算符（spread operator）`..` 来合并多个集合中的元素到一个新的集合中。这使得从一个或多个现有集合动态创建新集合变得非常方便：

```csharp
int[] row0 = [1, 2, 3];
int[] row1 = [4, 5, 6];
int[] row2 = [7, 8, 9];
int[][] twoDFromVariables = [row0, row1, row2];

List<string> names1 = ["one", "two"];
List<string> names2 = ["three", "four"];
List<string> allNames = [..names1, ..names2, "five"];
```

在上面的 `allNames` 示例中，新的列表包含了 `names1` 和 `names2` 中的所有元素，并在末尾添加了字符串 "five"。编译器会根据目标类型和初始化表达式来优化生成的代码。

对于自定义类型，也可以通过实现一个特定的 `Create` 方法并应用 `System.Runtime.CompilerServices.CollectionBuilderAttribute` 特性来选择加入集合表达式的支持。`CollectionBuilderAttribute` 指向一个接受 `ReadOnlySpan<T>` 并从中创建集合类型的方法。例如，为一个自定义的 `LineBuffer` 类型启用集合表达式支持：

```csharp
[CollectionBuilder(typeof(LineBufferBuilder), "Create")]
public class LineBuffer : IEnumerable<char>
{
    private readonly char[] _buffer = new char[80];
    public LineBuffer(ReadOnlySpan<char> buffer) { /* ... */ }
    // ... IEnumerable<char> 的实现
}

internal static class LineBufferBuilder
{
    internal static LineBuffer Create(ReadOnlySpan<char> values) => new LineBuffer(values);
}

// 使用集合表达式创建 LineBuffer 实例
LineBuffer line = ['H', 'e', 'l', 'l', 'o', ' ', 'W', 'o', 'r', 'l', 'd', '!'];
```

通过这种方式，自定义类型也可以像内置集合类型一样使用简洁的集合表达式进行初始化。集合表达式不仅可以在变量初始化时使用，还可以作为参数传递给接受集合类型的方法，从而显著提升代码的可读性和编写效率。

### 5.3 内联数组 (Inline arrays)

**内联数组是 C# 12 引入的一项用于优化内存管理和提高性能的特性，它允许开发者在结构体（struct）类型中创建固定大小的数组**。这种特性在处理缓冲区等需要高效内存操作的场景下非常有用，例如在游戏开发中的图形渲染、数据处理中的缓冲区操作等。与传统的堆分配数组不同，内联数组是值类型，通常分配在栈上，这可以减少垃圾回收的压力并可能提高访问速度。

声明内联数组需要使用 `System.Runtime.CompilerServices.InlineArray` 特性，并指定数组的长度。例如，声明一个包含 10 个整数的内联数组：

```csharp
[System.Runtime.CompilerServices.InlineArray(10)]
public struct IntBuffer
{
    private int _element; // 这个字段是模板，实际数组元素通过索引器访问
}
```

在这个例子中，`IntBuffer` 结构体内部实际上包含了一个固定大小的连续内存块，可以存储 10 个 `int` 类型的元素。`_element` 字段在这里更像是一个占位符，编译器会根据 `InlineArray` 特性生成实际的存储布局和索引访问逻辑。需要注意的是，内联数组本身不直接提供 `Length` 属性，也不能直接使用 LINQ 方法，因为它不是一个标准的集合类型。

使用内联数组时，可以像使用普通数组一样通过索引来访问和修改元素：

```csharp
var myArray = new IntBuffer();
for (int i = 0; i < 10; i++) // 注意：这里硬编码了长度 10，实际使用时需要确保不越界
{
    myArray[i] = i; // 通过索引赋值
}

foreach (var item in myArray) // 支持 foreach 迭代
{
    Console.WriteLine(item);
}
```

内联数组的主要优势在于其内存布局的紧凑性和可能的栈上分配，这对于性能敏感的应用非常重要。然而，由于其固定大小的特性，它更适用于那些已知数据量上限且对性能有严格要求的场景。微软指出，内联数组主要由 .NET 运行时团队和库作者使用，以改进库的性能，普通开发者可能更多地是消费这些库提供的功能，而不是直接创建大量的内联数组。尽管如此，了解内联数组的原理和用法，对于理解底层性能优化和与这些高性能库的交互仍然是有益的。

### 5.4 Lambda 表达式的可选参数 (Optional parameters in lambda expressions)

C# 12 增强了 lambda 表达式的灵活性，**允许在 lambda 表达式的参数列表中定义可选参数及其默认值**。这一特性使得 lambda 表达式的行为更接近于传统的方法和局部函数，允许开发者在调用 lambda 表达式时省略部分参数，从而简化调用代码并提高代码的可重用性。在 C# 12 之前，虽然可以通过 `DefaultParameterValue` 特性实现类似效果，但新的语法更加简洁直观。

定义带有可选参数的 lambda 表达式时，只需在参数声明中为其指定默认值即可。例如，定义一个执行加法操作的 lambda 表达式，其中第二个加数 `b` 是可选的，并默认值为 1：

```csharp
var addWithDefault = (int a, int b = 1) => a + b;
```

调用这个 lambda 表达式时，可以选择只提供一个参数，此时第二个参数将使用默认值 1；也可以提供两个参数，此时第二个参数将使用传入的值：

```csharp
int result1 = addWithDefault(5);      // 结果为 6 (5 + 1)
int result2 = addWithDefault(5, 2);   // 结果为 7 (5 + 2)
```

可选参数必须放在 lambda 表达式参数列表的末尾，并且其规则与方法和局部函数中的可选参数规则相同。如果 lambda 表达式只有一个输入参数，并且该参数是可选的，或者所有参数都是可选的，则调用时可以不提供任何参数，但括号仍然是必需的（除非使用空括号 `()` 调用无参 lambda）：

```csharp
var greet = (string name = "World") => $"Hello, {name}!";
Console.WriteLine(greet());         // 输出: Hello, World!
Console.WriteLine(greet("Alice"));  // 输出: Hello, Alice!
```

这个特性在处理需要默认行为或部分参数化的场景时非常有用。例如，在创建事件处理程序或回调函数时，可以通过可选参数提供一些默认配置，使得调用方在不需要定制化行为时可以简化调用。它减少了为不同参数组合编写多个重载 lambda 表达式的需要，使得代码更加简洁和易于维护。与方法和局部函数一样，为 lambda 参数提供默认值可以增强其灵活性，使其能够适应更多样化的逻辑需求。

### 5.5 `ref readonly` 参数 (`ref readonly` parameters)

C# 12 引入了 **`ref readonly` 参数，这是一个旨在提高代码安全性和性能的特性，它允许方法接收对值类型实例的只读引用**。通过将参数声明为 `ref readonly`，开发者可以确保方法内部不会修改传入的变量值，同时避免了值类型的复制开销，这对于大型结构体尤其重要。当传递一个引用类型的变量作为 `ref readonly` 参数时，编译器会确保该引用的目标对象（如果对象本身是可变的）不会被该方法重新赋值，但对象内部状态是否可变取决于对象自身的实现。

使用 `ref readonly` 参数可以在不牺牲性能的前提下，明确表达方法的意图——即该方法不会修改传入的参数。这对于编写健壮的库代码和 API 非常有用，因为它可以防止意外的数据修改。例如，考虑一个交换两个整数的方法，如果我们只想读取它们的值进行比较或其他操作，而不想实际交换它们，可以使用 `ref readonly`：

```csharp
public void PrintValues(ref readonly int x, ref readonly int y)
{
    Console.WriteLine($"x: {x}, y: {y}");
    // x = 10; // 这会导致编译错误，因为 x 是只读的
    // y = 20; // 这会导致编译错误，因为 y 是只读的
}

int a = 5, b = 10;
PrintValues(ref a, ref b); // 注意：调用时仍然使用 ref，但方法内部是只读的
```

在上面的例子中，`PrintValues` 方法接收两个 `ref readonly int` 参数。这意味着 `x` 和 `y` 是对外部 `int` 变量的只读引用。任何尝试在方法内部修改 `x` 或 `y` 的操作都会导致编译错误。调用该方法时，仍然需要使用 `ref` 关键字传递参数，以明确表示传递的是引用。`ref readonly` 参数对于确保数据在传递过程中的不变性非常有用，尤其是在涉及大型结构体或需要高性能的场景下，因为它避免了不必要的内存复制，同时保证了数据的安全性。

### 5.6 `using` 别名指令可以别名任何类型 (Alias any type)

C# 12 扩展了 `using` 别名指令的功能，**使其不仅可以为命名空间或命名类型（如类、结构体、接口、委托和枚举）创建别名，现在还可以为几乎所有其他类型创建别名**。这包括元组类型、数组类型、指针类型以及其他不安全类型。这一增强使得开发者能够为复杂的或冗长的类型定义赋予更简洁、更具描述性的名称，从而提高代码的可读性和可维护性。

使用 `using` 别名指令可以为各种类型创建语义化的别名。例如，可以为可空值类型、数组类型甚至元组类型定义别名：

```csharp
using OptionalFloat = float?; // 别名可空浮点数
using PathOfPoints = int[];   // 别名整数数组
using DatabaseInt = int?;     // 别名可空整数，可能用于数据库交互
using Measurement = (string Units, int Distance); // 别名元组类型，并指定元素名称
```

定义了这些别名之后，就可以在代码中使用这些别名来代替原始的类型名称，就像使用任何其他类型一样：

```csharp
public void LogMeasurement(Measurement data)
{
    Console.WriteLine($"Measured {data.Distance} {data.Units}");
}

OptionalFloat temp = 36.6f;
PathOfPoints route = [10, 20, 30, 40];
DatabaseInt userId = GetUserIdFromDatabase();
LogMeasurement(("meters", 100));
```

为类型创建别名有助于抽象实际使用的类型，并为那些可能令人困惑或过长的泛型名称提供友好的替代。例如，如果一个复杂的泛型类型在代码中多次出现，为其定义一个简短的别名可以显著提高代码的清晰度。需要注意的是，`using` 别名指令不支持为可空的引用类型创建别名（例如 `string?`），但支持可空的值类型（例如 `int?`）。这个特性使得代码的组织和阅读更加方便，尤其是在处理具有复杂类型签名的 API 或数据结构时。

### 5.7 实验特性 (Experimental attribute)

C# 12 引入了对实验性特性的更明确支持，通常通过 **`System.Diagnostics.CodeAnalysis.ExperimentalAttribute` 特性来标记**。当一个类型、方法、属性或程序集被标记为 `[Experimental]` 时，它表示该功能仍处于试验阶段，其 API 或行为在未来的版本中可能会发生更改，甚至可能被完全移除。编译器会对使用这些实验性特性的代码发出警告或错误，以提醒开发者注意其不稳定性。

使用 `[Experimental]` 特性时，通常会指定一个诊断 ID，该 ID 用于标识与该实验性特性相关的编译器消息。这有助于开发者理解为什么某些代码会触发警告或错误，并可以查阅相关文档了解该实验性特性的具体情况。例如，如果一个名为 `MyExperimentalFeature` 的特性被标记为实验性的，并且其诊断 ID 为 "MYEXP001"，那么编译器会在任何使用 `MyExperimentalFeature` 的代码处生成一个与 "MYEXP001" 相关的警告或错误。开发者可以通过在项目文件中设置 `<NoWarn>` 或 `<WarningsAsErrors>` 来控制这些诊断信息的行为，或者通过 `#pragma warning disable` 指令来临时禁用特定代码行的警告。这个特性为语言设计者和库作者提供了一种机制，可以在早期向开发者社区展示和收集关于新功能的反馈，同时明确告知其潜在的不稳定性，从而帮助开发者做出明智的技术选型。

### 5.8 拦截器 (Interceptors) (预览特性)

**拦截器（Interceptors）是 C# 12 中引入的一项预览特性，它允许开发者在编译时拦截对特定方法或属性的调用，并将其重定向到自定义的实现**。这个特性主要面向高级场景，如代码生成、AOP（面向切面编程）、 mocking 框架或性能分析工具的开发。通过使用拦截器，开发者可以在不修改原始代码的情况下，动态地改变程序的行为。拦截器通过在编译时修改调用站点的 IL 代码来实现，而不是在运行时通过反射或动态代理。

要使用拦截器，开发者需要定义一个拦截器类，并在其中编写拦截逻辑。然后，通过特定的特性或约定来标记哪些方法调用应该被拦截。当编译器遇到这些标记的调用时，它会将调用重定向到拦截器类中相应的方法。例如，一个拦截器可以记录方法调用的参数和返回值，或者在方法执行前后注入额外的逻辑。由于拦截器是预览特性，并且直接操作 IL 代码，因此它需要开发者对编译过程和 IL 有较深的理解，并且使用时需要谨慎，以避免引入难以调试的问题。这个特性为构建强大的元编程工具和框架开辟了新的可能性，但它也可能被滥用，导致代码难以理解和维护。因此，它通常建议由经验丰富的开发者或库作者在特定场景下使用。
