---
title: 外观模式
description: C# 外观模式（Facade）详解：为复杂子系统提供统一的高层接口，结合家庭影院示例讲解。
---

# 外观模式（Facade）

外观模式为复杂的子系统提供**一个简单、统一的高层接口**，客户端只与外观交互，不必了解子系统内部错综复杂的协作关系。就像家庭影院：看一部电影要依次开投影、放碟片、调灯光、开音响——外观把这一串操作封装成 `WatchMovie()` 一个按钮。

## 解决什么问题

子系统内部的类往往**彼此协作、调用顺序敏感**。客户端直接面对它们时：

1. **学习成本高**：客户端必须知道每个子系统的 API，以及正确的调用顺序。
2. **耦合度强**：子系统内部调整（换了播放器型号）会波及所有直接调用的客户端代码。
3. **代码重复**：每个客户端都要重复编写相同的调用序列。

外观在客户端与子系统之间加一道门面，把"该做什么、按什么顺序做"封装在门面里。

## 核心结构

| 角色 | 说明 | 示例代码 |
| :--- | :--- | :--- |
| 外观 | 封装子系统的复杂调用序列 | `HomeTheatreFacade` |
| 子系统 | 各自独立的底层类，互不感知外观 | `Dimmer`、`DvdPlayer`、`Dvd` |

## 代码示例

仓库示例 `src/DesignPatterns/FacadePattern` 以家庭影院为背景。**子系统**——灯光调节器：

```csharp
public class Dimmer
{
    internal void Dim(int val)
        => Console.WriteLine(val == 10 ? "Turning Lights On" : $"Dimming lights to {val}");

    internal void Off() => Console.WriteLine("Switching off lights");
}
```

**子系统**——DVD 播放器，支持开关机、插碟、播放、暂停、恢复：

```csharp
public class DvdPlayer
{
    private Dvd _dvd;
    private int _time = 0;

    public void On() => Console.WriteLine("DVD Player powered on");

    public void Insert(Dvd dvd)
    {
        _dvd = dvd;
        Console.WriteLine($"Inserting {dvd.Movie}");
    }

    public void Play() => Console.WriteLine($"Playing {_dvd.Movie}");

    public void Pause()
        => Console.WriteLine($"Pausing at {_time = new Random().Next(_time, _time + 120)}");

    public void Resume() => Console.WriteLine($"Resuming from {_time}");
}
```

**外观**——把"看电影"封装成单个方法，客户端只调用它：

```csharp
public class HomeTheatreFacade
{
    private readonly Dimmer _dimmer;
    private readonly Dvd _dvd;
    private readonly DvdPlayer _dvdPlayer;

    public HomeTheatreFacade(Dimmer dimmer, Dvd dvd, DvdPlayer dvdPlayer)
    {
        _dvd = dvd;
        _dimmer = dimmer;
        _dvdPlayer = dvdPlayer;
    }

    public void WatchMovie()
    {
        _dimmer.Dim(5);        // 调暗灯光
        _dvdPlayer.On();       // 打开播放器
        _dvdPlayer.Insert(_dvd);
        _dvdPlayer.Play();
    }

    public void Pause()
    {
        _dimmer.Dim(10);       // 亮灯
        _dvdPlayer.Pause();
    }

    public void Resume()
    {
        _dimmer.Dim(5);        // 再调暗
        _dvdPlayer.Resume();
    }
}
```

演示入口（完整代码见 `Program.cs`）：

```csharp
var dimmer = new Dimmer();
var dvdPlayer = new DvdPlayer();
var dvd = new Dvd("Gone with the Wind 2 : Electric Bugaloo");
var homeTheater = new HomeTheatreFacade(dimmer, dvd, dvdPlayer);

homeTheater.WatchMovie();
Console.WriteLine();
homeTheater.Pause();
Console.WriteLine();
homeTheater.Resume();
```

运行结果：

```text
Dimming lights to 5
DVD Player powered on
Inserting Gone with the Wind 2 : Electric Bugaloo
Playing Gone with the Wind 2 : Electric Bugaloo

Turning Lights On
Pausing at 47

Dimming lights to 5
Resuming from 47
```

客户端对灯光、播放器的内部逻辑一无所知，只依赖 `WatchMovie` / `Pause` / `Resume` 三个方法。

## 外观模式的两个视角

| 视角 | 理解 |
| :--- | :--- |
| 最小知识原则 | 客户端只与外观对话，不需要认识子系统中的所有类 |
| 分层思想 | 外观是子系统对外的"门面"，子系统内部仍可自由演化 |

注意：外观**没有**封装子系统——子系统仍然可以被直接使用。外观只是提供了一个更友好的入口，并不取代子系统。

## 典型应用场景

- **框架入口**：`DbContext` 把连接管理、跟踪、缓存等复杂机制统一成 `SaveChanges()` 一个方法。
- **SDK 封装**：把多个 HTTP API 调用序列封装成 `PlaceOrder()`、`CancelOrder()` 等业务方法。
- **启动/关闭流程**：复杂应用的初始化、资源释放顺序集中在外观中管理。
- **与适配器的区别**：适配器把接口"翻译"成客户端期望的接口；外观把复杂子系统"简化"成一个更易用的接口。

## 总结

- **简化交互**：把复杂的调用序列收敛到外观的少量方法中。
- **解耦客户端**：子系统内部变化不影响客户端，客户端只依赖外观。
- **不隐藏子系统**：需要精细控制时仍可直接使用子系统类。
- **注意**：外观不应塞入过多业务逻辑，否则会退化为"上帝对象"；它只负责编排，不负责实现。
