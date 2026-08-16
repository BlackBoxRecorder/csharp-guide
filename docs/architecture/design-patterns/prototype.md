---
title: 原型模式
description: C# 原型模式（Prototype）详解：通过复制现有实例创建新对象，避免重复初始化，结合图形克隆示例讲解。
---

# 原型模式（Prototype）

原型模式通过**复制现有实例**来创建新对象，而不是通过 `new` 重新构造。被复制的实例称为"原型"，新对象是原型的一份拷贝。就像复印机：先有一份原稿，需要多少份就直接复印，而不是重新排版打印。

## 解决什么问题

有些对象的**创建成本很高**：构造函数参数复杂、初始化过程漫长、字段很多。每次需要新对象都从头构造，既浪费资源又容易出错。原型模式把"构造"变成"复制"：

1. **省去重复初始化**：复杂的初始状态只需构建一次，之后全部靠克隆。
2. **复制运行时状态**：克隆得到的新对象天然携带原型的当前状态，无需重新计算。
3. **对客户端隐藏构造细节**：客户端只需调用 `Clone()`，无需知道构造函数需要哪些参数。

## 核心结构

| 角色 | 说明 | 示例代码 |
| :--- | :--- | :--- |
| 抽象原型 | 声明克隆方法 | `IFigure : ICloneable` |
| 具体原型 | 实现克隆方法，返回自身类型的副本 | `Rectangle`、`Circle` |

C# 中 `System.ICloneable` 接口即为此而生，它声明了 `object Clone()` 方法。

## 代码示例

仓库示例 `src/DesignPatterns/PrototypePattern` 以几何图形为背景。**抽象原型**继承 `ICloneable` 并定义图形共有的行为：

```csharp
public interface IFigure : ICloneable
{
    void GetInfo();
}
```

**具体原型**实现 `Clone()`。注意克隆返回的是**全新实例**，而不是自身引用：

```csharp
public class Rectangle : IFigure
{
    private readonly int _width;
    private readonly int _height;

    public Rectangle(int w, int h)
    {
        _width = w;
        _height = h;
    }

    public object Clone() => new Rectangle(_width, _height);

    public void GetInfo()
        => Console.WriteLine($"Rectangle height {_height} and width {_width}");
}
```

圆形同样实现克隆：

```csharp
public class Circle : IFigure
{
    private readonly int _radius;

    public Circle(int r) => _radius = r;

    public object Clone() => new Circle(_radius);

    public void GetInfo()
        => Console.WriteLine($"Circle with radius {_radius}");
}
```

演示入口（完整代码见 `Program.cs`）：

```csharp
IFigure figure = new Rectangle(30, 40);
IFigure clonedFigure = (IFigure)figure.Clone();
figure.GetInfo();
clonedFigure.GetInfo();

figure = new Circle(30);
clonedFigure = (IFigure)figure.Clone();
figure.GetInfo();
clonedFigure.GetInfo();
```

运行结果：

```text
Rectangle height 40 and width 30
Rectangle height 40 and width 30
Circle with radius 30
Circle with radius 30
```

## 深拷贝与浅拷贝

克隆的核心难点在于**内部引用类型字段**的处理：

| 拷贝方式 | 行为 | 适用场景 |
| :--- | :--- | :--- |
| 浅拷贝 | 引用字段共享同一对象 | 字段多为值类型，或允许共享 |
| 深拷贝 | 引用字段也一并复制 | 对象包含集合、实体等可变引用 |

`MemberwiseClone()` 只做浅拷贝；需要深拷贝时，可在 `Clone()` 中手动复制引用字段，或借助序列化（`JsonSerializer`、`BinaryFormatter`）实现。示例中的 `Rectangle` 只含 `int` 字段，直接构造新实例即为"深拷贝"。

## 典型应用场景

- **对象池与缓存**：高频创建且构造昂贵的对象（数据库连接配置、大文档模板），以原型为模板快速克隆。
- **游戏实体生成**：同一兵种的不同单位复制自同一个原型对象，各自独立演化状态。
- **表单/文档复制**：Excel 复制工作表、Word 复制节，复制出的内容可独立编辑。

## 总结

- **复制优于构造**：构造昂贵或状态复杂的对象，用克隆替代重复初始化。
- **对客户端透明**：调用方只依赖 `Clone()`，不关心具体类的构造参数。
- **注意克隆深度**：明确浅拷贝与深拷贝的边界，避免克隆后共享可变状态引发 bug。
- **注意 `ICloneable` 的争议**：其 `Clone()` 未明确浅深语义，团队使用时可自定义 `T Clone()` 强类型接口替代。
