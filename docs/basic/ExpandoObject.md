---
title: ExpandoObject
description: C# 动态类型 ExpandoObject 的使用详解，包括动态属性添加、JSON 反序列化、与 DataTable 互转等。
---

ExpandoObject 是 .NET Framework 4.0 引入的一个类，位于 System.Dynamic 命名空间。它允许我们在运行时动态地添加和删除对象的成员（属性、方法、事件等），非常适合需要灵活数据结构的场景，如 JSON 操作、动态配置、数据转换等。

## ExpandoObject 的核心能力

### 运行时动态添加属性

```csharp
dynamic user = new ExpandoObject();
user.Id = 1;
user.Name = "Tom";
user.IsVip = true;
```

必须使用 `dynamic` ，不用 `dynamic` 的话：

```csharp
ExpandoObject obj = new ExpandoObject();
// obj.Name = "Tom"; ❌ 编译错误
```

- 属性名、数量完全不受限制
- 非常适合：
  - **结构不固定的数据**
  - **中间态数据拼装**

---

### 运行时动态添加“方法”

```csharp
dynamic obj = new ExpandoObject();

obj.SayHello = (Action)(() =>
{
    Console.WriteLine("Hello Expando!");
});

obj.SayHello();
```

也可以带参数：

```csharp
obj.Add = (Func<int, int, int>)((a, b) => a + b);
Console.WriteLine(obj.Add(2, 3)); // 5
```

---

### 实现 IDictionary<string, object>

这是 **ExpandoObject 的关键设计点**：

```csharp
var expando = new ExpandoObject();
var dict = (IDictionary<string, object>)expando;

dict["A"] = 100;
dict["B"] = "Hello";

dynamic d = expando;
Console.WriteLine(d.A); // 100
Console.WriteLine(d.B); // Hello
```

**属性访问 & 字典访问是同一份数据**

---

### 可枚举所有动态成员

```csharp
dynamic obj = new ExpandoObject();
obj.X = 1;
obj.Y = 2;

foreach (var kv in (IDictionary<string, object>)obj)
{
    Console.WriteLine($"{kv.Key} = {kv.Value}");
}
```

---

### 删除属性

```csharp
var dict = (IDictionary<string, object>)obj;
dict.Remove("A");
```

### 监听属性变化

因为实现了 INotifyPropertyChanged，可以订阅 PropertyChanged 事件：

```csharp
expando.PropertyChanged += (sender, e) =>
{
    Console.WriteLine($"属性 {e.PropertyName} 发生变化");
};

expando.Name = "Bob";  // 会触发通知
```

## ExpandoObject 的典型使用场景

### JSON / API 动态数据承载

灵活构建动态数据，不需要创建类：

```csharp
dynamic response = new ExpandoObject();
response.code = 0;
response.message = "ok";
response.data = new { id = 1, name = "Tom" };

var json = System.Text.Json.JsonSerializer.Serialize(response);

Console.WriteLine(json);
//输出：{"code":0,"message":"ok","data":{"id":1,"name":"Tom"}}
```

将 JSON 字符串转成对象：

```csharp
// 模拟从API接收的动态JSON数据
string jsonString = @"{
  ""Name"": ""智能手表"",
  ""Price"": 1299.00,
  ""Specs"": {
   ""Color"": ""黑色"",
   ""Battery"": ""7天续航""
  }
 }";

// 正确的反序列化设置
var settings = new JsonSerializerSettings();
settings.Converters.Add(new ExpandoObjectConverter());

dynamic product = JsonConvert.DeserializeObject<ExpandoObject>(jsonString, settings);

Console.WriteLine($"产品名称：{product.Name}");
Console.WriteLine($"价格：¥{product.Price}");
Console.WriteLine($"颜色：{product.Specs.Color}");
Console.WriteLine($"电池：{product.Specs.Battery}");

// 动态添加新属性
product.Stock = 100;
product.IsNewArrival = true;

// 转换回JSON
string updatedJson = JsonConvert.SerializeObject(product, Newtonsoft.Json.Formatting.Indented);
Console.WriteLine("\n更新后的JSON：");
Console.WriteLine(updatedJson);

//输出：
/*
产品名称：智能手表  
价格：¥1299  
颜色：黑色  
电池：7天续航  
  
更新后的JSON：  
{  
  "Name": "智能手表",  
  "Price": 1299.0,  
  "Specs": {  
    "Color": "黑色",  
    "Battery": "7天续航"  
  },  
  "Stock": 100,  
  "IsNewArrival": true  
}
*/
```

**注意**：以上JSON序列化使用 Newtonsoft.Json 。

---

## 注意事项

1. 性能开销：动态类型比静态类型慢，不适合高性能场景
2. 类型安全缺失：编译时不检查成员，运行时出错风险较高
3. IntelliSense失效：IDE无法提供智能提示
4. 调试困难：动态成员在调试时不易观察
5. 建议：仅在确实需要动态特性的场景使用，优先使用静态类型
