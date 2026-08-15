---
title: C# 反射
description: C# 反射（Reflection）详解，包括类型信息获取、动态创建对象、方法调用、特性（Attribute）使用、插件系统实现等。
---

### 什么是反射？

反射让你可以在编译时不知道具体类型的情况下，在运行时：

- 获取类型的信息（如类、接口、方法、属性等）。
- 创建类型的实例。
- 调用类型的方法。
- 访问和修改类型的字段和属性。

反射的功能主要由 System.Reflection 命名空间下的一系列类提供。

| 类 | 用途 |
| :--- | :--- |
| `Type` | 反射的入口，代表一个类型（类、接口、结构等）的元数据。 |
| `Assembly` | 代表一个程序集（.dll 或 .exe 文件），用于加载和检查程序集<websource>source_group_web_5</websource>。 |
| `MethodInfo` | 提供关于方法的信息，并允许动态调用它<websource>source_group_web_6</websource>。 |
| `PropertyInfo` | 提供关于属性的信息，并允许获取或设置属性值<websource>source_group_web_7</websource>。 |
| `FieldInfo` | 提供关于字段的信息，并允许获取或设置字段值<websource>source_group_web_8</websource>。 |
| `ConstructorInfo` | 提供关于构造函数的信息，并允许动态创建实例<websource>source_group_web_9</websource>。 |

### 动态插件系统场景

反射（Reflection）是 .NET 的核心机制，允许在运行时动态获取类型信息、创建对象、调用方法、访问字段/属性等。以下通过一个**插件系统**的完整场景演示反射的核心功能。

实现一个主程序，能动态加载插件 DLL 文件，执行插件中的任务并收集结果。

#### 1. 定义插件接口 (IPlugin.cs)

```csharp
public interface IPlugin
{
    string Name { get; }
    string Execute(object input);
    Type GetInputType();  // 反射关键：获取输入类型
}
```

#### 2. 创建插件 (TextPlugin.dll)

```csharp
[Description("文本处理插件")]
public class TextPlugin : IPlugin
{
    public string Name => "文本处理器";
    
    public Type GetInputType() => typeof(string);

    public string Execute(object input)
    {
        string text = (string)input;
        return $"处理结果: {text.ToUpper()} (长度: {text.Length})";
    }
}
```

#### 3. 主程序核心反射逻辑

```csharp
public class PluginHost
{
    public List<IPlugin> LoadPlugins(string pluginPath)
    {
        var plugins = new List<IPlugin>();
        
        foreach (string dll in Directory.GetFiles(pluginPath, "*.dll"))
        {
            // 1. 加载程序集
            Assembly assembly = Assembly.LoadFrom(dll);

            // 2. 获取所有公共类型
            foreach (Type type in assembly.GetExportedTypes())
            {
                // 3. 检查是否实现IPlugin接口
                if (typeof(IPlugin).IsAssignableFrom(type) && !type.IsAbstract)
                {
                    // 4. 创建实例（激活）
                    IPlugin plugin = (IPlugin)Activator.CreateInstance(type);
                    plugins.Add(plugin);
                }
            }
        }
        return plugins;
    }

    public void RunPlugins(List<IPlugin> plugins)
    {
        foreach (var plugin in plugins)
        {
            // 5. 获取输入类型（动态类型检查）
            Type inputType = plugin.GetInputType();
            
            // 6. 动态创建输入对象
            object input = CreateSampleInput(inputType);
            
            // 7. 反射调用方法
            MethodInfo executeMethod = plugin.GetType().GetMethod("Execute");
            string result = (string)executeMethod.Invoke(plugin, new[] { input });
            
            // 8. 获取特性元数据
            var descAttr = (DescriptionAttribute)Attribute.GetCustomAttribute(
                plugin.GetType(), typeof(DescriptionAttribute));
            
            Console.WriteLine($"[{descAttr?.Description ?? "无描述"}] {result}");
        }
    }

    private object CreateSampleInput(Type type)
    {
        if (type == typeof(string)) return "hello reflection";
        if (type == typeof(int)) return 42;
        // 可扩展其他类型
        return Activator.CreateInstance(type); // 默认构造函数
    }
}
```

---

### 反射核心功能详解

#### 1. 程序集加载

```csharp
Assembly assembly = Assembly.LoadFrom("Plugin.dll");
```

- `Assembly.LoadFrom()`：从文件路径加载
- `Assembly.Load()`：通过强名称加载

#### 2. 类型发现

```csharp
Type[] types = assembly.GetTypes();          // 所有类型
Type[] exportedTypes = assembly.GetExportedTypes(); // 公开类型
Type pluginType = assembly.GetType("Namespace.PluginClass");
```

#### 3. 类型检查

