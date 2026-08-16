using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace EFCoreDemo.Demos;

/// <summary>
/// 演示 8：模型配置 —— 数据注解 / IEntityTypeConfiguration 配置类 /
/// 种子数据 HasData / 全局查询过滤 / 日志 LogTo。
///
/// 知识点：
///   · 数据注解：在实体属性上用特性配置映射（[Table]/[Column]/[Key]/[MaxLength]/[NotMapped]）
///   · 配置类：IEntityTypeConfiguration 把映射代码从实体中分离，ApplyConfigurationsFromAssembly 批量注册
///   · HasData：种子数据随迁移写入数据库，要求主键显式指定
///   · HasQueryFilter：全局查询过滤自动追加 WHERE 条件，IgnoreQueryFilters 可绕过
///   · LogTo：把 EF Core 生成的 SQL 输出到控制台，是排查查询问题的利器
///
/// 说明：本 Demo 运行前演示库需已由迁移创建（MigrateAsync 幂等保证）。
/// </summary>
public static class ConfigurationDemo
{
    public static async Task Run()
    {
        // ================= 1. 数据注解 =================
        Console.WriteLine("【1】数据注解：Product 的表名/列名由特性指定");
        using (var db = DemoDbPaths.CreateContext())
        {
            await db.Database.MigrateAsync();          // 确保演示库结构存在（幂等）

            // 元数据检查：表名、列名都来自特性
            var entityType = db.Model.FindEntityType(typeof(Product))!;
            var tableName = entityType.GetTableName();
            var columns = entityType.GetProperties()
                .Where(p => p.GetColumnName() != null)
                .Select(p => $"{p.Name} → {p.GetColumnName()}")
                .ToList();
            Console.WriteLine($"    表名：[Table(\"Product\")] → {tableName}");
            Console.WriteLine($"    列映射：{string.Join("，", columns)}");

            // 生成的 SQL 直接体现列名（product_id / product_name）
            var sql = db.Products
                .Where(p => p.Id > 0)
                .Select(p => new { p.Id, p.Name })
                .ToQueryString();
            Console.WriteLine("    生成的 SQL（列名来自 [Column] 特性）：");
            foreach (var line in sql.Split('\n'))
            {
                Console.WriteLine($"      {line}");
            }

            // [NotMapped] 属性不落库
            var product = new Product
            {
                Name = "演示商品",
                Price = 9.99m,
                TempData = "仅内存中的数据",
                CategoryId = 1,
            };
            await db.Products.AddAsync(product);
            await db.SaveChangesAsync();
            Console.WriteLine($"    已插入商品（Id={product.Id}），TempData 仅存内存、不写入数据库");
        }

        // ================= 2. IEntityTypeConfiguration 配置类 =================
        Console.WriteLine("【2】配置类：Category 的映射来自 CategoryConfiguration");
        using (var db = DemoDbPaths.CreateContext())
        {
            var entityType = db.Model.FindEntityType(typeof(Category))!;
            var maxLength = entityType.FindProperty(nameof(Category.Name))!.GetMaxLength();
            Console.WriteLine($"    表名：{entityType.GetTableName()}（由配置类 ToTable 指定）");
            Console.WriteLine($"    Name 最大长度：{maxLength}（由配置类 HasMaxLength(100) 指定）");
            Console.WriteLine("    OnModelCreating 中一行 ApplyConfigurationsFromAssembly 批量注册所有配置类");
        }

        // ================= 3. 种子数据 HasData =================
        Console.WriteLine("【3】种子数据：HasData 随迁移写入的分类");
        using (var db = DemoDbPaths.CreateContext())
        {
            var categories = await db.Categories
                .OrderBy(c => c.Id)
                .ToListAsync();
            foreach (var c in categories)
            {
                Console.WriteLine($"    {c.Id}. {c.Name}");
            }
            Console.WriteLine("    这些数据定义在 CategoryConfiguration.HasData 中，由迁移一并写入数据库");
        }

        // ================= 4. 全局查询过滤 =================
        Console.WriteLine("【4】全局查询过滤：软删除商品对普通查询不可见");
        using (var db = DemoDbPaths.CreateContext())
        {
            var total = await db.Products.CountAsync();
            var softDeleted = new Product
            {
                Name = "已下架商品",
                Price = 0.01m,
                IsDeleted = true,                    // 软删除标记
                CategoryId = 1,
            };
            await db.Products.AddAsync(softDeleted);
            await db.SaveChangesAsync();

            var normal = await db.Products.CountAsync();                // 自动过滤 IsDeleted=true
            var withFilterIgnored = await db.Products.IgnoreQueryFilters().CountAsync();
            Console.WriteLine($"    插入前商品数：{total}");
            Console.WriteLine($"    插入软删除商品后，普通查询：{normal}（自动追加 WHERE IsDeleted = 0）");
            Console.WriteLine($"    IgnoreQueryFilters 查询：{withFilterIgnored}（绕过过滤）");
        }

        // ================= 5. 日志 LogTo =================
        Console.WriteLine("【5】日志：LogTo 把 EF Core 生成的 SQL 输出到控制台");
        var options = new DbContextOptionsBuilder<DemoContext>()
            .UseSqlite($"Data Source={DemoDbPaths.DemoDbPath}")
            .LogTo(
                Console.WriteLine,
                new[] { RelationalEventId.CommandExecuted },   // 只输出 SQL 命令执行事件（默认 Information 级别）
                LogLevel.Information,
                DbContextLoggerOptions.SingleLine)   // 单行紧凑格式
            .Options;
        using (var db = new DemoContext(options))
        {
            Console.WriteLine("    下面两行是 EF Core 输出的日志（含生成的 SQL）：");
            var first = await db.Products.OrderBy(p => p.Id).FirstOrDefaultAsync();
            Console.WriteLine($"    查询结果：{(first is null ? "无" : first.Name)}");
        }

        // ================= 6. 小结 =================
        Console.WriteLine();
        Console.WriteLine("小结：四种配置方式按场景选用——简单映射用约定，实体局部配置用数据注解，复杂/集中管理用配置类；种子数据随迁移落库；全局过滤实现软删除；LogTo 排查 SQL。");
    }
}
