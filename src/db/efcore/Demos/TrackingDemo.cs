using Microsoft.EntityFrameworkCore;

namespace EFCoreDemo.Demos;

/// <summary>
/// 演示 4：跟踪与不跟踪 —— EntityState 状态机与 AsNoTracking。
///
/// 知识点：
///   · 默认查询结果会被 DbContext 跟踪，修改属性后 SaveChanges 即可生效
///   · 状态流转：Detached → Added → Unchanged → Modified → Deleted
///   · AsNoTracking 查询不跟踪实体，修改无效（适合只读场景，性能更好）
///   · Update() 可以把「断开连接」的实体附加回上下文，整行标记为修改
/// </summary>
public static class TrackingDemo
{
    public static void Run()
    {
        using var db = DbPaths.CreateContext();

        // ================= 1. 默认跟踪：查询后状态 Unchanged =================
        Console.WriteLine("【1】默认跟踪：查询后实体状态");
        var artist = db.Artists.First(a => a.ArtistId == 1);
        Console.WriteLine($"    查询后：{db.Entry(artist).State}");        // Unchanged

        // ================= 2. 修改属性：状态变 Modified，SaveChanges 生效 =================
        Console.WriteLine("【2】修改已跟踪实体");
        artist.Name = $"{artist.Name}（本演示修改）";
        Console.WriteLine($"    修改属性后：{db.Entry(artist).State}");    // Modified
        db.SaveChanges();
        Console.WriteLine($"    SaveChanges 后：{db.Entry(artist).State}"); // Unchanged

        // ================= 3. AsNoTracking：不跟踪，修改无效 =================
        Console.WriteLine("【3】AsNoTracking：不跟踪的实体");
        var noTrack = db.Artists.AsNoTracking().First(a => a.ArtistId == 2);
        Console.WriteLine($"    查询后：{db.Entry(noTrack).State}");       // Detached
        noTrack.Name = "这个修改不会生效";
        db.SaveChanges();                                                  // 不会报错，但也不会写库
        var verify = db.Artists.AsNoTracking().First(a => a.ArtistId == 2);
        Console.WriteLine($"    未跟踪修改是否生效：{(verify.Name == "这个修改不会生效" ? "生效了" : "未生效")}");

        // ================= 4. Update：附加断开实体 =================
        Console.WriteLine("【4】Update：附加「断开连接」的实体");
        var detached = new Artist { ArtistId = 3, Name = "Aerosmith（外部修改）" };
        db.Artists.Update(detached);                                       // 直接标记 Modified
        Console.WriteLine($"    Update 附加后：{db.Entry(detached).State}"); // Modified
        db.SaveChanges();

        // ================= 5. Remove：状态 Deleted =================
        Console.WriteLine("【5】Remove：删除流程");
        var temp = new Artist { Name = "临时删除歌手" };
        db.Artists.Add(temp);
        db.SaveChanges();
        Console.WriteLine($"    已插入临时歌手（ArtistId={temp.ArtistId}），用于安全删除演示");
        var toDelete = db.Artists.Find(temp.ArtistId);
        db.Artists.Remove(toDelete!);
        Console.WriteLine($"    Remove 后：{db.Entry(toDelete!).State}");  // Deleted
        db.SaveChanges();
        var gone = db.Artists.Find(temp.ArtistId);
        Console.WriteLine($"    删除后查询：{(gone is null ? "查无此人" : gone.Name)}");

        // ================= 6. 小结 =================
        Console.WriteLine();
        Console.WriteLine("小结：跟踪由上下文管理（Unchanged/Modified/Deleted/Added），AsNoTracking 适合纯读场景；Update 适合脱管实体的整行更新。");
    }
}
