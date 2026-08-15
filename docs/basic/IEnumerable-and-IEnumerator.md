---
title: IEnumerable & IEnumerator
description: C# 中 IEnumerable 和 IEnumerator 接口的深入解析，包括迭代器模式、yield 关键字、协变与逆变等。
---

简单来说，**`IEnumerable` 是“能力”，`IEnumerator` 是“执行者”**。

* **`IEnumerable`**：代表一个集合 **“可以被遍历”** 。它是一个“能力接口”。
* **`IEnumerator`**：代表一个 **“正在遍历集合的工具”**。它是一个“功能接口”，负责具体的遍历动作。

## 核心区别对比

| 特性 | IEnumerable | IEnumerator |
| :--- | :--- | :--- |
| **核心角色** | **集合本身** | **遍历工具/游标** |
| **主要职责** | 声明“我是可枚举的”，提供获取 **枚举器(游标)** 的方法 | 执行具体的移动和读取操作 |
| **关键成员** | `GetEnumerator()` 方法 | `MoveNext()`, `Current`, `Reset()` |
| **状态维护** | 不维护遍历状态 | 维护当前遍历到了哪个位置 |

---

## 深入解析

#### IEnumerable：数据的提供者

当你让一个类实现 `IEnumerable` 接口时，你实际上是在告诉编译器，这个类里的数据是可以被 `foreach` 循环处理的。

* 它只包含一个核心方法：`GetEnumerator()`。
* 这个方法的作用不是自己去遍历，而是 **生产** 一个 `IEnumerator` 对象。

#### IEnumerator：状态的维护者

`IEnumerator` 是真正执行遍历的接口。

* **`MoveNext()`**：把遍历的游标移到下一个元素上。如果还有下一个，返回 `true`，否则返回 `false`。
* **`Current`**：读取游标当前指着的元素的内容。
* **`Reset()`**：把游标重置到 0，也就是初始化状态。

**关键点**：IEnumerator负责记住当前遍历到哪儿了，而不是让集合自己去记。

---

### 为什么要分开设计？

为什么不直接让集合类同时实现这两个接口？

**答案是为了支持“多重遍历”和“状态隔离”。**

如果在一个方法内有两个嵌套的循环遍历同一个集合，如果第一个 `foreach` 循环遍历到了第 50 个元素，这时候，第二个嵌套的 `foreach` 循环要开始遍历，第二个循环要重新获取一个新的游标从 0 开始遍历，而不和第一个循环发生冲突。

所以，`IEnumerable`（集合）负责提供数据，而每次调用 `GetEnumerator()` 都会创建一个新的 `IEnumerator`（游标），保证每个循环都有自己独立的进度条。

### 代码中的实际关系

在 C# 中，`foreach` 语法糖其实就是这两个接口配合的简写。

当你写：

```csharp
foreach (var item in myCollection) { ... }
```

编译器在底层实际上是这样执行的：

1. 调用 `myCollection.GetEnumerator()` 获取一个 `IEnumerator` 对象。
2. 循环调用 `enumerator.MoveNext()`。
3. 如果返回 `true`，通过 `enumerator.Current` 获取当前值。
4. 如果返回 `false`，结束循环。
