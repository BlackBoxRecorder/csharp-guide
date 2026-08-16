using Microsoft.EntityFrameworkCore;

namespace EFCoreDemo.Demos;

/// <summary>
/// 演示 5：原始 SQL 与批量操作 —— FromSqlRaw / FromSqlInterpolated / ExecuteUpdate / ExecuteDelete。
///
/// 知识点：
///   · FromSqlRaw/FromSqlInterpolated 把实体查询改为自定义 SQL，但要求返回完整实体列
///   · 参数必须用 {0} 占位符或插值写法，EF Core 会自动参数化，杜绝 SQL 注入
///   · EF Core 8 起：非组合的 FromSqlRaw 必须先 ToList 物化（本演示用的是可组合的 SELECT *）
///   · ExecuteUpdate / ExecuteDelete 直接生成 UPDATE/DELETE 语句，不加载实体，性能极高
/// </summary>
public static class RawSqlDemo
{
    public static void Run()
    {
        using var db = DbPaths.CreateContext();

        // ================= 1. FromSqlRaw：原生 SQL 查询（参数化） =================
        Console.WriteLine("【1】FromSqlRaw：单价超过 0.99 的曲目数量");
        var price = 0.99m;                             // 用占位符 {0} 传参，EF Core 自动参数化
        var expensive = db.Tracks
            .FromSqlRaw("SELECT * FROM Track WHERE UnitPrice > {0}", price)
            .ToList();
        Console.WriteLine($"    共 {expensive.Count} 首");

        // ================= 2. FromSqlInterpolated：插值写法（同样参数化） =================
        Console.WriteLine("【2】FromSqlInterpolated：曲名以 Rock 开头的曲目（前 5 首）");
        var pattern = "Rock%";
        var rockTracks = db.Tracks
            .FromSqlInterpolated($"SELECT * FROM Track WHERE Name LIKE {pattern}")
            .Take(5)
            .ToList();
        foreach (var t in rockTracks)
        {
            Console.WriteLine($"    {t.Name}");
        }

        // ================= 3. ExecuteUpdate：批量更新（不加载实体） =================
        Console.WriteLine("【3】ExecuteUpdate：Rock 流派（GenreId=1）曲目单价打 9 折");
        var before = db.Tracks.Where(t => t.GenreId == 1).Select(t => t.UnitPrice).First();
        var updated = db.Tracks
            .Where(t => t.GenreId == 1)
            .ExecuteUpdate(setters => setters.SetProperty(t => t.UnitPrice, t => t.UnitPrice * 0.9m));
        var after = db.Tracks.Where(t => t.GenreId == 1).Select(t => t.UnitPrice).First();
        Console.WriteLine($"    更新 {updated} 行：示例单价 {before:F2} → {after:F2}");

        // ================= 4. ExecuteDelete：批量删除（不加载实体） =================
        Console.WriteLine("【4】ExecuteDelete：删除刚才插入的测试数据");
        for (var i = 0; i < 3; i++)
        {
            db.Tracks.Add(new Track { Name = $"BulkDeleteTest-{i}", MediaTypeId = 1, UnitPrice = 0.99m });
        }

        db.SaveChanges();
        Console.WriteLine($"    已插入 3 条 BulkDeleteTest 测试曲目");

        var deleted = db.Tracks
            .Where(t => t.Name.StartsWith("BulkDeleteTest"))
            .ExecuteDelete();
        var remain = db.Tracks.Count(t => t.Name.StartsWith("BulkDeleteTest"));
        Console.WriteLine($"    ExecuteDelete 删除 {deleted} 行，剩余 {remain} 行");

        // ================= 5. 小结 =================
        Console.WriteLine();
        Console.WriteLine("小结：FromSql 用于复杂 SQL 场景且必须参数化；ExecuteUpdate/ExecuteDelete 不经过实体跟踪，批量操作首选。");
    }
}
