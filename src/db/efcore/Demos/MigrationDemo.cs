using Microsoft.EntityFrameworkCore;

namespace EFCoreDemo.Demos;

/// <summary>
/// 演示 10：迁移 —— 分步升级（v1 → v2）/ 迁移历史表 / 数据保留 / EnsureCreated 对比。
///
/// 知识点：
///   · Database.Migrate 按 __EFMigrationsHistory 记录增量应用迁移，可指定目标版本
///   · 升级只改结构不动数据：v1 写入的数据在 v2 升级后原样保留
///   · EnsureCreated 也能建表并写入 HasData 数据，但没有迁移历史表，适合原型快速起步
///   · 迁移文件是普通 C# 代码（Migrations/ 目录），提交进仓库后可审查、可回放
///
/// 说明：演示库在启动时已被删除，本演示从空库开始完整走一遍迁移生命周期。
/// </summary>
public static class MigrationDemo
{
    public static async Task Run()
    {
        using var db = DemoDbPaths.CreateContext();

        // ================= 1. 分步迁移：先 v1 再 v2 =================
        Console.WriteLine("【1】分步迁移：先应用 InitialCreate（v1），再升级到最新（v2 加列）");

        // 第一步：只应用 v1 初始迁移
        await db.Database.MigrateAsync("InitialCreate");
        Console.WriteLine("    已应用 InitialCreate（v1）：创建全部表 + 写入种子数据");
        Console.WriteLine($"    当前迁移历史（{await HistoryCountAsync(db)} 条）：");
        foreach (var id in await GetHistoryAsync(db))
        {
            Console.WriteLine($"      · {id}");
        }

        // 第二步：v1 阶段写入一条数据（此时模型还没有 Description 列，用原始 SQL 写入）
        await db.Database.ExecuteSqlRawAsync(
            "INSERT INTO Product (product_name, Price, CategoryId, IsDeleted, Version) VALUES ('迁移前的商品', 1.00, 1, 0, 0)");
        Console.WriteLine("    在 v1 结构下插入一条商品（原始 SQL，不依赖新列）");

        // 第三步：升级到最新（v2：Product 加 Description 列）
        await db.Database.MigrateAsync();
        Console.WriteLine("    已升级到最新（v2：AddProductDescription → Product 表新增 Description 列）");
        Console.WriteLine($"    当前迁移历史（{await HistoryCountAsync(db)} 条）：");
        foreach (var id in await GetHistoryAsync(db))
        {
            Console.WriteLine($"      · {id}");
        }

        // ================= 2. 升级保数据 =================
        Console.WriteLine("【2】升级只改结构不动数据");
        var hasDescription = await db.Database
            .SqlQueryRaw<int>("SELECT COUNT(*) AS Value FROM pragma_table_info('Product') WHERE name = 'Description'")
            .SingleAsync();
        // 用原始 SQL 验证（不依赖模型属性）：v1 写入的行数 + 新列 Description 的值
        var descValues = await db.Database
            .SqlQueryRaw<string>("SELECT COALESCE(Description, '(NULL)') AS Value FROM Product WHERE product_name = '迁移前的商品'")
            .ToListAsync();
        Console.WriteLine($"    Product 表是否包含 Description 列：{hasDescription == 1}");
        Console.WriteLine($"    v1 写入的商品仍存在：{(descValues.Count == 0 ? "丢失（异常！）" : $"存在 {descValues.Count} 条，新列 Description 值为 {descValues[0]}")}");

        // ================= 3. EnsureCreated 对比 =================
        Console.WriteLine("【3】EnsureCreated 对比：建表但无迁移历史表");
        var tempPath = Path.Combine(AppContext.BaseDirectory, "EnsureCreatedTest.sqlite");
        try
        {
            var tempOptions = new DbContextOptionsBuilder<DemoContext>()
                .UseSqlite($"Data Source={tempPath};Pooling=False")   // 关闭连接池，便于结束后删除临时文件
                .Options;
            using (var temp = new DemoContext(tempOptions))
            {
                await temp.Database.EnsureCreatedAsync();   // 直接按当前模型建表
                var hasHistoryTable = await temp.Database
                    .SqlQueryRaw<int>("SELECT COUNT(*) AS Value FROM sqlite_master WHERE type = 'table' AND name = '__EFMigrationsHistory'")
                    .SingleAsync();
                var seedCount = await temp.Categories.CountAsync();
                Console.WriteLine($"    EnsureCreated 建库后：迁移历史表 {(hasHistoryTable == 1 ? "存在" : "不存在")}，HasData 种子数据 {seedCount} 条（随模型建表一并写入）");
                Console.WriteLine("    对比：Migrate 会记录迁移历史且支持后续演进；EnsureCreated 建库后无法再改用迁移（历史表缺失，Migrate 会因表已存在而失败）");
            }
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);   // 临时库用完即删
            }
        }

        // ================= 4. CLI 指引 =================
        Console.WriteLine("【4】CLI 命令（读者可自行复现完整流程）：");
        Console.WriteLine("      dotnet ef migrations add InitialCreate      # 生成 v1 迁移");
        Console.WriteLine("      dotnet ef migrations add AddProductDescription  # 模型变更后生成 v2");
        Console.WriteLine("      dotnet ef database update                   # 应用全部迁移");
        Console.WriteLine("      dotnet ef database update InitialCreate     # 回退/升级到指定版本");
        Console.WriteLine("      迁移文件在项目 Migrations/ 目录，可打开查看每步的 Up/Down 操作");

        // ================= 5. 小结 =================
        Console.WriteLine();
        Console.WriteLine("小结：Migrate 按历史表增量升级、保留数据；EnsureCreated 只建表不建史，二者不可混用；迁移文件纳入版本控制可审查可回放。");
    }

    /// <summary>查询迁移历史表 __EFMigrationsHistory 的全部迁移 ID。</summary>
    private static async Task<List<string>> GetHistoryAsync(DemoContext db)
        => await db.Database
            .SqlQueryRaw<string>("SELECT \"MigrationId\" AS Value FROM \"__EFMigrationsHistory\" ORDER BY \"MigrationId\"")
            .ToListAsync();

    /// <summary>统计迁移历史条数。</summary>
    private static async Task<int> HistoryCountAsync(DemoContext db)
        => await db.Database
            .SqlQueryRaw<int>("SELECT COUNT(*) AS Value FROM \"__EFMigrationsHistory\"")
            .SingleAsync();
}
