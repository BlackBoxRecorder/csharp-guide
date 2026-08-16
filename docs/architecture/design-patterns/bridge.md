---
title: 桥接模式
description: C# 桥接模式（Bridge）详解：将抽象与实现分离，使两者可以独立变化，结合武器与附魔示例讲解。
---

# 桥接模式（Bridge）

桥接模式把**抽象部分**与**实现部分**分离，让两者沿着各自的维度独立变化。就像武器系统：武器（剑、锤）是一维，附魔（飞行、噬魂）是另一维，任何武器都可以搭配任何附魔，两者自由组合而互不干扰。

## 解决什么问题

当一个类存在**两个独立变化的维度**时，用继承组合会产生"类爆炸"。武器有 2 种 × 附魔有 2 种，用继承需要 4 个类（`FlyingSword`、`SoulEatingSword`、`FlyingHammer`、`SoulEatingHammer`）；维度越多，类越多。桥接用**组合代替继承**：

1. **消除类爆炸**：抽象持有实现的引用，组合数量从乘法变成加法（2 + 2 = 4 个类，而不是 4 + N 个）。
2. **独立扩展**：新增一种武器或一种附魔，互不影响对方。

## 核心结构

| 角色 | 说明 | 示例代码 |
| :--- | :--- | :--- |
| 抽象 | 定义高层接口，持有实现引用 | `IWeapon` |
| 细化抽象 | 抽象的不同变体 | `Sword`、`Hammer` |
| 实现 | 定义实现维度的接口 | `IEnchantment` |
| 具体实现 | 实现维度的具体类 | `FlyingEnchantment`、`SoulEatingEnchantment` |

## 代码示例

仓库示例 `src/DesignPatterns/BridgePattern` 以游戏武器为背景。**实现维度**——附魔接口：

```csharp
public interface IEnchantment
{
    void OnActivate();   // 激活附魔
    void Apply();        // 攻击时生效
    void OnDeactivate(); // 解除附魔
}
```

两种具体附魔：

```csharp
public class FlyingEnchantment : IEnchantment
{
    public void OnActivate() => Console.WriteLine("The item begins to glow faintly.");
    public void Apply() => Console.WriteLine("The item flies and strikes the enemies finally returning to owner's hand.");
    public void OnDeactivate() => Console.WriteLine("The item's glow fades.");
}

public class SoulEatingEnchantment : IEnchantment
{
    public void OnActivate() => Console.WriteLine("The item spreads bloodlust.");
    public void Apply() => Console.WriteLine("The item eats the soul of enemies.");
    public void OnDeactivate() => Console.WriteLine("Bloodlust slowly disappears.");
}
```

**抽象维度**——武器接口，`Swing` 时委托给附魔：

```csharp
public interface IWeapon
{
    void Wield();
    void Swing();
    void Unwield();
    IEnchantment GetEnchantment();
}
```

**细化抽象**——剑与锤各自实现武器行为，但附魔逻辑完全委托给 `IEnchantment`：

```csharp
public class Sword : IWeapon
{
    private readonly IEnchantment _enchantment;

    public Sword(IEnchantment enchantment) => _enchantment = enchantment;

    public void Wield()
    {
        Console.WriteLine("The sword is wielded.");
        _enchantment.OnActivate();
    }

    public void Swing()
    {
        Console.WriteLine("The sword is swinged.");
        _enchantment.Apply();
    }

    public void Unwield()
    {
        Console.WriteLine("The sword is unwielded.");
        _enchantment.OnDeactivate();
    }

    public IEnchantment GetEnchantment() => _enchantment;
}
```

`Hammer` 结构相同，只是输出文案不同。演示入口（完整代码见 `Program.cs`）：

```csharp
IWeapon sword = new Sword(new FlyingEnchantment());
sword.Wield();
sword.Swing();
sword.Unwield();

IWeapon hammer = new Hammer(new SoulEatingEnchantment());
hammer.Wield();
hammer.Swing();
hammer.Unwield();
```

运行结果（节选）：

```text
The sword is wielded.
The item begins to glow faintly.
The sword is swinged.
The item flies and strikes the enemies finally returning to owner's hand.
The sword is unwielded.
The item's glow fades.
...
```

任意组合都只需一行：`new Sword(new SoulEatingEnchantment())`、`new Hammer(new FlyingEnchantment())`，无需新增类。

## 与适配器模式的区别

| 对比项 | 桥接模式 | 适配器模式 |
| :--- | :--- | :--- |
| 目的 | 分离抽象与实现，让两者独立演化 | 转换接口，让不兼容的类协作 |
| 设计时机 | 提前设计，结构灵活 | 事后补救，接口不匹配 |
| 变化维度 | 两个维度对称，自由组合 | 一个方向：适配到目标接口 |

## 典型应用场景

- **跨平台渲染**：形状（圆形、矩形）× 渲染器（OpenGL、DirectX），任意组合。
- **消息发送**：消息类型（文本、图片）× 渠道（邮件、短信、微信），`IMessageChannel` 注入 `Message`。
- **数据库驱动**：`DbConnection`（抽象）与 `DbProviderFactory`（实现）分离，可对照 [抽象工厂模式](./abstract-factory)。
- **UI 主题系统**：控件 × 主题（浅色、深色）自由搭配。

## 总结

- **组合优于继承**：用持有引用代替多层继承，消除组合爆炸。
- **独立演化**：抽象与实现各改各的，互不牵连。
- **开闭原则**：新增武器或新增附魔，都只加新类。
- **注意**：桥接适合"两个维度都频繁变化"的场景；只有一个维度会变化时，引入桥接反而增加复杂度。
