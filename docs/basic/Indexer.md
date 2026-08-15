---
title: 索引器
description: C# 索引器（Indexer）详解，包括基本用法、多参数索引器、接口实现、与属性的对比等。
---


索引器（Indexer）允许对象像数组一样通过索引进行数据访问，它是 C# 中实现类似容器行为的核心特性。与属性不同，索引器使用 **参数化访问** ，支持自定义索引逻辑。

## ​**一、索引器的核心概念**​

- 为自定义类型提供类似数组的访问方式（如 `obj[0] = 10`）
- 封装内部集合的访问逻辑
- 支持非整数索引（如字符串、枚举等）

## ​**二、实现索引器的步骤**​

### ​**1. 基本语法结构**​

```cs
public 返回类型 this[参数类型 索引]
{
    get { /* 返回索引对应的值 */ }
    set { /* 设置索引对应的值 (使用 value 关键字) */ }
}
```

### ​**2. 简单示例：封装数组的类**​

```cs
public class IntArrayWrapper
{
    private int[] _array = new int[10];

    // 实现整型索引器
    public int this[int index]
    {
        get => _array[index];
        set => _array[index] = value;
    }
}

// 使用示例
var wrapper = new IntArrayWrapper();
wrapper[0] = 42;      // 调用 set
Console.WriteLine(wrapper[0]); // 调用 get → 输出 42
```

### ​**3. 多参数索引器（模拟矩阵）​**​

```cs
public class Matrix
{
    private double[,] _data = new double[3, 3];

    // 双参数索引器
    public double this[int row, int col]
    {
        get => _data[row, col];
        set => _data[row, col] = value;
    }
}

// 使用
var matrix = new Matrix();
matrix[1, 2] = 3.14;  // 设置第二行第三列
```

### ​**4. 非整数索引（字典风格）​**​

```cs
public class PersonCollection
{
    private Dictionary<string, Person> _people = new();

    // 字符串索引器
    public Person this[string name]
    {
        get => _people[name];
        set => _people[name] = value;
    }
}

// 使用
var employees = new PersonCollection();
employees["Alice"] = new Person("Alice", 30);
Console.WriteLine(employees["Alice"].Age); // 输出 30
```

## ​**三、高级用法**​

### ​**1. 索引器重载**​

通过多种方式来访问数据

```cs
public class SmartCollection
{
    private List<string> _items = new();
    
    // 重载 1: 整型索引
    public string this[int index] => _items[index];
    
    // 重载 2: 字符串索引（查找元素）
    public int this[string item] => _items.IndexOf(item);
}
```

### ​**2. 只读索引器**​

```cs
public class ReadOnlyWrapper
{
    private int[] _data = { 1, 2, 3 };

    public int this[int index] => _data[index]; // 只有 get
}
```

#### ​**3. 接口中的索引器**​

```cs
public interface IDataContainer
{
    string this[int idx] { get; set; }
}

public class DataFile : IDataContainer
{
    public string this[int idx] { get; set; } // 必须实现接口索引器
}
```

## ​**四、关键注意事项**​

1. ​**参数限制**​
    - 至少需要一个参数
    - 不支持 `ref`/`out` 参数
2. ​**设计原则**​
    - ​**避免复杂逻辑**​：索引器应快速返回结果
    - ​**异常处理**​：在 `get`/`set` 中验证索引有效性
    - ​**集合封装**​：优先使用现有集合（如 `List<T>`），通过索引器暴露访问

## ​**五、完整示例**​

```cs
public class TemperatureTracker
{
    private double[] _temps = new double[365];
    
    // 索引器（带边界检查）
    public double this[int day]
    {
        get
        {
            if (day < 0 || day >= 365)
                throw new IndexOutOfRangeException();
            return _temps[day];
        }
        set
        {
            if (day < 0 || day >= 365)
                throw new IndexOutOfRangeException();
            _temps[day] = value;
        }
    }

    // 重载：日期索引
    public double this[DateTime date]
    {
        get => this[date.DayOfYear - 1];
        set => this[date.DayOfYear - 1] = value;
    }
}

// 使用
var tracker = new TemperatureTracker();
tracker[0] = -5.0;              // 第0天
tracker[DateTime.Today] = 22.5; // 通过日期设置
```

通过索引器，`TemperatureTracker` 实现了两种自然的数据访问方式：通过天数序号或直接通过日期对象，大幅提升了代码可读性。

索引器是 C# 封装集合类数据的利器，合理使用能使 API 设计更直观高效。关键在于平衡便捷性和封装性，避免暴露内部实现细节。
