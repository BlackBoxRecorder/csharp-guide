---
title: 单例模式
description: C# 单例模式（Singleton）详解：确保一个类只有一个实例并提供全局访问点，结合巧克力锅炉示例讲解线程安全的 Lazy 实现。
---

# 单例模式（Singleton）

单例模式确保一个类**在整个进程中只有一个实例**，并提供一个全局访问点。巧克力工厂的锅炉是典型例子：同一台锅炉不能同时被两条生产线占用，全厂只能有一个锅炉实例。

## 解决什么问题

某些资源在系统里**天然只应存在一份**：

1. **重复实例有害**：多个数据库连接池、多个配置管理器、多个日志器，会造成资源浪费或状态不一致。
2. **需要全局共享**：配置、缓存、线程池等组件需要被各模块访问同一份状态。

单例通过**私有构造函数**禁止外部创建，再通过**静态方法**返回唯一实例，把"唯一性"从约定变成强制约束。

## 核心结构

| 角色 | 说明 | 示例代码 |
| :--- | :--- | :--- |
| 单例类 | 持有自身唯一实例的静态字段，构造函数私有 | `ChocolateBoiler` |
| 访问点 | 静态方法返回唯一实例 | `GetInstance()` |

## 代码示例

仓库示例 `src/DesignPatterns/SingletonPattern` 以巧克力锅炉为背景。**单例类**用 `Lazy<T>` 实现线程安全的延迟初始化：

```csharp
public partial class ChocolateBoiler
{
    private static readonly Lazy<ChocolateBoiler> _singleton =
        new Lazy<ChocolateBoiler>(() => new ChocolateBoiler());

    public static ChocolateBoiler GetInstance() => _singleton.Value;

    private Status _boiler;

    private ChocolateBoiler()   // 私有构造函数：外部无法 new
    {
        Console.WriteLine("Starting");
        _boiler = Status.Empty;
    }

    public void Fill()
    {
        if (!IsEmpty) return;
        Console.WriteLine("Filling...");
        _boiler = Status.InProgress;
    }

    public void Boil()
    {
        if (IsBoiled || IsEmpty) return;
        Console.WriteLine("Boiling...");
        _boiler = Status.Boiled;
    }

    public void Drain()
    {
        if (!IsBoiled) return;
        Console.WriteLine("Draining...");
        _boiler = Status.Empty;
    }

    private bool IsEmpty => _boiler == Status.Empty;
    private bool IsBoiled => _boiler == Status.Boiled;
}
```

锅炉状态用私有枚举管理（完整代码见 `Status.cs`）：

```csharp
private enum Status
{
    Empty, InProgress, Boiled
}
```

演示入口（完整代码见 `Program.cs`）：

```csharp
var chocoEggs = ChocolateBoiler.GetInstance();
chocoEggs.Fill();
chocoEggs.Boil();
chocoEggs.Drain();
```

运行结果：

```text
Starting
Filling...
Boiling...
Draining...
```

多次调用 `GetInstance()` 返回的是同一个实例，构造函数只执行一次（`Starting` 只打印一次）。

## C# 中的三种单例实现

| 实现方式 | 线程安全 | 延迟加载 | 说明 |
| :--- | :--- | :--- | :--- |
| `static readonly` 字段 | ✅ | ❌（类型加载时创建） | 写法最简单，进程启动即初始化 |
| `Lazy<T>`（示例采用） | ✅ | ✅ | 首次访问才创建，且线程安全 |
| `lock` 双重检查 | ✅ | ✅ | 手写锁，代码繁琐，不推荐 |

`Lazy<T>` 兼顾线程安全与延迟加载，是 .NET 中实现单例的推荐方式。

## 典型应用场景

- **配置中心**：`ConfigurationManager`、应用级设置读取，全进程共享一份。
- **日志器**：`ILogger` 的全局实例，避免每个类各自创建。
- **连接池 / 线程池**：数据库连接池、`ThreadPool` 天然单例。
- **依赖注入容器**：DI 容器注册的 Singleton 生命周期正是单例模式的框架级实现。

## 总结

- **强制唯一性**：私有构造函数 + 静态访问点，杜绝重复实例。
- **全局访问**：任何地方 `GetInstance()` 即可拿到同一实例。
- **线程安全**：多线程环境下必须保证初始化只发生一次，`Lazy<T>` 是最省心的方案。
- **注意滥用**：单例是全局状态，会隐藏依赖关系、阻碍测试。能用依赖注入（Singleton 生命周期）就不要手写单例；状态很少变化的场景也可用静态类替代。
