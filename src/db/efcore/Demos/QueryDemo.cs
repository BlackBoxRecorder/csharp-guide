using Microsoft.EntityFrameworkCore;

namespace EFCoreDemo.Demos;

/// <summary>
/// 演示 2：LINQ 查询 —— 过滤、排序、投影、聚合、分页、分组。
///
/// 知识点：
///   · LINQ 查询会被 EF Core 翻译为 SQL，IQueryable 是延迟执行的
///   · ToList / First / Count 等终结操作才真正执行 SQL
///   · 投影（Select 匿名对象）可以只取需要的列，避免整行加载
/// </summary>
public static class QueryDemo
{
    public static void Run()
    {
        using var db = DbPaths.CreateContext();

        // ================= 1. 过滤（Where） =================
        Console.WriteLine("【1】过滤：时长超过 400 秒的曲目");
        var longTracks = db.Tracks.Count(t => t.Milliseconds > 400_000);
        Console.WriteLine($"    共 {longTracks} 首（SQL：WHERE Milliseconds > 400000）");

        // ================= 2. 排序（OrderBy/ThenBy） =================
        Console.WriteLine("【2】排序：按时长降序，前 5 首");
        var top5 = db.Tracks
            .OrderByDescending(t => t.Milliseconds)
            .ThenBy(t => t.Name)
            .Take(5)
            .ToList();
        foreach (var t in top5)
        {
            var minutes = t.Milliseconds / 60_000m;
            Console.WriteLine($"    {t.Name.PadRight(40)} {minutes,6:F1} 分钟");
        }

        // ================= 3. 投影（Select） =================
        Console.WriteLine("【3】投影：只取曲名与时长（匿名类型，只查两列）");
        var names = db.Tracks
            .Where(t => t.GenreId == 1)
            .OrderBy(t => t.Name)
            .Select(t => new { t.Name, Minutes = t.Milliseconds / 60_000m })
            .Take(3)
            .ToList();
        foreach (var n in names)
        {
            Console.WriteLine($"    {n.Name.PadRight(40)} {n.Minutes,6:F1} 分钟");
        }

        // ================= 4. 聚合（Count/Max/Avg） =================
        Console.WriteLine("【4】聚合：订单金额统计");
        var maxTotal = db.Invoices.Max(i => i.Total);
        var avgTotal = db.Invoices.Average(i => i.Total);
        var invoiceCount = db.Invoices.Count();
        Console.WriteLine($"    订单数 {invoiceCount}，最大金额 {maxTotal:F2}，平均金额 {avgTotal:F2}");

        // ================= 5. 分页（Skip/Take） =================
        Console.WriteLine("【5】分页：专辑按标题排序，第 3 页（每页 10 条）");
        var page = db.Albums
            .OrderBy(a => a.Title)
            .Skip(20)
            .Take(10)
            .ToList();
        foreach (var a in page)
        {
            Console.WriteLine($"    {a.AlbumId,4}  {a.Title.PadRight(40)} 歌手 {a.ArtistId}");
        }

        // ================= 6. 存在性（Any/Contains） =================
        Console.WriteLine("【6】存在性判断");
        var hasAcDc = db.Artists.Any(a => a.Name == "AC/DC");
        var names2 = new[] { "Metallica", "Led Zeppelin" };
        var contains = db.Artists.Count(a => names2.Contains(a.Name));
        Console.WriteLine($"    存在 AC/DC：{hasAcDc}；Metallica / Led Zeppelin 出现 {contains} 次");

        // ================= 7. 分组（GroupBy） =================
        Console.WriteLine("【7】分组：各流派曲目数量 Top 5");
        var groups = db.Tracks
            .GroupBy(t => t.GenreId)
            .Select(g => new { GenreId = g.Key, Count = g.Count() })
            .OrderByDescending(g => g.Count)
            .Take(5)
            .ToList();
        foreach (var g in groups)
        {
            var genreName = db.Genres.Find(g.GenreId)?.Name ?? "未知";
            Console.WriteLine($"    {genreName.PadRight(20)} {g.Count,5} 首");
        }

        // ================= 8. 小结 =================
        Console.WriteLine();
        Console.WriteLine("小结：LINQ 查询延迟执行，链式组合会被翻译为一条 SQL；分页/聚合/分组是日常高频操作。");
    }
}
