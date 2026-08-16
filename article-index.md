# 文章索引

本站 docs 目录下全部文章的标题、描述与链接索引。

## 使用说明

- 写作前请先查阅本文档，确认目标内容是否已有文章覆盖，避免内容重复；引用已有文章时使用本文档中的链接。
- 修改 docs 后运行 `npm run docs:index` 重新生成本文档。
- 本文档链接为项目根目录相对路径（无扩展名），在文章内引用时需去掉 `docs/` 前缀，并按文章所在层级添加 `../`。
  例如：本文档链接 `./docs/basic/oop/function`，在 `docs/basic/types/record.md` 中引用时应写作 `./../oop/function`。

共收录 51 篇文章，最后更新：2026-08-16

## (根目录)

| 标题 | 描述 | 链接 |
| :--- | :--- | :--- |
| index | （无描述，请补充） | [./docs/index](./docs/index) |

## basic

| 标题 | 描述 | 链接 |
| :--- | :--- | :--- |
| C# 基础指南 | C# 语言基础教程，涵盖类型系统、面向对象、委托与事件、集合、迭代与 LINQ、反射、异步并发及数据访问等核心主题。 | [./docs/basic/index](./docs/basic/index) |

## basic/collections

| 标题 | 描述 | 链接 |
| :--- | :--- | :--- |
| C# 中的常用数据结构 | C# 常用数据结构详解，包括 List、Dictionary、Queue、Stack、LinkedList、HashSet、SortedList 等的使用场景和最佳实践。 | [./docs/basic/collections/basic](./docs/basic/collections/basic) |
| BlockingCollection | C# BlockingCollection 详解，线程安全的生产者-消费者集合，包括阻塞和限制功能、并发编程实践。 | [./docs/basic/collections/BlockingCollection](./docs/basic/collections/BlockingCollection) |
| Channel | C# Channel 详解，基于 System.Threading.Channels 的线程安全生产者-消费者数据结构，支持异步读写。 | [./docs/basic/collections/Channel](./docs/basic/collections/Channel) |
| 内存与高性能处理 | C# 内存与高性能处理类型详解，包括 Memory、ArraySegment、StringBuilder、Buffer 的用法、与 Span 的配合及选型建议。 | [./docs/basic/collections/memory-and-performance](./docs/basic/collections/memory-and-performance) |
| 只读与不可变集合 | C# 只读与不可变集合详解，ReadOnlyCollection 包装器、Frozen 系列与 System.Collections.Immutable 不可变集合的用法和选型。 | [./docs/basic/collections/readonly-and-immutable](./docs/basic/collections/readonly-and-immutable) |
| 有序集合：SortedList 与 SortedDictionary | C# 有序集合详解，SortedList 与 SortedDictionary 的实现原理、性能对比和适用场景，以及与 SortedSet 的关系。 | [./docs/basic/collections/sorted-collections](./docs/basic/collections/sorted-collections) |
| 专用集合 | C# 专用集合详解，包括 Collection 基类、KeyedCollection、ObservableCollection、Deque、ConcurrentBag、OrderedDictionary 及遗留非泛型集合。 | [./docs/basic/collections/specialized-collections](./docs/basic/collections/specialized-collections) |

## basic/db

| 标题 | 描述 | 链接 |
| :--- | :--- | :--- |
| 在 ASP.NET Core 中集成 EFCore | EF Core 与 ASP.NET Core 集成，包括依赖注入、连接池、多租户和单元测试。 | [./docs/basic/db/efcore/aspnetcore-integration](./docs/basic/db/efcore/aspnetcore-integration) |
| EFCore 配置指南 | EF Core 配置指南，包括 Fluent API、数据注释、关系配置和表映射策略。 | [./docs/basic/db/efcore/configuration-guide](./docs/basic/db/efcore/configuration-guide) |
| EFCore 核心概念 | EF Core 核心概念，包括实体、DbContext、变更追踪、查询管道和保存数据。 | [./docs/basic/db/efcore/core-concepts](./docs/basic/db/efcore/core-concepts) |
| Entity Framework Core 简介 | （无描述，请补充） | [./docs/basic/db/efcore/index](./docs/basic/db/efcore/index) |

## basic/delegates

| 标题 | 描述 | 链接 |
| :--- | :--- | :--- |
| 委托 | C# 委托（Delegate）的完整学习指南，包括基本概念、多播委托、匿名方法、Lambda 表达式等。 | [./docs/basic/delegates/delegate](./docs/basic/delegates/delegate) |
| 事件 | C# 事件（Event）机制详解，包括事件声明、订阅、触发、事件访问器、自定义事件参数等。 | [./docs/basic/delegates/event](./docs/basic/delegates/event) |
| Func 和 Action | C# 中 Func 和 Action 委托的详细用法，包括泛型参数、Lambda 表达式、实际应用场景。 | [./docs/basic/delegates/func-and-action](./docs/basic/delegates/func-and-action) |

## basic/linq

