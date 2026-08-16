---
title: 代理模式
description: C# 代理模式（Proxy）详解：为对象提供替身以控制访问，结合图片懒加载示例讲解。
---

# 代理模式（Proxy）

代理模式为另一个对象提供一个**替身（代理）**，由替身控制对原对象的访问。调用方与代理交互，代理在合适的时机才真正触达原对象。就像文档中的图片：先占个位，滚动到附近时才真正从磁盘加载。

## 解决什么问题

直接访问某些对象是有代价的：

1. **创建昂贵**：对象构造需要加载资源（读磁盘、连网络），但调用方可能根本用不到它。
2. **需要附加控制**：访问需要鉴权、记日志、限流，但原对象不应关心这些横切关注点。

代理在不改动原对象的前提下，拦截访问并附加控制逻辑，按需创建原对象（延迟加载）、拦截非法访问（保护代理）、缓存结果（缓存代理）等。

## 核心结构

| 角色 | 说明 | 示例代码 |
| :--- | :--- | :--- |
| 抽象主题 | 代理与原对象共同实现的接口 | `Image` |
| 真实主题 | 真正执行业务的对象 | `RealImage` |
| 代理 | 持有真实主题引用，控制访问 | `ProxyImage` |

## 代码示例

仓库示例 `src/DesignPatterns/ProxyPattern` 以图片懒加载为背景。**抽象主题**：

```csharp
public interface Image
{
    void Display();
}
```

**真实主题**——构造时就从磁盘加载图片，代价昂贵：

```csharp
public class RealImage : Image
{
    private readonly string _fileName;

    public RealImage(string fileName)
    {
        _fileName = fileName;
        LoadFromDisk(_fileName);
    }

    public void Display() => Console.WriteLine($"Displaying {_fileName}");

    private void LoadFromDisk(string fileName)
        => Console.WriteLine($"Loading {fileName}");
}
```

**代理**——构造时只记录文件名，**真正加载延迟到首次显示**；第二次显示时直接复用已加载的实例：

```csharp
public class ProxyImage : Image
{
    private RealImage? _realImage;
    private readonly string _fileName;

    public ProxyImage(string fileName) => _fileName = fileName;

    public void Display()
    {
        if (_realImage == null)
        {
            _realImage = new RealImage(_fileName);   // 首次访问才真正加载
        }
        _realImage.Display();
    }
}
```

演示入口（完整代码见 `Program.cs`）：

```csharp
Image image = new ProxyImage("testImage.jpg");

// 首次显示：从磁盘加载
image.Display();
Console.WriteLine("");

// 再次显示：不再加载，直接复用
image.Display();
```

运行结果：

```text
Loading testImage.jpg
Displaying testImage.jpg

Displaying testImage.jpg
```

注意 `Loading` 只打印了一次——代理实现了延迟加载与结果复用。

## 代理的常见变体

| 变体 | 职责 | 示例 |
| :--- | :--- | :--- |
| 虚拟代理（示例） | 延迟创建昂贵对象 | 图片懒加载 |
| 保护代理 | 控制访问权限 | 只读视图、权限校验 |
| 远程代理 | 屏蔽远程调用细节 | `RPC`、`gRPC` 客户端代理 |
| 缓存代理 | 缓存计算结果 | 结果集缓存 |
| 日志代理 | 记录访问行为 | 审计日志 |

## 与装饰器模式的区别

| 对比项 | 代理模式 | 装饰器模式 |
| :--- | :--- | :--- |
| 目的 | 控制访问（何时、能否、如何访问） | 动态添加职责 |
| 关注点 | 访问管理 | 功能增强 |
| 对原对象 | 尽量保持透明 | 显式叠加行为 |

两者结构相似（都持有原对象引用），区别在意图：代理是"门卫"，装饰器是"化妆师"。

## 典型应用场景

- **EF Core 延迟加载**：导航属性的代理在首次访问时才查询数据库。
- **WCF / gRPC 客户端**：`ChannelFactory` 生成的客户端代理屏蔽网络细节。
- **`Lazy<T>` 与 `RealProxy`**：`Lazy<T>` 本质是虚拟代理；`DispatchProxy` 可在运行时动态生成代理类。
- **日志与鉴权中间件**：对服务调用统一附加审计、限流，业务代码无感知。

## 总结

- **控制访问时机**：昂贵对象延迟到真正需要时才创建（本示例）。
- **附加控制逻辑**：权限、日志、缓存等横切关注点集中到代理，原对象保持纯粹。
- **对客户端透明**：客户端面对接口编程，无感知代理的存在。
- **注意**：代理层会引入额外间接调用，简单场景直接用原对象即可。
