---
title: "EFCore 核心概念"
description: "EF Core 核心概念，包括实体、DbContext、变更追踪、查询管道和保存数据。"
slug: "dotnet/orm/entityframeworkcore/core-concepts"
draft: false
---

本文详细介绍 EF Core 的核心组件和工作原理，帮助你深入理解 EF Core 的运行机制，为后续的高级用法打下基础。

## 1. DbContext

DbContext 是 EF Core 的核心类，它是应用程序与数据库之间的会话，负责管理实体对象、数据库连接、数据操作和事务处理。

### 主要职责

- 管理实体对象与数据库表之间的映射
- 跟踪实体对象的状态变化
- 执行数据库查询和数据修改操作
- 处理事务和并发控制
- 缓存查询结果

### DbContext 的生命周期

DbContext 是轻量级对象，创建和销毁的开销很小，推荐的使用方式是每个请求/操作创建一个新的实例，使用完成后立即释放。

```csharp
// 推荐的使用方式：using 语句会自动释放资源
using var dbContext = new AppDbContext();
// 执行操作
```

> **注意**：DbContext 不是线程安全的，不要在多个线程中同时使用同一个 DbContext 实例。

### DbContext 配置

DbContext 可以通过两种方式进行配置：

1. **重写 OnConfiguring 方法**：适合简单场景，直接在 DbContext 类中配置
2. **通过构造函数注入 DbContextOptions**：适合 ASP.NET Core 等依赖注入场景

```csharp
// 方式1：重写 OnConfiguring
public class AppDbContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=demo.db");
    }
}

// 方式2：构造函数注入
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
}
```

> 在 ASP.NET Core 中使用 EF Core 的详细配置请参考 [在 ASP.NET Core 中集成 EFCore](./aspnetcore-integration)

## 2. DbSet<TEntity>

`DbSet<TEntity>` 代表数据库中的一个表，每个实体类型对应一个 `DbSet` 属性，你可以通过 `DbSet` 来执行对该表的增删改查操作。

```csharp
public class AppDbContext : DbContext
{
    // 对应数据库中的 Products 表
    public DbSet<Product> Products { get; set; }
    // 对应数据库中的 Categories 表
    public DbSet<Category> Categories { get; set; }
}
```

### 常用操作方法

| 方法 | 作用 |
|------|------|
| `Add()` / `AddAsync()` | 添加新实体 |
| `AddRange()` / `AddRangeAsync()` | 批量添加实体 |
| `Update()` | 更新实体 |
| `UpdateRange()` | 批量更新实体 |
| `Remove()` | 删除实体 |
| `RemoveRange()` | 批量删除实体 |
| `Find()` / `FindAsync()` | 根据主键查询实体 |
| `ToList()` / `ToListAsync()` | 执行查询并返回结果列表 |
| `FirstOrDefault()` / `FirstOrDefaultAsync()` | 返回第一个匹配的实体或 null |

## 3. 实体类

实体类是映射到数据库表的普通 C# 类（POCO），不需要继承任何基类。EF Core 通过约定和配置来确定实体与数据库表的映射关系。

### 实体类约定

EF Core 遵循"约定大于配置"的原则，默认的约定包括：

- 表名默认是 `DbSet` 属性的名称，也可以是实体类名称的复数形式
- 名为 `Id` 或 `{实体名}Id` 的属性会被识别为主键
- 引用类型属性默认是可空的，值类型属性默认是不可空的
- 名为 `{导航属性名}Id` 的属性会被识别为外键

### 示例实体类

```csharp
public class Product
{
    // 主键
    public int Id { get; set; }
    
    // 必填字段
    public required string Name { get; set; }
    public decimal Price { get; set; }
    
    // 可空字段
    public string? Description { get; set; }
    
    // 外键
    public int CategoryId { get; set; }
    // 导航属性：一个商品属于一个分类
    public Category Category { get; set; } = null!;
}

public class Category
{
    public int Id { get; set; }
    public required string Name { get; set; }
    // 导航属性：一个分类有多个商品
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
```

## 4. 数据跟踪（Change Tracking）

EF Core 会自动跟踪从数据库中查询出来的实体的状态变化，当调用 `SaveChanges()` 方法时，会自动根据实体的状态生成对应的 SQL 语句。

### 实体状态

| 状态 | 说明 | SaveChanges 时的行为 |
|------|------|----------------------|
| `Added` | 新添加的实体 | 执行 INSERT 操作 |
| `Modified` | 实体的属性被修改 | 执行 UPDATE 操作 |
| `Deleted` | 实体被标记为删除 | 执行 DELETE 操作 |
| `Unchanged` | 实体没有变化 | 不执行任何操作 |
| `Detached` | 实体没有被 DbContext 跟踪 | 不执行任何操作 |

### 跟踪示例