| 标题 | 描述 | 链接 |
| :--- | :--- | :--- |
| IEnumerable & IEnumerator | C# 中 IEnumerable 和 IEnumerator 接口的深入解析，包括迭代器模式、yield 关键字、协变与逆变等。 | [./docs/basic/linq/IEnumerable-and-IEnumerator](./docs/basic/linq/IEnumerable-and-IEnumerator) |
| 迭代器 | C# 迭代器（Iterator）详解，包括 yield return/yield break 的使用、延迟执行原理、状态机机制等。 | [./docs/basic/linq/Iterator](./docs/basic/linq/Iterator) |
| LINQ | C# LINQ（语言集成查询）详解，包括标准查询操作符、延迟执行、分组、连接、聚合等核心用法。 | [./docs/basic/linq/LINQ](./docs/basic/linq/LINQ) |

## basic/oop

| 标题 | 描述 | 链接 |
| :--- | :--- | :--- |
| 抽象类 | C# 抽象类和密封类详解，包括 abstract 和 sealed 关键字、抽象成员、密封方法、sealed record 及与接口的选择。 | [./docs/basic/oop/abstract](./docs/basic/oop/abstract) |
| 函数与方法 | C# 函数与方法详解，包括方法定义与调用、参数传递（ref/out/in）、返回值、委托与 Lambda、局部函数、异步方法、虚方法与多态。 | [./docs/basic/oop/function](./docs/basic/oop/function) |
| 面向对象编程 | C# 面向对象编程的核心概念，包括抽象类、接口、函数与方法、索引器、多态等。 | [./docs/basic/oop/index](./docs/basic/oop/index) |
| 索引器 | C# 索引器（Indexer）详解，包括基本用法、多参数索引器、接口实现、与属性的对比等。 | [./docs/basic/oop/indexer](./docs/basic/oop/indexer) |
| 接口 | C# 接口（Interface）详解，包括接口定义、实现、默认实现、多接口继承、接口与抽象类的区别。 | [./docs/basic/oop/interface](./docs/basic/oop/interface) |

## basic/reflection

| 标题 | 描述 | 链接 |
| :--- | :--- | :--- |
| C# 反射 | C# 反射（Reflection）详解，包括类型信息获取、动态创建对象、方法调用、特性（Attribute）使用、插件系统实现等。 | [./docs/basic/reflection/reflection](./docs/basic/reflection/reflection) |

## basic/types

| 标题 | 描述 | 链接 |
| :--- | :--- | :--- |
| 常用内置类型 | C# 常用内置类型详解，包括 BigInteger、Complex、Half、Int128 等数值类型，时间类型、Guid、Uri、Range 与 Index、Nullable、Lazy 的用法。 | [./docs/basic/types/common-types](./docs/basic/types/common-types) |
| ExpandoObject | C# 动态类型 ExpandoObject 的使用详解，包括动态属性添加、JSON 反序列化、与 DataTable 互转等。 | [./docs/basic/types/ExpandoObject](./docs/basic/types/ExpandoObject) |
| record | C# record 类型详解：不可变数据模型、值相等性、非破坏性修改等核心特性，以及与 readonly struct 的对比和选型建议。 | [./docs/basic/types/record](./docs/basic/types/record) |
| ref、in、out 参数传递 | C# 中 ref、in、out 参数修饰符的详细对比与使用场景，涵盖值传递与引用传递的区别、out 的 TryParse 模式以及 in 的只读引用传递性能优化。 | [./docs/basic/types/ref-in-out](./docs/basic/types/ref-in-out) |
| C#中的结构体 | C# 结构体（struct）详解，包括值类型特性、内存管理、与类的区别、适用场景和最佳实践。 | [./docs/basic/types/struct](./docs/basic/types/struct) |

## communication

| 标题 | 描述 | 链接 |
| :--- | :--- | :--- |
| 网络通信 | C# 网络通信编程指南，涵盖 TCP、UDP、MQTT、串口通信等协议的原理与实现。 | [./docs/communication/index](./docs/communication/index) |

## communication/mqtt

| 标题 | 描述 | 链接 |
| :--- | :--- | :--- |
| communication/mqtt/index | （无描述，请补充） | [./docs/communication/mqtt/index](./docs/communication/mqtt/index) |

## communication/tcp

