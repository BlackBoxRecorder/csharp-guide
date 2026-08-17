---
title: "EFCore 配置指南"
description: "EF Core 配置指南，包括 Fluent API、数据注释、关系配置和表映射策略。"
slug: "dotnet/orm/entityframeworkcore/configuration-guide"
draft: false
---

本文详细介绍 EF Core 的各种配置方式，包括数据库连接配置、DbContext 配置、日志配置、模型映射配置等内容。

## 1. 连接字符串配置

连接字符串是配置数据库连接的基础，不同的数据库有不同的连接字符串格式。

### 1.1 常用数据库连接字符串示例

#### SQLite

```json
"ConnectionStrings": {
  "DefaultConnection": "Data Source=demo.db;Cache=Shared"
}
```

#### SQL Server

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=DemoDb;Trusted_Connection=True;TrustServerCertificate=True"
}
```

#### MySQL / MariaDB

```json
"ConnectionStrings": {
  "DefaultConnection": "server=127.0.0.1;database=demo;user=root;password=123456;charset=utf8mb4;"
}
```

#### PostgreSQL

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Database=demo;Username=postgres;Password=123456"
}
```

### 1.2 安全配置建议

**不要在代码中硬编码连接字符串**，推荐使用以下方式存储连接字符串：

1. 配置文件（appsettings.json）
2. 环境变量
3. 密钥管理器（Secret Manager）
4. 云配置中心（如 Azure Key Vault、阿里云配置中心等）


### 1.3 读取连接字符串

在 ASP.NET Core 中，可以通过 `IConfiguration` 接口读取连接字符串：

```csharp
var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));
```

在控制台应用中：

```csharp
var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables()
    .Build();
var connectionString = config.GetConnectionString("DefaultConnection");
```

## 2. DbContext 配置

DbContext 是 EF Core 的核心，有多种配置方式。

### 2.1 配置方式

#### 方式1：重写 OnConfiguring 方法

适合简单应用和控制台应用：

```csharp
public class AppDbContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite("Data Source=demo.db");
        }
    }
}
```

#### 方式2：构造函数注入 DbContextOptions

适合 ASP.NET Core 等使用依赖注入的场景：

```csharp
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
}

// 在 Program.cs 中注册
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```

### 2.2 DbContext 生命周期配置

在 ASP.NET Core 中注册 DbContext 时，可以指定生命周期：

```csharp
// 默认是 Scoped，每个请求一个实例（推荐）
builder.Services.AddDbContext<AppDbContext>(options => ...);

// 显式指定生命周期
builder.Services.AddDbContext<AppDbContext>(options => ..., ServiceLifetime.Scoped);
builder.Services.AddDbContext<AppDbContext>(options => ..., ServiceLifetime.Transient);
builder.Services.AddDbContext<AppDbContext>(options => ..., ServiceLifetime.Singleton);
```

> **注意**：将 DbContext 注册为 Singleton 时需要特别小心，因为 DbContext 不是线程安全的，并且会缓存数据导致数据不一致。

### 2.3 DbContext 池配置

使用 DbContext 池可以重用 DbContext 实例，提高性能：

```csharp
// 启用 DbContext 池，默认池大小是 1024
builder.Services.AddDbContextPool<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 指定池大小
builder.Services.AddDbContextPool<AppDbContext>(options => ..., poolSize: 2048);
```


## 3. 日志配置

配置 EF Core 的日志可以帮助你查看生成的 SQL 语句、查询性能等信息。

### 3.1 简单日志配置

在 `OnConfiguring` 方法中配置日志输出到控制台：

```csharp
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    optionsBuilder
        .UseSqlite("Data Source=demo.db")
        .LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information);
}
```

### 3.2 更详细的日志配置

可以指定只输出特定类别的日志：

```csharp
optionsBuilder.LogTo(
    Console.WriteLine,
    // 只输出 EF Core 数据库命令相关的日志
    new[] { Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.CommandExecuting },
    // 日志级别
    LogLevel.Information,
    // 配置日志格式
    DbContextLoggerOptions.DefaultWithLocalTime | DbContextLoggerOptions.SingleLine
);
```

### 3.3 在 ASP.NET Core 中配置日志

