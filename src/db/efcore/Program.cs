using EFCoreDemo;
using EFCoreDemo.Demos;

// ============================================================
// EF Core 演示项目入口
//
// 用法：
//   dotnet run                     —— 显示菜单，交互选择演示
//   dotnet run -- 3                —— 按编号直接运行（如 3 = 关系与加载）
//   dotnet run -- relationship     —— 按名称直接运行
//
// 演示分两组：
//   1-7   Chinook 库演示（真实音乐库，操作 bin 目录下的工作副本）
//   8-11  演示模型库演示（教学主题，每次启动删库、由迁移重建）
// ============================================================

DbPaths.EnsureWorkingCopy();
DemoDbPaths.EnsureClean();
Console.OutputEncoding = System.Text.Encoding.UTF8;   // 统一控制台输出编码，避免中文乱码
Console.WriteLine($"工作副本数据库：{DbPaths.WorkingDbPath}（已从源库重新复制）");
Console.WriteLine($"演示模型库：{DemoDbPaths.DemoDbPath}（已删除，由迁移重建）");
Console.WriteLine();

var demos = new (int Id, string Key, string Title, Func<Task> Run)[]
{
    (1, "crud",         "CRUD 基础：增删改查与 SaveChanges",            () => { CrudDemo.Run(); return Task.CompletedTask; }),
    (2, "query",        "LINQ 查询：过滤/排序/投影/聚合/分页",            () => { QueryDemo.Run(); return Task.CompletedTask; }),
    (3, "relationship", "关系与加载：Include/ThenInclude/显式加载",       () => { RelationshipDemo.Run(); return Task.CompletedTask; }),
    (4, "tracking",     "跟踪与不跟踪：EntityState 与 AsNoTracking",      () => { TrackingDemo.Run(); return Task.CompletedTask; }),
    (5, "rawsql",       "原始 SQL 与批量操作：FromSql/ExecuteUpdate",     () => { RawSqlDemo.Run(); return Task.CompletedTask; }),
    (6, "transaction",  "事务：自动事务/手动事务/回滚",                   () => { TransactionDemo.Run(); return Task.CompletedTask; }),
    (7, "join",         "联表查询：LINQ Join/GroupJoin/多表连接",         () => { JoinDemo.Run(); return Task.CompletedTask; }),
    (8, "config",       "模型配置：数据注解/配置类/种子/全局过滤/日志",    ConfigurationDemo.Run),
    (9, "advrel",       "进阶关系：一对一/原生多对多/延迟加载",            RelationshipAdvancedDemo.Run),
    (10, "migration",   "迁移：分步升级/历史表/EnsureCreated 对比",       MigrationDemo.Run),
    (11, "concurrency", "乐观并发：Version 版本号与冲突处理",               ConcurrencyDemo.Run),
};

if (args.Length > 0)
{
    await RunDemo(demos, args[0]);
    return;
}

// —— 无参数：交互菜单 ——
while (true)
{
    Console.WriteLine("========== EF Core 演示菜单 ==========");
    Console.WriteLine("  —— Chinook 库演示 ——");
    foreach (var (id, key, title, _) in demos.Where(d => d.Id <= 7))
    {
        Console.WriteLine($"  {id}. {title}（{key}）");
    }

    Console.WriteLine("  —— 演示模型库演示 ——");
    foreach (var (id, key, title, _) in demos.Where(d => d.Id > 7))
    {
        Console.WriteLine($"  {id}. {title}（{key}）");
    }

    Console.WriteLine("  q. 退出");
    Console.Write("请选择：");
    var input = Console.ReadLine()?.Trim();
    if (string.Equals(input, "q", StringComparison.OrdinalIgnoreCase) || input is null)
    {
        Console.WriteLine("再见！");
        return;
    }

    await RunDemo(demos, input);
    Console.WriteLine();
}

/// <summary>按编号或名称解析并运行一个演示，捕获异常避免中断整个程序。</summary>
static async Task RunDemo((int Id, string Key, string Title, Func<Task> Run)[] demos, string input)
{
    var demo = demos.FirstOrDefault(d =>
        d.Id.ToString() == input ||
        string.Equals(d.Key, input, StringComparison.OrdinalIgnoreCase));

    if (demo == default)
    {
        Console.WriteLine($"未找到演示：{input}（可用编号或名称，如 3 或 relationship）");
        return;
    }

    Console.WriteLine($"====== 演示 {demo.Id}：{demo.Title} ======");
    try
    {
        await demo.Run();
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"演示执行失败：{ex.Message}");
        if (ex.InnerException is not null)
        {
            Console.WriteLine($"  内部异常：{ex.InnerException.Message}");
        }

        Console.ResetColor();
    }

    Console.WriteLine("====== 演示结束 ======");
}
