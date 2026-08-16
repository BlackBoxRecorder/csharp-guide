using Microsoft.EntityFrameworkCore;

namespace EFCoreDemo.Demos;

/// <summary>
/// 演示 7：联表查询 —— LINQ Join / GroupJoin / 多表连续连接。
///
/// 知识点：
///   · Join 等值内连接：只返回两侧都匹配的行（INNER JOIN）
///   · GroupJoin + SelectMany：左侧全部保留，无匹配侧为 null（LEFT JOIN 语义）
///   · 多表连续 Join：链式连接多张表
///   · Join + 分组聚合：联表后 GroupBy + Sum
///   · 导航属性 vs 手动 Join：同一结果两种写法
///
/// 说明：每个小节使用独立的 DbContext，遵循「独立上下文避免状态干扰」的规范。
/// </summary>
public static class JoinDemo
{
    public static void Run()
    {
        // ================= 1. Join 等值内连接 =================
        Console.WriteLine("【1】Join 内连接：曲目 → 专辑（INNER JOIN，只保留两侧都匹配的行）");
        using (var db = DbPaths.CreateContext())
        {
            var query = db
                .Tracks.Join(
                    db.Albums,
                    t => t.AlbumId,
                    a => (int?)a.AlbumId,
                    (t, a) => new { t.Name, Album = a.Title }
                )
                .OrderBy(x => x.Name)
                .Take(5);

            Console.WriteLine("    生成的 SQL：");
            foreach (var line in query.ToQueryString().Split('\n'))
            {
                Console.WriteLine($"      {line}");
            }

            Console.WriteLine("    结果（曲目名 → 专辑，按曲名排序前 5 条）：");
            foreach (var x in query.ToList())
            {
                Console.WriteLine($"      {x.Name.PadRight(40)} {x.Album}");
            }
        }

        // ================= 2. GroupJoin 左连接 =================
        Console.WriteLine("【2】GroupJoin 左连接：歌手 → 专辑（LEFT JOIN，无专辑的歌手也保留）");
        using (var db = DbPaths.CreateContext())
        {
            // 统计：每位歌手的专辑数（GroupJoin 翻译为相关子查询）
            var stats = db
                .Artists.GroupJoin(
                    db.Albums,
                    a => a.ArtistId,
                    al => al.ArtistId,
                    (a, albums) => new { a.Name, AlbumCount = albums.Count() }
                )
                .ToList();
            var withAlbums = stats.Count(s => s.AlbumCount > 0);
            Console.WriteLine(
                $"    共 {stats.Count} 位歌手：有专辑 {withAlbums} 位，无专辑 {stats.Count - withAlbums} 位（左连接不丢弃左侧）"
            );
            Console.WriteLine("    无专辑歌手示例：");
            foreach (var s in stats.Where(s => s.AlbumCount == 0).Take(3))
            {
                Console.WriteLine($"      · {s.Name}");
            }

            // 展平：SelectMany + DefaultIfEmpty 把「每组」还原为逐行，翻译为 LEFT JOIN
            var flatQuery = db
                .Artists.GroupJoin(
                    db.Albums,
                    a => a.ArtistId,
                    al => al.ArtistId,
                    (a, albums) => new { a, albums }
                )
                .SelectMany(
                    x => x.albums.DefaultIfEmpty(),
                    (x, al) => new { Artist = x.a.Name, Album = al == null ? null : al.Title }
                );

            Console.WriteLine("    生成的 SQL（SelectMany 展平）：");
            foreach (var line in flatQuery.ToQueryString().Split('\n'))
            {
                Console.WriteLine($"      {line}");
            }

            Console.WriteLine("    SelectMany 展平：无专辑的歌手，专辑列为 null");
            foreach (var r in flatQuery.ToList().Where(r => r.Album is null).Take(3))
            {
                Console.WriteLine(
                    $"      · {(r.Artist ?? "未知").PadRight(30)} 专辑：{r.Album ?? "（无专辑）"}"
                );
            }
        }

        // ================= 3. 多表连续 Join =================
        Console.WriteLine("【3】多表连续 Join：订单明细 → 曲目 → 专辑 → 歌手（4 表链式连接）");
        using (var db = DbPaths.CreateContext())
        {
            var query = db
                .InvoiceLines.Join(
                    db.Tracks,
                    il => il.TrackId,
                    t => t.TrackId,
                    (il, t) => new { il, t }
                )
                .Join(
                    db.Albums,
                    x => x.t.AlbumId,
                    a => (int?)a.AlbumId,
                    (x, a) =>
                        new
                        {
                            x.il,
                            x.t,
                            a,
                        }
                )
                .Join(
                    db.Artists,
                    x => x.a.ArtistId,
                    ar => ar.ArtistId,
                    (x, ar) =>
                        new
                        {
                            x.il,
                            x.t,
                            x.a,
                            ar,
                        }
                )
                .Select(x => new
                {
                    x.t.Name,
                    Album = x.a.Title,
                    Artist = x.ar.Name,
                })
                .Take(3);

            Console.WriteLine("    生成的 SQL：");
            foreach (var line in query.ToQueryString().Split('\n'))
            {
                Console.WriteLine($"      {line}");
            }

            Console.WriteLine("    结果（曲目名 → 专辑 → 歌手，前 3 条）：");
            foreach (var x in query.ToList())
            {
                Console.WriteLine($"      {x.Name.PadRight(40)} {x.Album.PadRight(30)} {x.Artist}");
            }
        }

        // ================= 4. Join + 分组聚合 =================
        Console.WriteLine("【4】Join + 分组聚合：客户 → 订单，统计每位客户总消费 Top 5");
        using (var db = DbPaths.CreateContext())
        {
            var top = db
                .Customers.Join(
                    db.Invoices,
                    c => c.CustomerId,
                    i => i.CustomerId,
                    (c, i) => new { c, i }
                )
                .GroupBy(x => new
                {
                    x.c.CustomerId,
                    x.c.FirstName,
                    x.c.LastName,
                })
                .Select(g => new
                {
                    g.Key.FirstName,
                    g.Key.LastName,
                    Total = g.Sum(x => x.i.Total),
                })
                .OrderByDescending(x => x.Total)
                .Take(5)
                .ToList();

            foreach (var x in top)
            {
                Console.WriteLine(
                    $"      {($"{x.LastName} {x.FirstName}").PadRight(25)} {x.Total, 10:F2}"
                );
            }
        }

        // ================= 5. 导航属性 vs 手动 Join =================
        Console.WriteLine("【5】导航属性 vs 手动 Join：同一查询（歌手 → 专辑数）两种写法");
        using (var db = DbPaths.CreateContext())
        {
            // 写法 A：导航属性（EF Core 自动生成 JOIN）
            var byNav = db
                .Artists.Select(a => new { a.Name, AlbumCount = a.Albums.Count })
                .ToList();

            // 写法 B：手动 GroupJoin
            var byJoin = db
                .Artists.GroupJoin(
                    db.Albums,
                    a => a.ArtistId,
                    al => al.ArtistId,
                    (a, albums) => new { a.Name, AlbumCount = albums.Count() }
                )
                .ToList();

            Console.WriteLine(
                $"    导航属性写法返回 {byNav.Count} 位歌手，手动 Join 写法返回 {byJoin.Count} 位歌手 —— 结果一致"
            );
            var acdcNav = byNav.First(a => a.Name == "AC/DC");
            var acdcJoin = byJoin.First(a => a.Name == "AC/DC");
            Console.WriteLine(
                $"    对比示例 AC/DC：导航属性 {acdcNav.AlbumCount} 张，Join {acdcJoin.AlbumCount} 张"
            );
            Console.WriteLine(
                "    结论：有导航属性时优先用导航属性写法（简洁、可读）；手动 Join 适用于没有导航关系、"
            );
            Console.WriteLine("          需要跨 DbContext 或精确控制连接条件的场景。");
        }

        // ================= 6. 小结 =================
        Console.WriteLine();
        Console.WriteLine(
            "小结：Join 生成 INNER JOIN，GroupJoin + SelectMany 生成 LEFT JOIN；多表连接靠链式 Join；"
        );
        Console.WriteLine("      业务上优先用导航属性，手动 Join 作为兜底手段。");
    }
}