在 `appsettings.json` 中配置 EF Core 日志级别：

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.EntityFrameworkCore": "Information",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information" // 只输出 SQL 语句
    }
  }
}
```

## 4. 模型配置

EF Core 提供了三种方式来配置实体与数据库的映射关系：约定、数据注解（特性）、Fluent API。

### 4.1 约定（Conventions）

EF Core 有一套默认的约定，按照约定命名实体和属性可以减少配置工作：

- 表名默认是 `DbSet` 属性名或实体类名的复数形式
- 名为 `Id` 或 `{实体名}Id` 的属性会被识别为主键
- 可空类型对应数据库可空字段，非可空类型对应非空字段
- 字符串属性默认映射到 `nvarchar(max)`（SQL Server）或 `text`（其他数据库）

### 4.2 数据注解（Data Annotations）

使用特性直接在实体类上配置映射关系：

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("t_products")] // 指定表名
public class Product
{
    [Key] // 指定主键
    [Column("product_id")] // 指定列名
    public int Id { get; set; }
    
    [Required] // 必填字段
    [MaxLength(100)] // 最大长度100
    [Column("product_name")]
    public string Name { get; set; }
    
    [Column(TypeName = "decimal(18,2)")] // 指定列类型
    public decimal Price { get; set; }
    
    [NotMapped] // 该属性不映射到数据库
    public string TempData { get; set; }
    
    [Index("IX_Category_Sort", IsUnique = false)] // 添加索引
    public int CategoryId { get; set; }
}
```

常用数据注解特性：

| 特性 | 作用 |
|------|------|
| `[Table]` | 指定表名 |
| `[Column]` | 指定列名、列类型 |
| `[Key]` | 指定主键 |
| `[Required]` | 指定字段为必填 |
| `[MaxLength]` | 指定字符串最大长度 |
| `[NotMapped]` | 该属性不映射到数据库 |
| `[ForeignKey]` | 指定外键属性 |
| `[Index]` | 添加索引 |

### 4.3 Fluent API

Fluent API 是在 `OnModelCreating` 方法中配置模型，功能最强大，可以实现复杂的配置：

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // 配置 Product 实体
    modelBuilder.Entity<Product>(entity =>
    {
        // 指定表名
        entity.ToTable("t_products");
        
        // 配置主键
        entity.HasKey(p => p.Id);
        entity.Property(p => p.Id).HasColumnName("product_id");
        
        // 配置属性
        entity.Property(p => p.Name)
            .HasColumnName("product_name")
            .IsRequired()
            .HasMaxLength(100);
            
        entity.Property(p => p.Price)
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0.00m);
            
        // 添加索引
        entity.HasIndex(p => p.Name).IsUnique();
        entity.HasIndex(p => p.CategoryId);
        
        // 配置关系：一个商品属于一个分类
        entity.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Cascade); // 级联删除
    });
    
    // 配置种子数据
    modelBuilder.Entity<Category>().HasData(
        new Category { Id = 1, Name = "手机" },
        new Category { Id = 2, Name = "电脑" },
        new Category { Id = 3, Name = "配件" }
    );
}
```

### 4.4 单独的配置类

对于复杂的模型，建议将配置代码提取到单独的 `IEntityTypeConfiguration<T>` 类中，使代码更清晰：

```csharp
// ProductConfiguration.cs
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("t_products");
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);
            
        // 其他配置...
    }
}

// 在 OnModelCreating 中应用配置
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.ApplyConfiguration(new ProductConfiguration());
    // 或者自动应用当前程序集中的所有配置
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
}
```

## 5. 其他常用配置

### 5.1 全局查询过滤

可以为实体类型配置全局查询过滤器，自动应用到所有查询中：

```csharp
modelBuilder.Entity<Product>()
    .HasQueryFilter(p => !p.IsDeleted); // 自动过滤已删除的商品
```

常见应用场景：软删除、多租户、数据权限控制等。

### 5.2 约定配置

可以修改 EF Core 的默认约定：

```csharp
// 统一配置所有字符串属性的最大长度为 256
modelBuilder.Properties<string>()
    .Configure(p => p.HasMaxLength(256));
    
// 统一配置所有 decimal 类型的精度
modelBuilder.Properties<decimal>()
    .Configure(p => p.HasPrecision(18, 2));
```

### 5.3 敏感数据日志保护

启用敏感数据保护，避免在日志中泄露敏感信息：

```csharp
optionsBuilder.EnableSensitiveDataLogging(false); // false 表示不记录敏感数据
```

生产环境建议禁用敏感数据日志。

> **配套示例**：`src/db/efcore` 演示项目的「模型配置」演示（`dotnet run -- 8`）覆盖数据注解、配置类（ApplyConfigurationsFromAssembly）、HasData 种子数据、全局查询过滤（含 IgnoreQueryFilters）与 LogTo SQL 日志。

## 下一步学习

- 学习在 ASP.NET Core 中使用 EF Core：[在 ASP.NET Core 中集成 EFCore](./aspnetcore-integration)
