---
title: LINQ
description: C# LINQ（语言集成查询）详解，包括标准查询操作符、延迟执行、分组、连接、聚合等核心用法。
---

### C# LINQ 方法详解及使用示例

LINQ（Language Integrated Query）是.NET中的一组技术，提供统一的数据查询接口，用于直接在实现了 `IEnumerable<T>` 或 `IQueryable<T>` 接口的内存集合（如列表、数组）上进行查询操作。它提供了一种**声明式、类型安全**的查询语法，显著简化了对集合数据的处理。以下是常用LINQ方法分类及示例：

#### 1. **过滤方法**

- **`Where`**：条件筛选

```csharp
int[] numbers = { 1, 2, 3, 4, 5 };
var evens = numbers.Where(n => n % 2 == 0);  // 结果: {2, 4}
```

#### 2. **投影方法**

- **`Select`**：转换元素

```csharp
int[] numbers = { 1, 2, 3, 4, 5 };
var squares = numbers.Select(n => n * n);  // 结果: {1, 4, 9, 16, 25}
```

- **`SelectMany`**：展平嵌套集合

```csharp
string[] words = { "hello", "world" };
var letters = words.SelectMany(w => w.ToCharArray());  // 结果: {'h','e','l','l','o','w','o','r','l','d'}
```

#### 3. **排序方法**

- **`OrderBy`** / **`OrderByDescending`**：主排序

```csharp
string[] words = { "hi", "world" };
var ordered = words.OrderBy(w => w.Length);  // 按长度升序
```

- **`ThenBy`** / **`ThenByDescending`**：次级排序

```csharp
var users = new[] { new { Name = "Alice", Age = 30 }, new { Name = "Bob", Age = 25 } };
var sorted = users.OrderBy(u => u.Name).ThenBy(u => u.Age);
```

#### 4. **分组方法**

- **`GroupBy`**：按键分组

```csharp
var users = new[] { new { Name = "Alice", Age = 30 }, new { Name = "Bob", Age = 25 } };
var grouped = users.GroupBy(u => u.Age > 25);
foreach (var group in grouped) 
{
  Console.WriteLine(group.Key ? "Over 25" : "Under 26");
  foreach (var user in group) Console.WriteLine(user.Name);
}
```

#### 5. **聚合方法**

- **`Count`** / **`Sum`** / **`Average`** / **`Min`** / **`Max`**

```csharp
int[] numbers = { 1, 2, 3, 4, 5 };
int count = numbers.Count();  // 5
int total = numbers.Sum();    // 15
double avg = numbers.Average(); // 3
```

- **`Aggregate`**：自定义聚合

```csharp
string[] words = { "hello", "world" };
string joined = words.Aggregate((a, b) => a + ", " + b);  // "hello, world"
```

#### 6. **连接方法**

- **`Join`**：内连接

```csharp
var orders = new[] { new { ID = 1, Product = "Apple" }, new { ID = 2, Product = "Banana" } };
var customers = new[] { new { OrderID = 1, Name = "Alice" }, new { OrderID = 2, Name = "Bob" } };

var joinedData = orders.Join(customers, 
  o => o.ID, 
  c => c.OrderID,
  (o, c) => $"{c.Name} bought {o.Product}");
// 结果: {"Alice bought Apple", "Bob bought Banana"}
```

#### 7. **集合操作**

- **`Distinct`**：去重

```csharp
int[] dupes = { 1, 2, 2, 3 };
var unique = dupes.Distinct();  // {1, 2, 3}
```

- **`Union`** / **`Intersect`** / **`Except`**

```csharp
int[] setA = { 1, 2, 3 }, setB = { 2, 3, 4 };
var union = setA.Union(setB);      // {1,2,3,4}
var intersect = setA.Intersect(setB); // {2,3}
var except = setA.Except(setB);    // {1}
```

#### 8. **元素访问**

- **`First`** / **`Last`** / **`ElementAt`**

```csharp
int[] numbers = { 1, 2, 3, 4, 5 };
int first = numbers.First();  // 1
int last = numbers.Last();    // 5
int third = numbers.ElementAt(2); // 3
```

- **`FirstOrDefault`**：安全访问

```csharp
int[] numbers = { 1, 2, 3, 4, 5 };
int firstOver10 = numbers.FirstOrDefault(n => n > 10); // 0 (默认值)
```

#### 9. **分页方法**

- **`Skip`** / **`Take`**：分页处理

```csharp
int[] numbers = { 1, 2, 3, 4, 5 };
var page2 = numbers.Skip(2).Take(2);  // 跳过前2个，取2个: {3, 4}
```

#### 10. **判断方法**

- **`Any`** / **`All`** / **`Contains`**

```csharp
int[] numbers = { 1, 2, 3, 4, 5 };
bool hasEven = numbers.Any(n => n % 2 == 0); // true
bool allPositive = numbers.All(n => n > 0);  // true
bool hasThree = numbers.Contains(3);         // true
```

#### 11. **生成序列**

- **`Range`** / **`Repeat`** / **`Empty`**

```csharp
var range = Enumerable.Range(1, 3); // {1, 2, 3}
var repeats = Enumerable.Repeat("Hi", 2); // {"Hi", "Hi"}
var empty = Enumerable.Empty<int>(); // 空序列
```

#### 12. **转换方法**

- **`ToArray`** / **`ToList`**：立即执行查询

```csharp
int[] numbers = { 1, 2, 3, 4, 5 };
List<int> list = numbers.Where(n => n > 3).ToList(); // [4, 5]
```

- **`ToDictionary`** / **`ToLookup`**

```csharp
var users = new[] { new { Name = "Alice", Age = 30 }, new { Name = "Bob", Age = 25 } };
var dict = users.ToDictionary(u => u.Name); // 键值对字典
var lookup = users.ToLookup(u => u.Age);    // 一键多值
```

### 关键特性

1. **延迟执行**：大部分方法（如`Where`）在枚举结果时才执行
2. **链式调用**：支持方法链组合

```csharp
var result = numbers
   .Where(n => n > 1)
   .OrderByDescending(n => n)
   .Select(n => n * 10);
```

1. **两种语法**：
   - 方法语法（如上示例）
   - 查询表达式语法：

```csharp
var query = from n in numbers 
   where n % 2 == 0 
   select n * 2;
```
