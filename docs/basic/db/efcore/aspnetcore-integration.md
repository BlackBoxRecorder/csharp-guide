---
title: "在 ASP.NET Core 中集成 EFCore"
description: "EF Core 与 ASP.NET Core 集成，包括依赖注入、连接池、多租户和单元测试。"
slug: "dotnet/orm/entityframeworkcore/aspnetcore-integration"
draft: false
sidebar:
  order: 7
---

EF Core 与 ASP.NET Core 有很好的集成，本文详细介绍如何在 ASP.NET Core 项目中配置和使用 EF Core，包括依赖注入、配置、最佳实践等内容。

## 1. 基本配置

### 1.1 安装 NuGet 包

首先安装数据库对应的 EF Core 提供程序，比如 SQL Server：

```bash
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
```

其他数据库的包：

```bash
# MySQL / MariaDB
dotnet add package Pomelo.EntityFrameworkCore.MySql
# SQLite
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
# PostgreSQL
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
```

### 1.2 配置 DbContext

在 `Program.cs` 中注册 DbContext 到依赖注入容器：

```csharp
var builder = WebApplication.CreateBuilder(args);

// 获取连接字符串
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 注册 DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
{
    // 配置数据库提供程序
    options.UseSqlServer(connectionString);
    
    // 开发环境启用敏感数据日志，方便调试
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
    }
});

// 注册控制器等其他服务
builder.Services.AddControllers();
```

在 `appsettings.json` 中配置连接字符串：

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=DemoDb;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

### 1.3 自定义 DbContext 类

```csharp
public class AppDbContext : DbContext
{
    // 构造函数注入 DbContextOptions
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
    
    // DbSet 属性
    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    
    // 模型配置
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 应用配置
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
```

> 详细的模型配置请参考 [EFCore 配置指南](./configuration-guide)

## 2. 进阶配置

### 2.1 使用 DbContext 池

DbContext 池可以重用 DbContext 实例，减少对象创建开销，提高性能：

```csharp
// 使用 AddDbContextPool 代替 AddDbContext
builder.Services.AddDbContextPool<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")),
    poolSize: 1024 // 池大小，默认是 1024
);
```

性能提升通常在 10%~30% 左右，推荐在生产环境启用。

### 2.2 配置 DbContext 生命周期

默认情况下，DbContext 的生命周期是 `Scoped`，即每个请求创建一个实例，这是推荐的配置。如果需要修改生命周期：

```csharp
builder.Services.AddDbContext<AppDbContext>(options => ..., 
    ServiceLifetime.Scoped,  // DbContext 的生命周期
    ServiceLifetime.Singleton // DbContextOptions 的生命周期
);
```

> **注意**：除非你很清楚自己在做什么，否则不要修改默认的生命周期配置。

### 2.3 多环境配置

可以针对不同环境配置不同的数据库连接：

```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        // 开发环境使用 SQLite
        options.UseSqlite("Data Source=dev.db");
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
    else
    {
        // 生产环境使用 SQL Server
        options.UseSqlServer(builder.Configuration.GetConnectionString("ProductionConnection"));
    }
});
```

## 3. 在控制器/服务中使用 DbContext

### 3.1 构造函数注入

在控制器或服务中通过构造函数注入 DbContext：

```csharp
[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    
    // 构造函数注入 DbContext
    public ProductsController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var products = await _dbContext.Products
            .AsNoTracking()
            .ToListAsync();
        return Ok(products);
    }
    
    [HttpPost]
    public async Task<IActionResult> Create(CreateProductDto dto)
    {
        var product = new Product
        {
            Name = dto.Name,
            Price = dto.Price,
            Stock = dto.Stock
        };
        
        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync();
        
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }
}
```

### 3.2 工作单元模式（Unit of Work）

对于复杂的业务操作，推荐使用工作单元模式来管理事务：

```csharp
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    IDbContextTransaction BeginTransaction();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _dbContext;
    
    public UnitOfWork(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }
    
    public IDbContextTransaction BeginTransaction()
    {
        return _dbContext.Database.BeginTransaction();
    }
    
    public async Task CommitTransactionAsync()
    {
        await _dbContext.Database.CommitTransactionAsync();
    }
    
    public async Task RollbackTransactionAsync()
    {
        await _dbContext.Database.RollbackTransactionAsync();
    }
}

// 注册到容器
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
```

使用示例：

```csharp
public class ProductService
{
    private readonly AppDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;
    
    public ProductService(AppDbContext dbContext, IUnitOfWork unitOfWork)
    {
        _dbContext = dbContext;
        _unitOfWork = unitOfWork;
    }
    
    public async Task CreateProductWithCategoryAsync(CreateProductDto productDto, CreateCategoryDto categoryDto)
    {
        using var transaction = _unitOfWork.BeginTransaction();
        try
        {
            // 创建分类
            var category = new Category { Name = categoryDto.Name };
            _dbContext.Categories.Add(category);
            await _unitOfWork.SaveChangesAsync();
            
            // 创建商品
            var product = new Product
            {
                Name = productDto.Name,
                Price = productDto.Price,
                CategoryId = category.Id
            };
            _dbContext.Products.Add(product);
            await _unitOfWork.SaveChangesAsync();
            
            // 提交事务
            await transaction.CommitAsync();
        }
        catch
        {
            // 回滚事务
            await transaction.RollbackAsync();
            throw;
        }
    }
}
```

## 4. 常见场景处理

### 4.1 在单例服务中使用 DbContext

