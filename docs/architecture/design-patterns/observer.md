---
title: 观察者模式
description: C# 观察者模式（Observer）详解：定义一对多依赖，状态变化时自动通知订阅者，结合天气广播示例讲解。
---

# 观察者模式（Observer）

观察者模式定义**一对多**的依赖关系：当一个对象（主题）的状态发生变化时，所有依赖它的对象（观察者）都会**自动收到通知**。就像气象台：天气一变，所有订阅了天气服务的屏幕立即更新显示，气象台不需要知道屏幕有多少块、是谁家的。

## 解决什么问题

状态变化需要同步给多个接收方时，最直接的写法是让主题持有所有接收方的引用，并逐个调用：

1. **主题与接收方强耦合**：每新增一种接收方（温度屏、湿度屏），主题代码都要改。
2. **扩展不灵活**：接收方不能动态订阅/退订，只能写死在主题里。
3. **通知逻辑侵入**：主题的核心业务里到处是"通知别人"的代码。

观察者让主题只维护一个**观察者列表**，状态变化时统一广播。谁订阅、谁退订，主题完全不关心。

## 核心结构

| 角色 | 说明 | 示例代码 |
| :--- | :--- | :--- |
| 主题接口 | 提供订阅、退订、通知方法 | `IObservable<Weather>` |
| 具体主题 | 维护观察者列表，状态变化时广播 | `WeatherSupplier` |
| 观察者接口 | 声明接收通知的方法 | `IObserver<Weather>` |
| 具体观察者 | 实现收到通知后的更新逻辑 | `WeatherMonitor` |

.NET 标准库内置了 `IObservable<T>` / `IObserver<T>` 接口，示例直接基于它实现。

## 代码示例

仓库示例 `src/DesignPatterns/ObserverPattern` 以气象站为背景。**数据对象**：

```csharp
public class Weather
{
    public double Pressure { get; }
    public double Humidity { get; }
    public double Temperature { get; }

    public Weather(double humd, double pres, double temp)
    {
        Temperature = temp;
        Pressure = pres;
        Humidity = humd;
    }
}
```

**具体主题**——气象数据提供方，维护观察者列表；`Subscribe` 返回一个可退订句柄（`IDisposable`）：

```csharp
public class WeatherSupplier : IObservable<Weather>
{
    private readonly List<IObserver<Weather>> _observers;

    public WeatherSupplier()
    {
        _observers = new List<IObserver<Weather>>();
    }

    public IDisposable Subscribe(IObserver<Weather> observer)
    {
        if (!_observers.Contains(observer))
        {
            _observers.Add(observer);
        }
        return new Unsubscriber<Weather>(_observers, observer);
    }

    public void WeatherConditions(double temp = 0, double humd = 0, double pres = 0)
    {
        var conditions = new Weather(humd, pres, temp);
        foreach (var observer in _observers)   // 广播给所有订阅者
        {
            observer.OnNext(conditions);
        }
    }
}
```

**退订句柄**——调用 `Dispose` 时把自己从列表移除：

```csharp
public class Unsubscriber<TWeather> : IDisposable
{
    private readonly List<IObserver<TWeather>> _observers;
    private readonly IObserver<TWeather> _observer;

    internal Unsubscriber(List<IObserver<TWeather>> observers, IObserver<TWeather> observer)
    {
        _observers = observers;
        _observer = observer;
    }

    public void Dispose()
    {
        if (_observers.Contains(_observer))
            _observers.Remove(_observer);
    }
}
```

**具体观察者**——监控屏按订阅时声明的关注项（名称中含 T 显示温度、P 显示气压、H 显示湿度）更新显示：

```csharp
public class WeatherMonitor : IObserver<Weather>
{
    private IDisposable _cancellation;
    private readonly string _name;

    public WeatherMonitor(string name) => _name = name;

    public void Subscribe(WeatherSupplier provider)
        => _cancellation = provider.Subscribe(this);

    public void Unsubscribe() => _cancellation.Dispose();

    public void OnCompleted() { }

    public void OnError(Exception error)
        => Console.WriteLine("Error has occured");

    public void OnNext(Weather value)
    {
        Console.Write(_name);
        if (_name.Contains("T"))
            Console.Write($"| Temperature : {value.Temperature} Celsius |");
        if (_name.Contains("P"))
            Console.Write($"| Pressure : {value.Pressure} atm |");
        if (_name.Contains("H"))
            Console.Write($"| Humidity : {value.Humidity * 100} % |");
        Console.WriteLine();
    }
}
```

演示入口（完整代码见 `Program.cs`）——注意观察者**何时订阅**，决定了它何时开始接收通知：

```csharp
var provider = new WeatherSupplier();
var observer1 = new WeatherMonitor("TP");
var observer2 = new WeatherMonitor("H");

provider.WeatherConditions(32.0, 0.05, 1.5);   // 尚无订阅者，无人接收

observer1.Subscribe(provider);                  // observer1 开始订阅
provider.WeatherConditions(33.5, 0.04, 1.7);   // 只有 observer1 收到

observer2.Subscribe(provider);                  // observer2 加入
provider.WeatherConditions(37.5, 0.07, 1.2);   // 两个观察者都收到
```

运行结果：

```text
TP| Temperature : 33.5 Celsius || Pressure : 1.7 atm |
TP| Temperature : 37.5 Celsius || Pressure : 1.2 atm |
H| Humidity : 7 % |
```

第一次发布没有观察者，静默无人接收；第二次只有 `TP` 收到；第三次两人都收到——订阅关系完全是动态的。

## 观察者模式与事件

C# 中观察者模式更常用的形式是 **event 关键字**：

```csharp
public class WeatherSupplier
{
    public event Action<Weather>? WeatherChanged;

    public void WeatherConditions(double temp, double humd, double pres)
        => WeatherChanged?.Invoke(new Weather(humd, pres, temp));
}

// 订阅：weatherSupplier.WeatherChanged += monitor.Update;
// 退订：weatherSupplier.WeatherChanged -= monitor.Update;
```

`event` 自动处理多播（+=/-=）与空判断，日常开发比手写 `IObservable<T>` 更简洁。`IObservable<T>` 的优势在于返回退订句柄、支持推送完成/错误通知，适合更正式的场景。

## 典型应用场景

- **UI 数据绑定**：`INotifyPropertyChanged` 让属性变化自动刷新界面，是观察者模式在 WPF/WinForms 中的框架级应用。
- **事件驱动架构**：发布/订阅消息（`event`、`Channel`、消息队列）。
- **游戏状态同步**：血量变化通知血条、伤害飘字、成就系统等多个接收方。
- **日志与监控**：程序集加载、异常发生时广播给多个监听器。

## 总结

- **一对多自动通知**：主题状态变化，所有订阅者自动收到更新。
- **动态订阅**：订阅/退订在运行时自由控制，主题无感知。
- **松耦合**：主题只认识观察者接口，双方可独立演化。
- **注意**：观察者执行缓慢会拖慢通知线程；忘记退订会造成内存泄漏（事件持有引用导致对象无法回收）。
