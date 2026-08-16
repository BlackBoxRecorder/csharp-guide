---
title: 适配器模式
description: C# 适配器模式（Adapter）详解：将不兼容的接口转换为客户端期望的接口，结合鸭子与火鸡示例讲解。
---

# 适配器模式（Adapter）

适配器模式把**不兼容的接口**转换成**客户端期望的接口**，让原本无法协作的类可以一起工作。就像电源转换插头：插座是两孔，充电器是三孔，一个转换头就解决了问题，双方都不需要改动。

## 解决什么问题

系统已有成熟组件（如火鸡类），但客户端只认另一种接口（如鸭子接口）。强行改造组件会破坏既有代码，甚至组件属于第三方库无法修改。适配器充当中间层：

1. **复用既有类**：不需要修改已有类（开闭原则），通过适配器让它"看起来像"目标接口。
2. **接口统一**：客户端只用一种接口编程，不必感知背后对象的真实类型。

## 核心结构

| 角色 | 说明 | 示例代码 |
| :--- | :--- | :--- |
| 目标接口 | 客户端期望的接口 | `IDuck` |
| 被适配者 | 已有的、接口不兼容的类 | `WildTurkey`（实现 `ITurkey`） |
| 适配器 | 实现目标接口，内部委托给被适配者 | `TurkeyAdapter` |

## 代码示例

仓库示例 `src/DesignPatterns/AdapterPattern` 以"把火鸡当鸭子用"为背景。**目标接口**是鸭子：

```csharp
public interface IDuck
{
    void Quack();   // 鸭子叫
    void Fly();     // 飞
}
```

**被适配者**火鸡有自己的叫声和飞行距离：

```csharp
public interface ITurkey
{
    void Gobble();  // 火鸡叫
    void Fly();
}

public class WildTurkey : ITurkey
{
    public void Gobble() => Console.WriteLine("Gobble Gobble Gobble");
    public void Fly() => Console.WriteLine("Flies 100 Metres");
}
```

**适配器**实现鸭子接口，内部把鸭子方法翻译成火鸡方法——火鸡不会"嘎嘎叫"，就让它"咯咯叫"；火鸡飞得短，就多飞几次凑距离：

```csharp
public class TurkeyAdapter : IDuck
{
    private readonly ITurkey _turkey;

    public TurkeyAdapter(ITurkey turkey)
    {
        _turkey = turkey;
    }

    public void Quack() => _turkey.Gobble();    // 把 Quack 翻译成 Gobble

    public void Fly()
    {
        for (var i = 0; i < 5; i++)             // 火鸡飞 5 次 ≈ 鸭子飞一次
        {
            _turkey.Fly();
            Console.WriteLine("Resting..");
        }
    }
}
```

演示入口（完整代码见 `Program.cs`）——测试方法只认 `IDuck`，适配器让它接受一只"伪装成鸭子的火鸡"：

```csharp
var turkey = new WildTurkey();
var adapter = new TurkeyAdapter(turkey);

Tester(adapter);   // 传入适配器，完全当鸭子用

private static void Tester(IDuck duck)
{
    duck.Fly();
    duck.Quack();
}
```

运行结果：

```text
Flies 100 Metres
Resting..
Flies 100 Metres
Resting..
Flies 100 Metres
Resting..
Flies 100 Metres
Resting..
Flies 100 Metres
Resting..
Gobble Gobble Gobble
```

## 对象适配器与类适配器

| 类型 | 实现方式 | 特点 |
| :--- | :--- | :--- |
| 对象适配器（示例采用） | 组合：适配器持有被适配者引用 | 灵活，任何子类都能适配 |
| 类适配器 | 继承：适配器同时继承目标和被适配者 | C# 单继承限制下较少使用 |

## 典型应用场景

- **第三方库集成**：外部 SDK 的接口与业务接口不一致，用适配器做翻译层，业务代码保持纯净。
- **旧系统兼容**：新接口替换旧接口期间，用适配器让旧实现继续工作。
- **数据源转换**：`DataTable`、`IEnumerable` 等不同数据结构之间的适配（如 LINQ 的 `AsEnumerable()`）。
- **`Stream` 适配**：`StreamReader` 把 `Stream`（字节流）适配为文本读取接口，本质就是适配器。

## 总结

- **接口翻译**：适配器不改动双方，只做方法层面的映射。
- **复用与隔离**：既有代码得以复用，客户端与第三方实现解耦。
- **与外观模式的区别**：适配器改变接口以**匹配客户端**；外观提供新接口以**简化子系统**。
- **注意**：适配层不宜过厚，接口差异过大时考虑重构或桥接模式。
