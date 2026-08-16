---
title: ExpandoObject
description: C# 动态类型 ExpandoObject 的使用详解，包括动态属性添加、JSON 反序列化、与 DataTable 互转等。
---

ExpandoObject 允许我们在运行时动态地添加和删除对象的成员（属性、方法、事件等），非常适合需要灵活数据结构的场景，如 JSON 操作、动态配置、数据转换等。

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

输出：
```text
X = 1
Y = 2
```


---

### 删除属性

```csharp
dynamic obj = new ExpandoObject();
obj.A = 100;
obj.B = "Hello";

var dict = (IDictionary<string, object>)obj;

Console.WriteLine($"删除前属性数量：{dict.Count}");

bool removed = dict.Remove("A");  // 返回是否删除成功

Console.WriteLine($"删除 A 是否成功：{removed}，删除后属性数量：{dict.Count}");

foreach (var kv in dict)
{
    Console.WriteLine($"  {kv.Key} = {kv.Value}");
}
```

输出：
```text
删除前属性数量：2
删除 A 是否成功：True，删除后属性数量：1
  B = Hello
```

**注意**：属性删除后，再通过 `obj.A` 访问会抛出 `RuntimeBinderException`（运行时绑定异常），因为该成员已不存在。

### 监听属性变化

因为实现了 INotifyPropertyChanged，可以订阅 PropertyChanged 事件来监听属性的变化：

```csharp
using System.ComponentModel;
using System.Dynamic;

// 创建 ExpandoObject 并设置初始值
// 注意：必须用 dynamic 声明，才能以属性的方式访问成员
dynamic expando = new ExpandoObject();
expando.Name = "Tom";  // 初始赋值（新增属性，也会触发通知，只是此时还没有订阅者）

// 订阅 PropertyChanged 事件
// 注意：dynamic 无法直接用 += 订阅事件，需先转换为 INotifyPropertyChanged 接口
((INotifyPropertyChanged)expando).PropertyChanged += (sender, e) =>
{
    Console.WriteLine($"属性 {e.PropertyName} 发生变化");
};

expando.Name = "Bob";  // 修改已有属性，值不同，触发通知
expando.Age = 18;      // 新增属性，同样触发通知
```

输出：
```text
属性 Name 发生变化
属性 Age 发生变化
```

**要点**：

- 添加新属性、修改已有属性（值不同时）都会触发 `PropertyChanged`
- 给已有属性赋相同的值时不会触发通知
- `dynamic` 无法直接 `expando.PropertyChanged += lambda` 订阅事件（编译器不知道委托类型），必须先转换为 `INotifyPropertyChanged` 接口再订阅

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

需要安装 nuget 包 `Newtonsoft.Json`

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