DbContext 默认是 Scoped 生命周期，不能直接注入到 Singleton 服务中，会导致生命周期不匹配的错误。
> 错误信息：`Cannot consume scoped service 'AppDbContext' from singleton 'XXXService'`

**解决方案：**使用 `IServiceScopeFactory` 手动创建作用域：

```csharp
public class SingletonProductService
{
    private readonly IServiceScopeFactory _scopeFactory;
    
    public SingletonProductService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }
    
    public async Task DoSomethingAsync()
    {
        // 创建一个新的作用域
        using var scope = _scopeFactory.CreateScope();
        // 从作用域中获取 DbContext
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        // 使用 DbContext
        var products = await dbContext.Products.ToListAsync();
        
        // 作用域销毁时会自动释放 DbContext
    }
}
```


### 4.2 自动应用迁移

在开发环境可以配置应用启动时自动应用迁移：

```csharp
var app = builder.Build();

// 仅在开发环境自动应用迁移
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // 自动应用所有未应用的迁移
    await dbContext.Database.MigrateAsync();
    
    // 可选：自动生成种子数据
    await SeedDataAsync(dbContext);
}

app.Run();
```

> **注意**：生产环境不建议自动应用迁移，应该手动审核 SQL 脚本后再执行。

### 4.3 配置健康检查

可以添加 EF Core 数据库健康检查：

```csharp
// 安装健康检查包
// dotnet add package Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore

builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("数据库健康检查");

// 配置健康检查端点
app.MapHealthChecks("/health");
```

## 5. 性能优化最佳实践

### 5.1 启用查询缓存

EF Core 8.0+ 支持查询缓存，可以缓存 LINQ 查询的解析结果：

```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString)
           .EnableQueryCaching() // 启用查询缓存
);
```

### 5.2 批量操作优化

对于批量操作，使用 EF Core 原生的批量更新/删除方法：

```csharp
// 批量涨价
await _dbContext.Products
    .Where(p => p.CategoryId == categoryId)
    .ExecuteUpdateAsync(p => p.SetProperty(x => x.Price, x => x.Price * 1.1m));

// 批量删除
await _dbContext.Products
    .Where(p => p.Stock == 0)
    .ExecuteDeleteAsync();
```

### 5.3 仓库模式（Repository Pattern）

对于复杂项目，可以使用仓库模式封装数据访问：

```csharp
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<List<T>> GetAllAsync();
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
}

public class EfRepository<T> : IRepository<T> where T : class
{
    private readonly AppDbContext _dbContext;
    
    public EfRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<T?> GetByIdAsync(int id)
    {
        return await _dbContext.Set<T>().FindAsync(id);
    }
    
    public async Task<List<T>> GetAllAsync()
    {
        return await _dbContext.Set<T>().ToListAsync();
    }
    
    public async Task AddAsync(T entity)
    {
        await _dbContext.Set<T>().AddAsync(entity);
        await _dbContext.SaveChangesAsync();
    }
    
    // 其他方法...
}

// 注册
builder.Services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
```

## 6. 常见问题和解决方案

### 6.1 事务相关问题

#### 问题：多个操作需要事务保证原子性

**解决方案：**使用 EF Core 的事务功能，或者使用工作单元模式，参考前面的工作单元示例。

#### 问题：分布式事务

如果需要跨多个数据库或服务的分布式事务，可以使用：

- ADO.NET 的分布式事务（MSDTC）
- 事件溯源（Event Sourcing）+ 最终一致性
- Saga 模式

### 6.2 并发冲突处理

EF Core 支持乐观并发控制：

```csharp
// 配置并发令牌
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    
    // 并发令牌字段
    [ConcurrencyCheck]
    [Timestamp]
    public byte[] RowVersion { get; set; }
}
```

处理并发冲突：

```csharp
try
{
    product.Price = 99.99m;
    await _dbContext.SaveChangesAsync();
}
catch (DbUpdateConcurrencyException ex)
{
    var entry = ex.Entries.Single();
    var databaseValues = await entry.GetDatabaseValuesAsync();
    var clientValues = entry.CurrentValues;
    
    // 处理冲突，比如合并更改，或者提示用户
    foreach (var property in clientValues.Properties)
    {
        var databaseValue = databaseValues[property];
        var clientValue = clientValues[property];
        // 比较并处理冲突
    }
    
    // 重新保存
    await _dbContext.SaveChangesAsync();
}
```

> **配套示例**：`src/db/efcore` 演示项目的「乐观并发」演示（`dotnet run -- 11`）展示冲突复现与三种处理策略（以库为准 Reload / 强制覆盖 / 属性级合并）；SQLite 无原生 rowversion，示例用 `[ConcurrencyCheck]` 版本号实现。

### 6.3 连接池配置

可以配置数据库连接池大小：

```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString, 
        sqlOptions => sqlOptions.MaxPoolSize(100))); // 连接池大小，默认是 100
```

## 7. 项目结构建议

对于中大型项目，推荐的分层结构：

```
YourProject/
├── src/
│   ├── YourProject.Domain/          # 领域层：实体、值对象、领域服务
│   ├── YourProject.Application/     # 应用层：服务接口、DTO、业务逻辑
│   ├── YourProject.Infrastructure/  # 基础设施层：EF Core 配置、仓储实现
│   │   ├── Data/
│   │   │   ├── AppDbContext.cs
│   │   │   ├── Configurations/      # 实体配置
│   │   │   └── Migrations/          # 迁移文件
│   │   └── Repositories/            # 仓储实现
│   └── YourProject.Web/             # Web 层：控制器、Program.cs
└── tests/
    └── YourProject.Tests/           # 单元测试、集成测试
```
