---
title: 事件
description: C# 事件（Event）机制详解，包括事件声明、订阅、触发、事件访问器、自定义事件参数等。
---

**事件（Event）** 是实现 **发布-订阅（Publisher-Subscriber）模式** 的核心机制，它允许对象在特定情况发生时通知其他对象，是实现组件间 **松耦合通信** 的关键。`EventHandler` 是 .NET 中预定义的委托类型，专门用于表示不生成数据的事件的事件处理程序方法。

## 事件（Event）基本概念

### 什么是事件？

事件是一种特殊的委托，基于委托并为委托提供一个 **发布/订阅的机制** 。简单来说，事件就是一件事情发生，然后通知外界此事情发生了。

**特点：**

- 只能在声明事件的类内部触发（保证封装性）
- 订阅者只能通过 `+=` 和 `-=` 来订阅和取消订阅
- 支持多播（多个订阅者可以订阅同一个事件）
- 事件是委托的安全包装，防止外部直接调用或赋值

## EventHandler 委托

### EventHandler 的定义

`EventHandler` 是 .NET Framework 中预定义的委托类型。

**标准签名：**

```csharp
public delegate void EventHandler(object sender, EventArgs e);
```

- **sender 参数**：`object` 类型，代表触发事件的对象（事件源）
- **e 参数**：`EventArgs` 类型，提供与事件相关的数据。对于不需要传递额外信息的基本事件，可以使用 `EventArgs.Empty`。

### EventHandler<TEventArgs> 泛型版本

当事件需要传递自定义数据时，应使用泛型版本 `EventHandler<TEventArgs>`，其中 `TEventArgs` 必须是 `EventArgs` 或其派生类。

## 三、事件的用法

1. **定义事件**（在发布者类中）
2. **注册事件处理程序**（在订阅者类中）
3. **触发事件**（在发布者类中）
4. **处理事件**（在订阅者类中）

### 示例

```csharp
// 发布者类
public class Publisher
{
    // 定义事件 - 无数据事件
    public event EventHandler MyEvent;
  
    // 触发事件的方法（推荐使用空值检查）
    protected virtual void OnMyEvent()
    {
        MyEvent?.Invoke(this, EventArgs.Empty);
    }
  
    // 触发事件的公共方法
    public void TriggerEvent()
    {
        OnMyEvent();
    }
}

// 订阅者类
public class Subscriber
{
    public void HandleMyEvent(object sender, EventArgs e)
    {
        Console.WriteLine("事件已触发!");
        // 可以访问sender对象，如：((Publisher)sender).SomeProperty
    }
}

// 使用示例
public class Program
{
    static void Main()
    {
        Publisher publisher = new Publisher();
        Subscriber subscriber = new Subscriber();
      
        // 注册事件处理程序
        publisher.MyEvent += subscriber.HandleMyEvent;
      
        // 触发事件
        publisher.TriggerEvent();
      
        // 可选：移除事件处理程序
        publisher.MyEvent -= subscriber.HandleMyEvent;
    }
}
```

## 传递自定义事件数据

### 创建自定义事件参数类

```csharp
// 自定义事件参数类
public class CustomEventArgs : EventArgs
{
    public string Message { get; set; }
    public int Value { get; set; }
}
```

### 使用泛型 EventHandler

```csharp
public class DataPublisher
{
    public event EventHandler<CustomEventArgs> DataProcessed;
  
    protected virtual void OnDataProcessed(CustomEventArgs e)
    {
        DataProcessed?.Invoke(this, e);
    }
  
    public void ProcessData(string message, int value)
    {
        // 处理数据...
        OnDataProcessed(new CustomEventArgs { Message = message, Value = value });
    }
}

public class DataSubscriber
{
    public void HandleDataProcessed(object sender, CustomEventArgs e)
    {
        Console.WriteLine($"收到数据: {e.Message}, 值: {e.Value}");
    }
}
```

## 注意事项

### 空值检查

```csharp
// 正确做法（空值检查）
MyEvent?.Invoke(this, EventArgs.Empty);
```

### 使用 Lambda 表达式注册事件

```csharp
publisher.MyEvent += (sender, e) => 
{
    Console.WriteLine("通过Lambda处理事件");
};
```

### 6.3 事件注销（避免内存泄漏）

```csharp
public class Subscriber : IDisposable
{
    private Publisher _publisher;
  
    public Subscriber(Publisher publisher)
    {
        _publisher = publisher;
        _publisher.MyEvent += HandleEvent;
    }
  
    public void Dispose()
    {
        _publisher.MyEvent -= HandleEvent;
    }
  
    private void HandleEvent(object sender, EventArgs e)
    {
        // 事件处理逻辑
    }
}
```

### 事件线程安全

```csharp
// 线程安全的事件触发
var handler = Volatile.Read(ref MyEvent);
handler?.Invoke(this, EventArgs.Empty);
```

如果事件处理程序本身不是线程安全的，或者你需要保护共享资源，你可以使用线程安全集合（如 `ConcurrentQueue<T>`）或显式的同步机制（如 `lock` 语句、`Monitor` 类、`Mutex`、`Semaphore` 等）来确保线程安全。

```csharp
private readonly object _lockObject = new object();

protected virtual void OnMyEvent(EventArgs e)
{
    lock (_lockObject)
    {
        var handler = MyEvent;
        handler?.Invoke(this, e);
    }
}
```

## 七、最佳实践

1. **遵循事件处理模式**：始终使用 `EventHandler` 或其泛型版本 `EventHandler<TEventArgs>`，保持代码的一致性和可读性。

2. **提供默认实现**：对于可继承的类，建议提供虚方法 `OnEventName` 作为事件触发的入口，方便子类重写。

3. **使用泛型版本**：当需要传递复杂参数时，优先使用 `EventHandler<TEventArgs>`，以提高类型安全性和代码可维护性。

4. **避免内存泄漏**：在适当的时候注销事件处理程序，特别是在订阅者生命周期结束时。

5. **保持松耦合**：通过事件实现发布者和订阅者之间的松耦合通信，这是 C# 事件模型的核心思想。