| 标题 | 描述 | 链接 |
| :--- | :--- | :--- |
| TCP 基础概念 | 了解 TCP 通信前必须掌握的基础概念：IP 与端口、Socket 与操作系统内核、收发缓冲区以及各类超时机制。 | [./docs/communication/tcp/tcp-basics](./docs/communication/tcp/tcp-basics) |
| TCP 序列号与字节流 | TCP 的序列号是字节编号而非包编号，报文段大小由 MSS、窗口、拥塞与 Nagle 算法共同决定；应用层面对的是无边界的字节流，需要自己定义消息边界。 | [./docs/communication/tcp/tcp-byte-stream](./docs/communication/tcp/tcp-byte-stream) |
| TCP 拥塞控制与流量控制 | 拥塞窗口如何控制发送速度，接收端如何通过窗口字段实现流量控制（背压），以及零窗口、窗口探测与窗口缩放等细节。 | [./docs/communication/tcp/tcp-congestion](./docs/communication/tcp/tcp-congestion) |
| TCP 连接的建立与断开 | 三次握手如何建立连接、四次挥手如何断开连接，以及为什么握手必须 3 次、挥手必须 4 次。 | [./docs/communication/tcp/tcp-connection](./docs/communication/tcp/tcp-connection) |
| TCP 连接断开检测与重连 | 拔网线、崩溃等意外情况下 TCP 如何发现连接已断；send 返回成功为什么不代表发送成功；C# 中如何正确处理断线并设计重连策略。 | [./docs/communication/tcp/tcp-disconnect](./docs/communication/tcp/tcp-disconnect) |
| TCP/IP 报文结构 | 理解 IP 头与 TCP 头中每个字段的作用，了解数据从应用层到网线的逐层封装过程，以及 MSS、SACK 等 TCP 选项。 | [./docs/communication/tcp/tcp-header](./docs/communication/tcp/tcp-header) |
| Linux 服务器如何支持 TCP 高并发 | 为什么 TCP 连接是"轻"资源，单机并发上限受什么限制；Linux IO 模型如何演进；以及文件描述符与内核网络参数的调优方法。 | [./docs/communication/tcp/tcp-high-concurrency](./docs/communication/tcp/tcp-high-concurrency) |
| TCP 可靠性机制 | TCP 如何通过编号、确认、重传与缓冲区排序保证数据完整有序地到达；接收端的交付规则与累积确认、快速重传的运作方式。 | [./docs/communication/tcp/tcp-reliability](./docs/communication/tcp/tcp-reliability) |

## language-features

| 标题 | 描述 | 链接 |
| :--- | :--- | :--- |
| C# 10.0 新特性 | C# 10.0 版本引入的所有新特性详细说明，包含记录结构、插值字符串处理程序、全局using指令、文件范围命名空间等特性的代码示例和最佳实践 | [./docs/language-features/csharp-10.0](./docs/language-features/csharp-10.0) |
| C# 11.0 新特性 | C# 11.0 版本引入的所有新特性详细说明，包含原始字符串字面量、泛型数学支持、泛型属性、UTF-8字符串字面量等特性的代码示例和最佳实践 | [./docs/language-features/csharp-11.0](./docs/language-features/csharp-11.0) |
| C# 12.0 新特性 | C# 12.0 版本引入的所有新特性详细说明，包含主构造函数、集合表达式、内联数组、Lambda可选参数等特性的代码示例和最佳实践 | [./docs/language-features/csharp-12.0](./docs/language-features/csharp-12.0) |
| C# 13.0 新特性 | C# 13.0 版本引入的所有新特性详细说明，包含扩展属性、any 类型别名、params 集合表达式等特性的代码示例和最佳实践 | [./docs/language-features/csharp-13.0](./docs/language-features/csharp-13.0) |
| C# 14.0 新特性（预览版） | C# 14.0 版本（预计2025年11月随.NET 10发布）的新特性预览，包含主构造函数改进、集合表达式增强等最新特性说明 | [./docs/language-features/csharp-14.0](./docs/language-features/csharp-14.0) |
| C# 8.0 新特性 | C# 8.0 版本引入的所有新特性详细说明，包含可空引用类型、异步流、默认接口方法等特性的代码示例和最佳实践 | [./docs/language-features/csharp-8.0](./docs/language-features/csharp-8.0) |
| C# 9.0 新特性 | C# 9.0 版本引入的所有新特性详细说明，包含记录类型、init-only属性、顶级语句、模式匹配增强等特性的代码示例和最佳实践 | [./docs/language-features/csharp-9.0](./docs/language-features/csharp-9.0) |
| 语言特性 | C# 各版本的语言特性演进，从 C# 8.0 到 C# 14.0 的新特性详解。 | [./docs/language-features/index](./docs/language-features/index) |

## libraries

| 标题 | 描述 | 链接 |
| :--- | :--- | :--- |
| 第三方库 | 常用 C# 第三方库的使用笔记，涵盖图像处理、序列化、日志、依赖注入等库的实践与踩坑记录。 | [./docs/libraries/index](./docs/libraries/index) |

## webapi

| 标题 | 描述 | 链接 |
| :--- | :--- | :--- |
| Web 开发 | C# Web 开发教程，涵盖 ASP.NET Core Web API、gRPC、SignalR 等后端服务开发技术。 | [./docs/webapi/index](./docs/webapi/index) |

## wpf

| 标题 | 描述 | 链接 |
| :--- | :--- | :--- |
| 桌面开发 | C# 桌面应用开发教程，涵盖 WPF、MVVM 模式、数据绑定与 UI 设计。 | [./docs/wpf/index](./docs/wpf/index) |
