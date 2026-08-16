using Microsoft.EntityFrameworkCore;

namespace EFCoreDemo;

/// <summary>
/// 数据库路径管理。
/// 所有演示只操作「工作副本」，源库 src/ChinookDemoDb.sqlite 永不被动。
/// 每次运行前都会强制重新复制一份副本，保证演示数据绝对干净、可无限重跑。
/// </summary>
public static class DbPaths
{
    private const string DbFileName = "ChinookDemoDb.sqlite";

    /// <summary>源库绝对路径（从运行目录向上遍历自动定位）。</summary>
    public static string SourceDbPath { get; } = FindSourceDb();

    /// <summary>工作副本路径（程序输出目录 bin/.../ 下）。</summary>
    public static string WorkingDbPath { get; } = Path.Combine(AppContext.BaseDirectory, DbFileName);

    /// <summary>每次运行前强制重新复制工作副本，丢弃上一次演示的写入。</summary>
    public static void EnsureWorkingCopy()
    {
        if (!File.Exists(SourceDbPath))
        {
            throw new FileNotFoundException($"找不到源数据库：{SourceDbPath}");
        }

        // 强制覆盖：无论工作副本是否存在都重新复制，保证每次演示从干净数据开始
        File.Copy(SourceDbPath, WorkingDbPath, overwrite: true);
    }

    /// <summary>创建指向工作副本的 DbContext。</summary>
    public static ChinookContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ChinookContext>()
            .UseSqlite($"Data Source={WorkingDbPath}")
            .Options;
        return new ChinookContext(options);
    }

    /// <summary>从当前目录与程序目录向上遍历，定位 src/ChinookDemoDb.sqlite。</summary>
    private static string FindSourceDb()
    {
        foreach (var start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
        {
            var dir = new DirectoryInfo(start);
            while (dir is not null)
            {
                var underSrc = Path.Combine(dir.FullName, "src", DbFileName);
                if (File.Exists(underSrc))
                {
                    return underSrc;
                }

                var direct = Path.Combine(dir.FullName, DbFileName);
                if (File.Exists(direct))
                {
                    return direct;
                }

                dir = dir.Parent;
            }
        }

        throw new FileNotFoundException(
            $"找不到源数据库 {DbFileName}，请确认在项目仓库目录中运行本程序。");
    }
}
