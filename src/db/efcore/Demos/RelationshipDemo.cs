using Microsoft.EntityFrameworkCore;

namespace EFCoreDemo.Demos;

/// <summary>
/// 演示 3：关系与加载 —— Include / ThenInclude / 过滤 Include / 显式加载 / 自引用。
///
/// 知识点：
///   · 导航属性默认不自动填充，需要用 Include 主动加载（显式加载）
///   · ThenInclude 用于加载「孙子级」关系
///   · Include 里可以带 Where 过滤集合内容（LEFT JOIN 语义，无匹配的歌手仍会返回）
///   · 多对多（Playlist ↔ Track）通过中间表 PlaylistTrack 表达
///
/// 说明：每个小节使用独立的 DbContext，避免「身份解析」导致前面步骤
/// 已加载的数据影响后面的演示结果。
/// </summary>
public static class RelationshipDemo
{
    public static void Run()
    {
        // ================= 1. Include：一对多 =================
        Console.WriteLine("【1】Include：歌手 → 专辑（一对多）");
        using (var db = DbPaths.CreateContext())
        {
            var artist = db.Artists
                .Include(a => a.Albums)
                .First(a => a.ArtistId == 1);              // AC/DC
            Console.WriteLine($"    歌手：{artist.Name}，专辑 {artist.Albums.Count} 张：");
            foreach (var album in artist.Albums)
            {
                Console.WriteLine($"      · {album.Title}");
            }
        }

        // ================= 2. ThenInclude：两级关系 =================
        Console.WriteLine("【2】ThenInclude：歌手 → 专辑 → 曲目（两级）");
        using (var db = DbPaths.CreateContext())
        {
            var artist = db.Artists
                .Include(a => a.Albums)
                .ThenInclude(al => al.Tracks)
                .First(a => a.ArtistId == 1);
            foreach (var album in artist.Albums)
            {
                Console.WriteLine($"    专辑《{album.Title}》共 {album.Tracks.Count} 首");
            }
        }

        // ================= 3. 多对多：播放列表 → 曲目 =================
        Console.WriteLine("【3】多对多：播放列表 → 曲目（经中间表 PlaylistTrack）");
        using (var db = DbPaths.CreateContext())
        {
            var playlist = db.Playlists
                .Include(p => p.PlaylistTracks)
                .ThenInclude(pt => pt.Track)
                .First(p => p.PlaylistId == 1);            // Music
            Console.WriteLine($"    播放列表：{playlist.Name}，共 {playlist.PlaylistTracks.Count} 首曲目");
            foreach (var pt in playlist.PlaylistTracks.Take(5))
            {
                Console.WriteLine($"      · {pt.Track?.Name}");
            }
            if (playlist.PlaylistTracks.Count > 5)
            {
                Console.WriteLine("      ……（仅显示前 5 条）");
            }
        }

        // ================= 4. 过滤 Include =================
        Console.WriteLine("【4】过滤 Include：只加载标题含 Greatest 的专辑");
        using (var db = DbPaths.CreateContext())
        {
            var artists = db.Artists
                .Include(a => a.Albums.Where(al => al.Title.Contains("Greatest")))
                .ToList();
            var withHits = artists.Where(a => a.Albums.Count > 0).ToList();
            Console.WriteLine($"    共返回 {artists.Count} 位歌手（LEFT JOIN 语义：无匹配的歌手也会返回）");
            Console.WriteLine($"    其中有 {withHits.Count} 位歌手加载到了匹配专辑，示例：");
            foreach (var a in withHits.Take(2))
            {
                var titles = string.Join(" / ", a.Albums.Select(al => al.Title));
                Console.WriteLine($"      · {a.Name}：{titles}");
            }

            Console.WriteLine("    未匹配的歌手集合为空，示例：");
            foreach (var a in artists.Where(a => a.Albums.Count == 0).Take(1))
            {
                Console.WriteLine($"      · {a.Name}：{a.Albums.Count} 张");
            }
        }

        // ================= 5. 显式加载（Explicit Loading） =================
        Console.WriteLine("【5】显式加载：先查歌手，再按需加载专辑");
        using (var db = DbPaths.CreateContext())
        {
            var artist = db.Artists.First(a => a.ArtistId == 2); // Accept
            Console.WriteLine($"    查询歌手后专辑数：{artist.Albums.Count}");
            db.Entry(artist).Collection(a => a.Albums).Load();    // 手动触发加载
            Console.WriteLine($"    Load() 后专辑数：{artist.Albums.Count}");
        }

        // ================= 6. 自引用：员工 → 上级 =================
        Console.WriteLine("【6】自引用：员工 → 上级（ReportsTo）");
        using (var db = DbPaths.CreateContext())
        {
            var employee = db.Employees
                .Include(e => e.ReportsToManager)
                .First(e => e.EmployeeId == 2);            // 员工 2 的上级是 1
            Console.WriteLine($"    {employee.FirstName} {employee.LastName} 的上级：{employee.ReportsToManager?.FirstName} {employee.ReportsToManager?.LastName}");
            var boss = db.Employees.First(e => e.EmployeeId == 1);
            var bossName = boss.ReportsToManager is null ? "无（最高层）" : boss.ReportsToManager.FirstName;
            Console.WriteLine($"    老板 {boss.FirstName} {boss.LastName} 的上级：{bossName}");
        }

        // ================= 7. 小结 =================
        Console.WriteLine();
        Console.WriteLine("小结：Include/ThenInclude 生成 JOIN 一次查完；显式加载适合按需取数；多对多靠中间表实体表达。");
    }
}
