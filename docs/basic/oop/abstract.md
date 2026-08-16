---
title: 抽象类
description: C# 抽象类和密封类详解，包括 abstract 和 sealed 关键字、抽象成员、密封方法、sealed record 及与接口的选择。
---

### C# 抽象类和密封类详解

在面向对象编程中，抽象类和密封类是控制继承层次的两端：`abstract` 强制派生类实现契约，`sealed` 冻结继承链。为什么需要这两种看似相反的机制？本文以抽象类为主线，逐一拆解。

#### 一、为什么需要抽象类？

假设要描述多种动物，每种都会发出声音，但叫声各不相同。用普通基类无法强制每个子类实现"发声"；每个子类独立定义又丢失了公共逻辑。抽象类正是为此而生：**既提供公共实现，又强制派生类补齐差异**。

#### 二、抽象类详解

抽象类是**不能被实例化**的特殊类，专门作为基类使用。

**核心特性**：

```csharp
abstract class Animal
{
    // 抽象属性（必须被重写）
    public abstract string Species { get; }

    // 抽象方法（无方法体）
    public abstract void MakeSound();

    // 抽象事件（必须被重写）
    public abstract event Action? OnFed;

    // 抽象索引器（必须被重写）
    public abstract string this[int index] { get; set; }

    // 普通方法（已有实现，直接复用）
    public void Breathe() => Console.WriteLine("呼吸中...");

    // 虚方法（可选重写）
    public virtual void Move() => Console.WriteLine("移动");
}
```

**使用规则**：

1. 通过 `abstract` 关键字声明，**不能创建实例**：`Animal a = new Animal();` ❌
2. 必须被继承才能使用，派生类**必须重写所有抽象成员**
3. 抽象成员只声明不实现：方法无方法体，属性、事件、索引器无访问器实现
4. 可同时包含抽象成员（无实现）、普通成员（有实现）、虚成员（可选重写）
5. 可以有构造函数（通常为 `protected`），用于初始化派生类共享的字段

**实战示例**：

```csharp
class Dog : Animal
{
    // 索引器背后的数据：宠物名字列表
    private readonly List<string> _names = new() { "旺财" };

    public override string Species => "犬科";

    public override void MakeSound() => Console.WriteLine("汪汪！");

    // 实现抽象事件：订阅"喂食"通知
    public override event Action? OnFed;

    // 实现抽象索引器：按编号访问宠物名字
    public override string this[int index]
    {
        get => _names[index];
        set => _names[index] = value;
    }

    // 触发事件：喂食时通知订阅者
    public void Feed()
    {
        Console.WriteLine("投喂狗粮...");
        OnFed?.Invoke();
    }

    // 可选：重写虚方法
    public override void Move() => Console.WriteLine("四足奔跑");
}

// 使用（基类引用 + 多态）
Animal myPet = new Dog();
myPet.MakeSound(); // 输出：汪汪！

// 订阅事件后喂食
myPet.OnFed += () => Console.WriteLine("狗狗吃饱了！");
((Dog)myPet).Feed(); // 输出：投喂狗粮... / 狗狗吃饱了！

// 通过索引器按编号访问名字
Dog dog = (Dog)myPet;
Console.WriteLine(dog[0]); // 输出：旺财
dog[0] = "大黄";           // 修改名字
Console.WriteLine(dog[0]); // 输出：大黄
```

**事件与索引器说明**：

- **事件**：`override event` 实现抽象事件后，只能在声明类内部触发（`OnFed?.Invoke()`），外部只能订阅和退订，保证了封装性；`?.` 确保没有订阅者时不报错
- **索引器**：`this[int index]` 的 `get` 读取、`set` 写入，内部由 `List<string>` 存储，下标越界会抛出 `ArgumentOutOfRangeException`
- **强转说明**：`Feed()` 和索引器是 `Dog` 独有的成员，通过基类引用访问时需要先转换为 `Dog` 类型

**抽象类实现接口**：

抽象类实现接口时不必实现全部成员，可以把剩余成员声明为抽象成员，"转交"给派生类：

```csharp
interface IPayable
{
    decimal CalculateSalary();
    void PrintReceipt();
}

abstract class Employee : IPayable
{
    // 接口成员转交派生类实现
    public abstract decimal CalculateSalary();

    // 接口成员在基类直接实现
    public void PrintReceipt() => Console.WriteLine("打印工资条");
}

class Developer : Employee
{
    public override decimal CalculateSalary() => 15000m;
}
```

**构造函数**：

抽象类不能被实例化，构造函数只服务于派生类的初始化链：

```csharp
abstract class Device
{
    protected readonly string Id;

    protected Device(string id) => Id = id;
}

class Sensor : Device
{
    public Sensor(string id) : base(id) { }
}
```

#### 三、密封类详解

密封类与抽象类相反，是**禁止被继承**的类，用于终止继承链。

**核心特性**：