```csharp
using var dbContext = new AppDbContext();

// 查询出来的实体默认会被跟踪
var product = await dbContext.Products.FindAsync(1);
// 此时状态是 Unchanged
Console.WriteLine(dbContext.Entry(product).State); // Unchanged

// 修改属性
product.Price = 99.99m;
// 此时状态变为 Modified
Console.WriteLine(dbContext.Entry(product).State); // Modified

// 保存更改，自动生成 UPDATE 语句
await dbContext.SaveChangesAsync();
```

### 无跟踪查询

如果只需要查询数据而不需要修改，可以使用 `AsNoTracking()` 方法来禁用跟踪，提高查询性能：

```csharp
// 无跟踪查询，性能更好
var products = await dbContext.Products
    .AsNoTracking()
    .ToListAsync();
```


## 5. 关系配置

EF Core 支持三种实体之间的关系：一对一、一对多、多对多。

### 5.1 一对多关系

最常见的关系，比如一个分类下有多个商品，一个商品属于一个分类：

```csharp
public class Category
{
    public int Id { get; set; }
    public string Name { get; set; }
    // 导航属性：一个分类有多个商品（一对多的"多"端）
    public ICollection<Product> Products { get; set; } = new List<Product>();
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    // 外键
    public int CategoryId { get; set; }
    // 导航属性：一个商品属于一个分类（一对多的"一"端）
    public Category Category { get; set; }
}
```

### 5.2 一对一关系

比如一个用户有一个用户详情：

```csharp
public class User
{
    public int Id { get; set; }
    public string UserName { get; set; }
    // 导航属性
    public UserProfile Profile { get; set; }
}

public class UserProfile
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    // 外键
    public int UserId { get; set; }
    // 导航属性
    public User User { get; set; }
}
```

### 5.3 多对多关系

EF Core 5.0+ 原生支持多对多关系，不需要显式定义中间表实体：

```csharp
public class Student
{
    public int Id { get; set; }
    public string Name { get; set; }
    // 导航属性：一个学生选了多门课程
    public ICollection<Course> Courses { get; set; } = new List<Course>();
}

public class Course
{
    public int Id { get; set; }
    public string Name { get; set; }
    // 导航属性：一门课程有多个学生选修
    public ICollection<Student> Students { get; set; } = new List<Student>();
}
```

EF Core 会自动创建中间表 `StudentCourse` 来存储两者的关系。

### 加载关联数据

EF Core 提供了三种加载关联数据的方式：

#### 1. 预先加载（Eager Loading）

在查询时使用 `Include` 和 `ThenInclude` 方法一次性加载关联数据：

```csharp
// 查询分类时同时加载该分类下的所有商品
var categories = await dbContext.Categories
    .Include(c => c.Products)
    .ToListAsync();

// 多级包含：查询商品时同时加载分类和分类的其他信息
var products = await dbContext.Products
    .Include(p => p.Category)
        .ThenInclude(c => c.OtherInfo)
    .ToListAsync();
```

#### 2. 显式加载（Explicit Loading）

在查询出主实体后，显式加载关联数据：

```csharp
var category = await dbContext.Categories.FindAsync(1);
// 显式加载该分类下的商品
await dbContext.Entry(category)
    .Collection(c => c.Products)
    .LoadAsync();
```

#### 3. 延迟加载（Lazy Loading）

在访问导航属性时自动加载关联数据，需要启用延迟加载功能：

```csharp
// 安装延迟加载代理包
// dotnet add package Microsoft.EntityFrameworkCore.Proxies

// 启用延迟加载
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    optionsBuilder
        .UseLazyLoadingProxies() // 启用延迟加载代理
        .UseSqlite("Data Source=demo.db");
}

// 使用时自动加载
var category = await dbContext.Categories.FindAsync(1);
// 访问 Products 属性时自动触发加载
var products = category.Products;
```

> **注意**：延迟加载容易导致 N+1 查询性能问题，谨慎使用。

> **配套示例**：`src/db/efcore` 演示项目的「进阶关系」演示（`dotnet run -- 9`）包含一对一、原生多对多与延迟加载代理的完整可运行代码。

## 6. 迁移（Migrations）

迁移是 EF Core 提供的一种管理数据库架构版本迭代的功能，允许你通过代码来定义数据模型的变化，然后自动更新数据库架构。

### 核心原理

EF Core 会比较当前数据模型和上次迁移时的快照，自动生成迁移代码，迁移代码包含了更新数据库架构所需的操作（创建表、添加列、删除列等）。

### 基本工作流

1. 修改数据模型（添加实体、修改属性等）
2. 创建新的迁移：`dotnet ef migrations add <迁移名称>`
3. 应用迁移到数据库：`dotnet ef database update`

> **配套示例**：`src/db/efcore` 演示项目的「迁移」演示（`dotnet run -- 10`）展示分步升级（v1 建表 → v2 加列）、迁移历史表与 EnsureCreated 的对比，迁移文件在项目的 `Migrations/` 目录。

## 下一步学习

- 学习 EF Core 的配置方法：[EFCore 配置指南](./configuration-guide)
