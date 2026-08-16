---
title: 命令模式
description: C# 命令模式（Command）详解：将请求封装为对象，支持撤销、重放与排队，结合遥控器示例讲解。
---

# 命令模式（Command）

命令模式把**请求封装成对象**，使发起请求的调用方与执行请求的接收方完全解耦。就像遥控器：按下按钮时，遥控器并不知道也不关心背后是开灯还是开车库门——每个按钮只是持有一个"命令对象"，按下去就执行它。

## 解决什么问题

传统调用方式中，动作的执行直接绑定在调用方代码里：

1. **调用方与接收方强耦合**：遥控器直接持有 `Light`、`Garage` 的引用，新增一种设备就要改遥控器。
2. **无法参数化与延迟执行**：请求不能像对象一样传递、排队、记录。
3. **撤销困难**：想要"撤销上一次操作"，没有统一的入口。

命令模式把"做什么"（命令对象）与"谁来做"（接收方）分离，请求变成了可传递、可存储、可撤销的**一等公民**。

## 核心结构

| 角色 | 说明 | 示例代码 |
| :--- | :--- | :--- |
| 命令接口 | 声明执行与撤销方法 | `ICommand` |
| 具体命令 | 绑定接收方，执行时调用其方法 | `LightOnCommand`、`GarageDoorOpenCommand`、`MacroCommand` |
| 接收方 | 真正执行动作的对象 | `Light`、`Garage` |
| 调用者 | 持有命令并触发执行 | `RemoteControl` |

## 代码示例

仓库示例 `src/DesignPatterns/CommandPattern` 以万能遥控器为背景。**命令接口**：

```csharp
public interface ICommand
{
    void Execute();
    void Undo();   // 撤销：反向操作
}
```

**接收方**——灯和车库门：

```csharp
public class Light
{
    private readonly string _name;

    public Light(string name) => _name = name;

    internal void On() => Console.WriteLine($"{_name} Light On");
    internal void Off() => Console.WriteLine($"{_name} Light Off");
}

public class Garage
{
    private readonly string _name;

    public Garage(string name) => _name = name;

    internal void Open() => Console.WriteLine($"{_name} Garage Opened");
    internal void Close() => Console.WriteLine($"{_name} Garage Closed");
}
```

**具体命令**——把"开灯"动作封装成对象，`Undo` 执行反向操作：

```csharp
public class LightOnCommand : ICommand
{
    private readonly Light _light;

    public LightOnCommand(Light light) => _light = light;

    public void Execute() => _light.On();
    public void Undo() => _light.Off();
}

public class GarageDoorOpenCommand : ICommand
{
    private readonly Garage _garage;

    public GarageDoorOpenCommand(Garage garage) => _garage = garage;

    public void Execute() => _garage.Open();
    public void Undo() => _garage.Close();
}
```

**调用者**——遥控器有多个槽位，每个槽位一组"开/关"命令，并记录上次操作支持撤销。空槽位用 `NoCommand` 占位，避免空引用判断：

```csharp
public class RemoteControl
{
    private readonly ICommand[] _offCommand;
    private readonly ICommand[] _onCommand;
    private ICommand _undoCommand;

    public RemoteControl(int slots)
    {
        _onCommand = new ICommand[slots];
        _offCommand = new ICommand[slots];

        var none = new NoCommand();
        _undoCommand = none;
        for (var i = 0; i < slots; i++)
        {
            _onCommand[i] = none;
            _offCommand[i] = none;
        }
    }

    public OnOffStruct this[int i]
    {
        set
        {
            _onCommand[i] = value.On;
            _offCommand[i] = value.Off;
        }
    }

    public void PushOn(int slot)
    {
        _onCommand[slot].Execute();
        _undoCommand = _offCommand[slot];   // 撤销 = 执行反操作
    }

    public void PushOff(int slot)
    {
        _offCommand[slot].Execute();
        _undoCommand = _onCommand[slot];
    }

    public void PushUndo() => _undoCommand.Execute();
}
```

**宏命令**——把多个命令打包成一个命令，一次执行、一次撤销：

```csharp
public class MacroCommand : ICommand
{
    private readonly ICommand[] _commands;

    public MacroCommand(ICommand[] commands) => _commands = commands;

    public void Execute()
    {
        foreach (var item in _commands) item.Execute();
    }

    public void Undo()
    {
        foreach (var item in _commands) item.Undo();
    }
}
```

演示入口（完整代码见 `Program.cs`）：

```csharp
var remote = new RemoteControl(3);

var bike = new Garage("Bike");
var bikeDoorOpen = new GarageDoorOpenCommand(bike);
var bikeDoorClose = new GarageDoorCloseCommand(bike);

var garageButton = new OnOffStruct { On = bikeDoorOpen, Off = bikeDoorClose };
remote[0] = garageButton;

remote.PushOn(0);
remote.PushUndo();
remote.PushUndo();
remote.PushOff(0);

Console.WriteLine();
var light = new Light("Hall");
ICommand[] partyOn = { new LightOffCommand(light), bikeDoorOpen };   // "派对模式"宏命令
var partyButton = new OnOffStruct
{
    On = new MacroCommand(partyOn),
    Off = new MacroCommand(partyOff)
};
remote[2] = partyButton;
remote.PushOn(2);
```

运行结果（节选）：

```text
Bike Garage Opened
Bike Garage Closed
Bike Garage Opened
Bike Garage Off...

Hall Light Off
Bike Garage Opened
```

按下"开"→ 撤销（关门）→ 再撤销（开门）→ 按"关"（关门）：每次操作的撤销都能精确回退，这正是命令对象可存储带来的能力。

## 命令模式的扩展能力

| 能力 | 说明 |
| :--- | :--- |
| 撤销/重做 | 命令对象入栈，出栈执行 `Undo` |
| 队列化 | 命令排队执行，实现任务调度 |
| 日志与恢复 | 记录命令序列，崩溃后可重放恢复状态 |
| 宏命令 | 组合多个命令为复合操作 |

## 典型应用场景

- **编辑器撤销栈**：文本编辑器每次操作压栈，Ctrl+Z 弹栈执行 `Undo`。
- **任务队列**：后台任务、定时任务把操作封装成命令排队执行。
- **事务性操作**：数据库操作的命令化，便于统一回滚。
- **GUI 按钮**：WPF 的 `ICommand`（`RelayCommand`）正是命令模式在框架中的实现，绑定按钮与业务动作。

## 总结

- **请求即对象**：动作被封装，可存储、传递、排队、撤销。
- **调用者与接收者解耦**：遥控器不认识灯和车库，只认识 `ICommand`。
- **组合能力**：宏命令把多个操作打包，扩展成复合操作。
- **注意**：命令类数量会随操作增多而增长，可用 `Action` 委托简化简单场景。
