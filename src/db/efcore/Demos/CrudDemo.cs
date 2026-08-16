using Microsoft.EntityFrameworkCore;

namespace EFCoreDemo.Demos;

/// <summary>
/// 演示 1：CRUD 基础 —— 新增、查询、更新、删除与 SaveChanges。
///
/// 知识点：
///   · Add / Update / Remove 只修改内存中的跟踪状态，SaveChanges 才真正写库
///   · 主键回填：插入后 EF Core 自动把数据库生成的主键写回实体属性
///   · 通过导航属性赋值，一次 SaveChanges 可写入多张关联表
///   · 删除时需注意外键依赖（EF Core 会按依赖顺序生成 DELETE）
/// </summary>
public static class CrudDemo
{
    public static void Run()
    {
        using var db = DbPaths.CreateContext();

        // ================= 1. 新增（Create） =================
        Console.WriteLine("【1】新增歌手");
        var artist = new Artist { Name = "演示乐队 Demo Band" };
        db.Artists.Add(artist);                        // 加入跟踪，状态 Added
        var added = db.SaveChanges();                  // 真正执行 INSERT
        Console.WriteLine($"    插入 {added} 行，生成的主键 ArtistId = {artist.ArtistId}（主键回填）");

        // 通过导航属性一次性写入 专辑 + 曲目 两张表
        Console.WriteLine("【2】新增关联数据（专辑 + 曲目）");
        var album = new Album { Title = "EF Core 演示专辑", ArtistId = artist.ArtistId };
        var track = new Track
        {
            Name = "Hello EF Core",
            Album = album,                             // 导航赋值，EF Core 自动补 AlbumId
            GenreId = 1,                               // Rock
            MediaTypeId = 1,                           // MPEG audio file
            Milliseconds = 180_000,
            UnitPrice = 0.99m,
        };
        db.Tracks.Add(track);
        db.SaveChanges();
        Console.WriteLine($"    新增专辑 AlbumId = {album.AlbumId}、曲目 TrackId = {track.TrackId}");

        // ================= 2. 查询（Read） =================
        Console.WriteLine("【3】查询");
        var found = db.Artists.Find(artist.ArtistId);  // Find 先查内存再查库
        Console.WriteLine($"    Find 查询到：{found?.Name}");
        var first = db.Artists.First(a => a.ArtistId == artist.ArtistId);
        Console.WriteLine($"    First 查询到：{first.Name}");

        // ================= 3. 更新（Update） =================
        Console.WriteLine("【4】更新");
        var toUpdate = db.Artists.Find(artist.ArtistId)!;
        toUpdate.Name = "演示乐队 Demo Band (v2)";     // 修改已跟踪实体的属性
        var updated = db.SaveChanges();                // 只 UPDATE 变化的列
        Console.WriteLine($"    更新 {updated} 行：{toUpdate.Name}");

        // ================= 4. 删除（Delete） =================
        Console.WriteLine("【5】删除（先子后父）");
        db.Remove(track);                              // 先删曲目
        db.Remove(album);                              // 再删专辑
        db.Remove(toUpdate);                           // 最后删歌手
        var deleted = db.SaveChanges();                // EF Core 自动按依赖顺序执行
        Console.WriteLine($"    删除 {deleted} 行");
        var gone = db.Artists.Find(artist.ArtistId);
        Console.WriteLine($"    删除后再次查询：{(gone is null ? "查无此人" : gone.Name)}");

        // ================= 5. 小结 =================
        Console.WriteLine();
        Console.WriteLine("小结：Add/Remove 修改的是跟踪状态，SaveChanges 才落库；插入后主键自动回填。");
    }
}