```csharp
bool isPlugin = typeof(IPlugin).IsAssignableFrom(type); // 接口实现检查
bool isAbstract = type.IsAbstract;                     // 抽象类检查
bool hasAttribute = type.IsDefined(typeof(SerializableAttribute)); // 特性检查
```

#### 4. 实例化对象

```csharp
// 无参构造
object obj = Activator.CreateInstance(type); 

// 带参数构造
ConstructorInfo ctor = type.GetConstructor(new[] { typeof(int) });
object obj = ctor.Invoke(new object[] { 100 });
```

#### 5. 方法调用

```csharp
MethodInfo method = type.GetMethod("Execute", 
    BindingFlags.Public | BindingFlags.Instance);

// 调用实例方法
object result = method.Invoke(pluginInstance, new object[] { input });

// 调用静态方法
method.Invoke(null, new object[] { staticParam });
```

#### 6. 属性/字段操作

```csharp
// 属性操作
PropertyInfo prop = type.GetProperty("Name");
string name = (string)prop.GetValue(pluginInstance);
prop.SetValue(pluginInstance, "NewName");

// 字段操作（包括私有字段）
FieldInfo field = type.GetField("_counter", 
    BindingFlags.NonPublic | BindingFlags.Instance);
int value = (int)field.GetValue(pluginInstance);
```

#### 7. 特性获取

```csharp
// 类级别特性
DescriptionAttribute attr = (DescriptionAttribute)
    Attribute.GetCustomAttribute(type, typeof(DescriptionAttribute));

// 方法级别特性
var attrs = method.GetCustomAttributes(typeof(ObsoleteAttribute), false);
```

#### 8. 泛型类型处理

```csharp
// 创建泛型类型
Type openType = typeof(List<>);
Type closedType = openType.MakeGenericType(typeof(int));

// 调用泛型方法
MethodInfo genericMethod = type.GetMethod("GenericMethod");
MethodInfo closedMethod = genericMethod.MakeGenericMethod(typeof(string));
closedMethod.Invoke(obj, null);
```

---

### 高级反射技巧

#### 1. 动态代理（AOP）

```csharp
public class DynamicProxy : DispatchProxy
{
    protected override object Invoke(MethodInfo method, object[] args)
    {
        Console.WriteLine($"调用前: {method.Name}");
        object result = method.Invoke(target, args);
        Console.WriteLine($"调用后: {method.Name}");
        return result;
    }
}
```

#### 2. 表达式树优化性能

```csharp
// 代替MethodInfo.Invoke提升100倍性能
public static Func<object, object[], object> CreateMethodInvoker(MethodInfo method)
{
    var instance = Expression.Parameter(typeof(object));
    var args = Expression.Parameter(typeof(object[]));
    
    var parameters = method.GetParameters();
    var argsExp = new Expression[parameters.Length];
    
    for (int i = 0; i < parameters.Length; i++)
    {
        argsExp[i] = Expression.Convert(
            Expression.ArrayIndex(args, Expression.Constant(i)),
            parameters[i].ParameterType);
    }
    
    var call = Expression.Call(
        Expression.Convert(instance, method.DeclaringType),
        method,
        argsExp);
    
    return Expression.Lambda<Func<object, object[], object>>(
        Expression.Convert(call, typeof(object)),
        instance, args).Compile();
}
```

#### 3. 模块初始化劫持

```csharp
[ModuleInitializer]
public static void Initialize()
{
    // 在模块加载时执行
    Console.WriteLine("模块被加载！");
}
```

---

### 反射的注意事项

1. **性能问题**：
   - 反射调用比直接调用慢约100-1000倍
   - 解决方案：缓存反射结果、使用`Delegate.CreateDelegate`或表达式树

2. **安全限制**：
   - 部分反射操作需要`ReflectionPermission`
   - 在沙箱环境中可能受限

3. **类型安全**：
   - `Invoke`可能抛出`TargetInvocationException`
   - 需处理`MissingMethodException`等异常

4. **程序集隔离**：
   - 使用`AssemblyLoadContext`实现插件热卸载

   ```csharp
   var alc = new AssemblyLoadContext("Plugins", true);
   Assembly assembly = alc.LoadFromAssemblyPath(dllPath);
   alc.Unload();  // 卸载程序集
   ```

---

### 反射的典型应用场景

1. 依赖注入框架（如ASP.NET Core）
2. ORM框架（如Entity Framework的映射）
3. 序列化/反序列化（如System.Text.Json）
4. AOP编程（动态代理）
5. 单元测试框架（发现测试方法）
6. 插件系统（如本例）

通过此插件系统示例，我们完整展示了反射在动态类型发现、对象创建、方法调用、特性处理等关键场景的应用，体现了反射作为.NET元编程核心技术的强大能力。
