---
title: 抽象类
description: C# 抽象类和密封类详解，包括 abstract 和 sealed 关键字的使用、设计模式中的应用。
---

### C# 抽象类和密封类详解

在面向对象编程中，抽象类和密封类是两种用于控制类继承层次的关键机制。它们分别通过`abstract`和`sealed`关键字实现，代表了继承关系的两个极端方向。

#### 一、抽象类 (Abstract Class)

抽象类是**不能被实例化**的特殊类，专门设计作为基类使用，用于定义派生类必须实现的契约和部分公共逻辑。

​**核心特性**:

```cs
abstract class Animal
{
    // 抽象属性（必须被重写）
    public abstract string Species { get; }
    
    // 抽象方法（无方法体）
    public abstract void MakeSound();
    
    // 普通方法（已有实现）
    public void Breathe() => Console.WriteLine("呼吸中...");
    
    // 虚方法（可选重写）
    public virtual void Move() => Console.WriteLine("移动");
}
```

​**使用规则**:

1. 通过`abstract`关键字声明
2. ​**不能创建实例**​：`Animal a = new Animal();` ❌
3. 必须被继承才能使用
4. 可以包含：
    - 抽象成员（无实现）
    - 普通成员（有实现）
    - 虚成员（可选重写）
5. 派生类**必须重写所有抽象成员**​
6. 可以包含构造函数（用于派生类初始化）

​**实战示例**​：

```cs
class Dog : Animal
{
    // 必须实现抽象属性
    public override string Species => "犬科";
    
    // 必须实现抽象方法
    public override void MakeSound() => Console.WriteLine("汪汪！");
    
    // 选择重写虚方法
    public override void Move() => Console.WriteLine("四足奔跑");
}

// 使用
Animal myPet = new Dog();
myPet.MakeSound();  // 输出：汪汪！
```

​**设计目的**​：

- 强制派生类实现特定行为（契约式设计）
- 封装通用逻辑（如`Breathe()`方法）
- 构建多态基础框架

#### 二、密封类 (Sealed Class)

密封类是**禁止被继承**的类，用于终止继承链，防止类被进一步扩展。

​**核心特性**​：

```cs
sealed class UltimateCalculator
{
    public int Add(int a, int b) => a + b;
    public int Multiply(int a, int b) => a * b;
}

// 尝试继承将报错
// class ScientificCalculator : UltimateCalculator {} ❌
```

​**使用规则**​：

1. 通过`sealed`关键字声明
2. 可以被实例化（与抽象类相反）
3. ​**不能被任何类继承**​
4. 常见于：
    - 工具类（如`Math`）
    - 安全敏感类
    - 不希望被修改的核心类
5. 所有方法默认为`sealed`（不可重写）

​**密封方法**​：

```cs
class Vehicle
{
    public virtual void StartEngine() {...}
}

class Car : Vehicle
{
    // 禁止后续子类重写此方法
    public sealed override void StartEngine() {...}
}

class SportsCar : Car
{
    // 错误：无法重写密封方法
    // public override void StartEngine() {...} ❌
}
```

​**设计目的**​：

1. ​**安全性**​：防止关键类被篡改（如加密类）
2. ​**性能优化**​：运行时跳过虚表查找
3. ​**设计控制**​：冻结类功能（如.NET中的`String`类）
4. ​**防止误扩展**​：终结设计完整的继承链

#### 三、抽象类 vs 密封类

|特性|抽象类 (Abstract)|密封类 (Sealed)|
|---|---|---|
|​**关键字**​|`abstract`|`sealed`|
|​**实例化**​|禁止|允许|
|​**继承性**​|必须被继承|禁止被继承|
|​**成员类型**​|可含未实现的抽象成员|所有成员均需完全实现|
|​**典型用途**​|基类模板、多态基础|工具类、安全类、不可变对象|
|​**继承链位置**​|顶部或中间|最底部|
|.NET 框架示例|`Stream`, `DbCommand`|`String`, `Math`|

#### 四、混合使用案例

```cs
abstract class PaymentProcessor
{
    public abstract void Process(decimal amount);
}

sealed class PayPalProcessor : PaymentProcessor
{
    public override void Process(decimal amount)
    {
        // PayPal专有实现
        Console.WriteLine($"通过PayPal支付{amount}美元");
    }
}

// 多态使用
PaymentProcessor processor = new PayPalProcessor();
processor.Process(99.99m);
```

​**设计策略**​：

1. 顶层定义抽象类作为契约
2. 中间层可选提供部分实现
3. 终端用密封类完成最终实现
4. 必要时在中间层使用`sealed`方法冻结关键逻辑

#### 五、最佳实践指南

1. ​**抽象类使用场景**​：

    - 多个相关类需要共享逻辑模板
    - 需要强制派生类实现特定行为
    - 构建多态体系基础框架
2. ​**密封类使用场景**​：

    - 功能完整无需扩展的工具类
    - 安全敏感的核心类（如加密模块）
    - 频繁实例化的性能关键类
    - 防止第三方程序集篡改
3. ​**组合优先**​：

    ```cs
    // 替代继承的更好方案
    class CustomCalculator
    {
        private readonly UltimateCalculator _core = new();
        public int Calculate(int x, int y) => _core.Multiply(x, y);
    }
    ```

4. ​**设计原则**​：

    - ​**开放封闭原则**​：抽象类支持扩展开放
    - ​**稳定性法则**​：密封类保护核心代码封闭
    - ​**里氏替换原则**​：抽象类确保派生类可替换基类

#### 六、重要技术细节

1. ​**抽象类的构造函数**​：

    ```cs
    abstract class Device
    {
        protected string ID;
        
        // 基类构造函数被派生类调用
        protected Device(string id) => ID = id;
    }
    ```

2. ​**密封类的性能优势**​：

    - JIT编译器可能内联密封类的方法
    - 避免虚方法表查找开销
    - 特别在热路径代码中效果显著
3. ​**兼容性设计**​：

    ```cs
    public abstract class CollectionBase
    {
        // 留出扩展点
        protected virtual void OnAdd(object item) {...}
    }
    ```

通过合理运用抽象类和密封类，可以在保证扩展性的同时控制继承层次，提升代码的安全性和性能。抽象类构建"可扩展的骨架"，密封类创建"不可变的终端"，二者协同形成健壮的面向对象架构。
