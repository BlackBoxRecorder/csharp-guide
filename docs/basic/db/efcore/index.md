---
title: "Entity Framework Core 简介"
slug: "dotnet/orm/entityframeworkcore"
draft: false
sidebar:
  order: 1
---

Entity Framework Core (简称 EF Core) 是微软官方推出的轻量级、跨平台、开源的对象关系映射（ORM）框架，是 .NET 生态中最主流的数据库访问技术。它允许开发者使用 .NET 对象来操作数据库，无需编写大量原生 SQL 代码，极大地提高了数据访问层的开发效率。

## 什么是 ORM？

对象关系映射（Object-Relational Mapping，ORM）是一种为了解决面向对象编程与关系型数据库数据不匹配问题的技术。它在对象模型和数据库表之间建立了映射关系，使得开发者可以直接使用面向对象的方式来操作数据库，而不需要关注底层数据库的实现细节。

ORM 的核心优势：

- 消除了重复的 SQL 编写工作，提高开发效率
- 提供了数据访问的抽象层，方便切换不同数据库
- 自动处理参数化查询，避免 SQL 注入风险
- 简化了数据模型的维护和版本管理

## EF Core 发展历史

EF Core 是 Entity Framework 的新一代版本，于 2016 年首次发布，相较于传统的 EF 6.x，它进行了完全的重写，具有更好的性能、跨平台支持和更灵活的架构。

| 版本 | 发布时间 | 对应 .NET 版本 | 主要特性 |
|------|----------|----------------|----------|
| EF Core 1.0 | 2016年6月 | .NET Core 1.0 | 初始版本，支持基本 ORM 功能 |
| EF Core 2.0 | 2017年8月 | .NET Core 2.0 | 延迟加载、全局查询过滤、表拆分 |
| EF Core 3.0 | 2019年9月 | .NET Core 3.0 | LINQ 查询重写、性能大幅提升 |
| EF Core 5.0 | 2020年11月 | .NET 5 | 多对多关系、迁移打包、性能优化 |
| EF Core 6.0 | 2021年11月 | .NET 6 | 原生 SQL 查询改进、批量操作、性能提升40% |
| EF Core 7.0 | 2022年11月 | .NET 7 | 批量更新删除、复杂类型、JSON 列支持 |
| EF Core 8.0 | 2023年11月 | .NET 8 | 原生 AOT 支持、值对象、查询性能优化 |
| EF Core 9.0 | 2024年11月 | .NET 9 | 进一步性能优化、云原生场景支持增强 |

## EF Core 核心特性

### 1. 跨平台支持

EF Core 可以在 Windows、macOS 和 Linux 上运行，支持 .NET Core/.NET 5+ 所有应用模型，包括 ASP.NET Core、Blazor、WPF、WinForms、MAUI 等。

### 2. 广泛的数据库支持

EF Core 通过数据库提供程序（Provider）的模式支持几乎所有主流关系型数据库：

- SQL Server / SQL Azure
- MySQL / MariaDB
- PostgreSQL
- SQLite
- Oracle
- DB2
- Cosmos DB（NoSQL）
- 还有众多第三方数据库支持

### 3. 丰富的查询功能

- 支持 LINQ（Language Integrated Query）查询，编写强类型的查询语句
- 自动生成 SQL 语句，支持复杂查询、连接查询、分组聚合等
- 支持原生 SQL 查询，方便处理复杂业务场景
- 提供查询跟踪和无跟踪模式，灵活控制性能

### 4. 数据迁移功能

内置数据迁移（Migrations）功能，可以通过代码来管理数据库架构的版本迭代，支持自动生成迁移脚本、升级和回滚数据库。

### 5. 性能优化特性

- DbContext 池：重用 DbContext 实例，减少对象创建开销

- 批量操作：支持批量增删改，减少数据库往返次数
- 查询缓存：支持二级缓存，提高重复查询性能
- 编译查询：预编译 LINQ 查询，提高高频查询性能

### 6. 灵活的配置方式

- 支持特性（Data Annotation）配置实体映射
- 支持 Fluent API 方式进行更复杂的配置
- 支持约定大于配置，减少重复配置工作

## EF Core 适用场景

### 推荐使用场景

- 大多数业务系统的 CRUD 操作场景
- 快速开发的项目，需要提高开发效率
- 需要支持多种数据库的应用
- 团队熟悉 LINQ 语法和面向对象编程
- 企业级应用，需要完善的数据访问层架构

### 不推荐使用场景

- 超高性能要求的系统，需要极致优化 SQL 的场景
- 大量复杂报表查询、批量数据处理的场景
- 只需要简单数据访问的小型工具类应用

## 与其他 ORM 对比

| 特性 | EF Core | Dapper | NHibernate | FreeSQL |
|------|---------|--------|------------|---------|
| 开发效率 | ⭐⭐⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐⭐⭐ |
| 性能 | ⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐⭐ |
| 功能丰富度 | ⭐⭐⭐⭐⭐ | ⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐⭐⭐ |
| 社区活跃度 | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐⭐ |
| 官方支持 | ⭐⭐⭐⭐⭐ | ⭐⭐⭐ | ⭐ | ⭐⭐ |
| 学习成本 | 中等 | 低 | 高 | 中等 |

## 学习资源推荐

### 官方文档

- [微软官方 EF Core 文档](https://learn.microsoft.com/zh-cn/ef/core/)
- [Entity Framework Tutorial](https://www.entityframeworktutorial.net/efcore/entity-framework-core.aspx)
- [Learn Entity Framework Core](https://www.learnentityframeworkcore.com/)

### 扩展库

- [Entity Framework Plus](https://entityframework-plus.net/)：增强 EF Core 功能，包含批量操作、查询缓存等
- [Entity Framework Extensions](https://entityframework-extensions.net/)：高性能批量操作扩展
- [EFCore.BulkExtensions](https://github.com/borisdj/EFCore.BulkExtensions)：开源的批量操作扩展

---

## 下一步学习

- 📚 [EFCore 核心概念](./core-concepts)：深入了解 EF Core 的核心组件和工作原理
- ⚙️ [EFCore 配置指南](./configuration-guide)：学习如何配置 EF Core 和数据库连接
