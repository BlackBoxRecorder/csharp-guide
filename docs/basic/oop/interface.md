---
title: 接口
description: C# 接口（Interface）详解，包括接口定义、实现、默认实现、多接口继承、接口与抽象类的区别。
---

在 C# 中，接口（Interface）是一种强大的抽象机制，用于定义**契约（Contract）**。它指定类或结构体必须实现的成员（方法、属性、事件、索引器），但不提供具体实现。接口的核心价值在于实现多态性和解耦，让代码更灵活、可扩展。

### **核心特性**

1. **纯抽象定义**  
   - 只包含成员声明（无实现）
   - 不能包含字段、构造函数或常量
2. **隐式公开性**  
   - 所有成员默认为 `public`（不能添加访问修饰符）
3. **多继承支持**  
   - 类可继承多个接口（弥补 C# 单继承限制）
4. **版本兼容性**（C# 8.0+）  
   - 支持默认方法实现（向后兼容）

### **接口定义语法**

```csharp
public interface ILogger
{
    // 方法声明
    void Log(string message);
    
    // 属性声明
    string LogLevel { get; set; }
    
    // 事件声明
    event Action<string> MessageLogged;
    
    // 索引器声明
    string this[int index] { get; set; }
}
```

### **接口实现方式**

#### 1. **隐式实现**

```csharp
public class FileLogger : ILogger
{
    public void Log(string message)
    {
        File.WriteAllText("log.txt", message);
    }

    public string LogLevel { get; set; }
    // 其他成员实现...
}
```

- 成员直接通过类实例访问
- **适用场景**：单一接口实现

#### 2. **显式实现**

```csharp
public class DatabaseLogger : ILogger
{
    void ILogger.Log(string message) // 显式实现
    {
        SaveToDatabase(message);
    }

    string ILogger.LogLevel { get; set; } // 显式实现
}
```

- 成员只能通过接口类型访问
- **适用场景**：
  - 解决多接口同名冲突
  - 隐藏特定接口的实现细节

```csharp
DatabaseLogger logger = new DatabaseLogger();
// logger.Log("test"); // 错误！不能直接访问
((ILogger)logger).Log("test"); // 正确
```

### **高级用法**

#### 1. **多接口继承**

```csharp
public interface IAuditable
{
    void Audit();
}

public class SecureLogger : ILogger, IAuditable
{
    // 实现 ILogger 成员...
    public void Audit() { /* 审计逻辑 */ }
}
```

#### 2. **接口继承接口**

```csharp
public interface IAdvancedLogger : ILogger
{
    void LogError(Exception ex);
}
```

#### 3. **默认实现（C# 8.0+）**

```csharp
public interface ILogger
{
    void Log(string message);
    
    // 默认实现
    void LogWarning(string warning) => Log($"WARNING: {warning}");
}
```

- 现有实现类无需重写该方法
- 可通过接口实例调用默认方法

#### 4. **接口与多态**

```csharp
public void ProcessLog(ILogger logger)
{
    logger.Log("Processing..."); // 多态调用
}

// 使用
ProcessLog(new FileLogger());
ProcessLog(new DatabaseLogger());
```

### **最佳实践**

1. **命名规范**  
   - 接口名以 `I` 开头（如 `IDisposable`）
2. **单一职责**  
   - 每个接口聚焦一个功能（参考接口隔离原则）
3. **依赖抽象**  
   - 代码应依赖接口而非具体类（便于单元测试和扩展）

   ```csharp
   // 推荐
   public class UserService
   {
       private readonly ILogger _logger;
       public UserService(ILogger logger) // 依赖注入
       {
           _logger = logger;
       }
   }
   ```

4. **显式实现冲突处理**  
   - 当多个接口有同名方法时，用显式实现消除歧义

### **接口 vs 抽象类**

| **特性**               | **接口**                     | **抽象类**                 |
|------------------------|------------------------------|----------------------------|
| 实现继承               | 多继承                       | 单继承                     |
| 成员实现               | 默认无实现（C#8.0 前）       | 可包含实现                 |
| 字段/构造函数          | ❌ 不允许                    | ✅ 允许                   |
| 访问修饰符             | 成员隐式 `public`            | 可自定义                   |
| 版本兼容性             | 默认方法避免破坏现有实现     | 修改抽象方法影响所有子类   |

### **典型应用场景**

1. **插件系统**  

   ```csharp
   public interface IPlugin
   {
       void Execute();
   }
   
   // 动态加载实现 IPlugin 的 DLL
   ```

2. **跨平台抽象**  

   ```csharp
   public interface IFileService
   {
       void SaveFile(string path, byte[] data);
   }
   
   // 分别实现 Windows/Mac/iOS 版本
   ```

3. **单元测试 Mock**  

   ```csharp
   var mockLogger = new Mock<ILogger>();
   mockLogger.Setup(x => x.Log(It.IsAny<string>()));
   ```

### **总结**

- **接口定义契约**：指定 "做什么"，不关心 "怎么做"。
- **解耦利器**：降低模块间耦合，提升可测试性。
- **多态基础**：通过接口实现运行时动态行为。
- **灵活扩展**：默认方法增强接口演进能力。

> 关键设计原则：**面向接口编程而非实现编程**，这是构建可维护、可扩展系统的基石。
