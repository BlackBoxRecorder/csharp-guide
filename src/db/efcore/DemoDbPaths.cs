using Microsoft.EntityFrameworkCore;

namespace EFCoreDemo;

/// <summary>
/// 演示模型库（DemoDemoDb.sqlite）路径管理。
/// 与 Chinook 的「复制工作副本」机制不同：演示库由迁移创建，
/// 因此每次启动强制删除旧文件，随后由各演示的 Migrate/EnsureCreated 重建。
/// </summary>
public static class DemoDbPaths
{
    private const string DbFileName = "DemoDemoDb.sqlite";

    /// <summary>演示库路径（程序输出目录 bin/.../ 下）。</summary>
    public static string DemoDbPath { get; } = Path.Combine(AppContext.BaseDirectory, DbFileName);

    /// <summary>每次运行前强制删除旧演示库，保证从空库开始重建。</summary>
    public static void EnsureClean()
    {
        if (File.Exists(DemoDbPath))
        {
            File.Delete(DemoDbPath);
        }
    }

    /// <summary>创建默认的 DemoContext（不开启延迟加载，不输出日志）。</summary>
    public static DemoContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<DemoContext>()
            .UseSqlite($"Data Source={DemoDbPath}")
            .Options;
        return new DemoContext(options);
    }

    /// <summary>创建开启延迟加载代理的 DemoContext（供 Demo 9 延迟加载小节使用）。</summary>
    public static DemoContext CreateLazyLoadingContext()
    {
        var options = new DbContextOptionsBuilder<DemoContext>()
            .UseSqlite($"Data Source={DemoDbPath}")
            .UseLazyLoadingProxies()
            .Options;
        return new DemoContext(options);
    }
}