```csharp
sealed class UltimateCalculator
{
    public int Add(int a, int b) => a + b;
    public int Multiply(int a, int b) => a * b;
}

// 尝试继承将报错
// class ScientificCalculator : UltimateCalculator { } ❌
```

**使用规则**：

1. 通过 `sealed` 关键字声明，**不能被任何类继承**
2. 可以被实例化（与抽象类相反）
3. 所有成员均为完全实现，不含抽象成员
4. 常用于工具类（如 `Math`）、安全敏感类、不希望被修改的核心类

**密封方法**：

对于从基类重写而来的虚方法，可以用 `sealed override` 冻结，禁止后续子类再重写：

```csharp
class Vehicle
{
    public virtual void StartEngine() { /* 启动逻辑 */ }
}

class Car : Vehicle
{
    // 重写并冻结，禁止后续子类再重写
    public sealed override void StartEngine() { /* 带安全检测的启动 */ }
}

class SportsCar : Car
{
    // 错误：无法重写密封方法
    // public override void StartEngine() { } ❌
}
```

**sealed record（C# 9+）**：

record 同样可用 `sealed` 修饰，禁止其他类型继承：

```csharp
sealed record Point(int X, int Y);

// 错误：record 是密封的
// record Point3D : Point { } ❌
```

**性能优势**：

密封类没有派生类，JIT 编译器可跳过虚方法表查找、直接内联方法调用，在热路径（高频调用）代码中效果显著。

#### 四、抽象类 vs 密封类

| 特性 | 抽象类 (Abstract) | 密封类 (Sealed) |
|---|---|---|
| 关键字 | `abstract` | `sealed` |
| 实例化 | 禁止 | 允许 |
| 继承性 | 必须被继承 | 禁止被继承 |
| 成员类型 | 可含未实现的抽象成员 | 所有成员均需完全实现 |
| 继承链位置 | 顶部或中间 | 最底部 |
| 典型用途 | 基类模板、多态基础 | 工具类、安全类、不可变对象 |
| .NET 框架示例 | `Stream`、`DbCommand` | `String`、`Math` |

#### 五、混合使用案例

二者不是互斥的，抽象类定义契约、密封类完成最终实现，是常见组合：

```csharp
abstract class PaymentProcessor
{
    public abstract void Process(decimal amount);
}

sealed class PayPalProcessor : PaymentProcessor
{
    public override void Process(decimal amount)
    {
        // PayPal 专有实现
        Console.WriteLine($"通过 PayPal 支付 {amount} 美元");
    }
}

// 多态使用
PaymentProcessor processor = new PayPalProcessor();
processor.Process(99.99m); // 输出：通过 PayPal 支付 99.99 美元
```

**设计策略**：

1. 顶层定义抽象类作为契约
2. 中间层可选提供部分实现
3. 终端用密封类完成最终实现
4. 必要时在中间层用 `sealed` 方法冻结关键逻辑

#### 六、抽象类 vs 接口：如何选择

抽象类和接口都能定义契约，两者的语义不同：**抽象类描述"是什么"（is-a），接口描述"能做什么"（can-do）**。

选择要点：

- 需要共享字段、构造函数或部分公共实现 → 选抽象类
- 需要多继承，或只约束"具备某种能力" → 选接口
- 拿不准时优先用接口；需要共享实现时再升级为抽象类
- 二者可结合：抽象类实现接口，先落实公共逻辑，再把差异点留给子类

> 接口的完整讲解见 [接口](./interface)。

#### 七、最佳实践

1. **抽象类的适用场景**：
   - 多个相关类需要共享逻辑模板
   - 需要强制派生类实现特定行为（契约式设计）
   - 构建多态体系基础框架
2. **密封类的适用场景**：
   - 功能完整无需扩展的工具类
   - 安全敏感的核心类（如加密模块）
   - 频繁实例化的性能关键类
   - 防止第三方程序集篡改
3. **组合优先于继承**：

   ```csharp
   // 替代继承的更好方案
   class CustomCalculator
   {
       private readonly UltimateCalculator _core = new();
       public int Calculate(int x, int y) => _core.Multiply(x, y);
   }
   ```

4. **设计原则**：
   - 开放封闭原则：抽象类对扩展开放
   - 稳定性法则：密封类对修改封闭
   - 里氏替换原则：抽象类确保派生类可替换基类

#### 八、总结

- **抽象类**：不能实例化、必须被继承、强制派生类实现抽象成员，是"可扩展的骨架"
- **密封类**：不能被继承、可实例化、冻结实现，是"不可变的终端"
- **选择口诀**：抽象类回答"是什么"，接口回答"能做什么"
- **组合用法**：顶层抽象类定义契约，终端密封类冻结实现

**参考链接**：

- [abstract 关键字（C# 参考）](https://learn.microsoft.com/zh-cn/dotnet/csharp/language-reference/keywords/abstract)
- [sealed 关键字（C# 参考）](https://learn.microsoft.com/zh-cn/dotnet/csharp/language-reference/keywords/sealed)
