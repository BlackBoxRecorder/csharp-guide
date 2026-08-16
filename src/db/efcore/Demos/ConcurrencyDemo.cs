using Microsoft.EntityFrameworkCore;

namespace EFCoreDemo.Demos;

/// <summary>
/// 演示 11：乐观并发 —— 并发令牌（Version 版本号）与 DbUpdateConcurrencyException。
///
/// 知识点：
///   · 并发令牌：UPDATE 语句携带 WHERE Version = 原值，行被他人更新则影响行数为 0，抛出异常
///   · 两个上下文同时修改同一行：先保存者成功，后保存者抛 DbUpdateConcurrencyException
///   · 冲突处理三策略：以库为准（Reload）/ 强制覆盖（重置 OriginalValues 后保存）/ 属性级合并
///   · SQLite 无原生 rowversion，用 [ConcurrencyCheck] 版本号代替（SQL Server 用 [Timestamp] 自动维护）
///
/// 说明：本 Demo 运行前演示库需已由迁移创建（MigrateAsync 幂等保证）。
/// </summary>
public static class ConcurrencyDemo
{
    public static async Task Run()
    {
        // ================= 0. 准备：建库 + 插入一个商品 =================
        int productId;
        using (var db = DemoDbPaths.CreateContext())
        {
            await db.Database.MigrateAsync();
            var product = new Product { Name = "并发测试商品", Price = 10.00m, CategoryId = 1 };
            await db.Products.AddAsync(product);
            await db.SaveChangesAsync();
            productId = product.Id;
            Console.WriteLine($"【0】已插入商品（Id={productId}），Version=0（并发令牌初始值）");
        }

        // ================= 1. 冲突复现：两个上下文同时修改 =================
        Console.WriteLine("【1】并发冲突复现：两个上下文同时修改同一商品");
        using (var db1 = DemoDbPaths.CreateContext())
        using (var db2 = DemoDbPaths.CreateContext())
        {
            var p1 = await db1.Products.FirstAsync(p => p.Id == productId);
            var p2 = await db2.Products.FirstAsync(p => p.Id == productId);

            p1.Name = "改名（先保存）";
            p1.Version++;                                 // 每次修改前递增版本号
            await db1.SaveChangesAsync();                 // WHERE Version=0 匹配 → 成功，库中 Version=1
            Console.WriteLine("    上下文 1 修改名称并保存成功（Version 0 → 1）");

            p2.Price = 99.99m;
            p2.Version++;                                 // 本地以为还是 Version=0，递增到 1
            try
            {
                await db2.SaveChangesAsync();             // WHERE Version=0 不匹配（库里已是 1）→ 冲突
                Console.WriteLine("    （异常！）后保存者居然成功了");
            }
            catch (DbUpdateConcurrencyException)
            {
                Console.WriteLine("    上下文 2 修改价格后保存：抛出 DbUpdateConcurrencyException（乐观并发检测到冲突）");
            }
            Console.WriteLine("    原因：UPDATE 语句携带 WHERE Version = 0（查询时的版本），行已被别人更新则影响行数为 0");
        }

        // ================= 2. 冲突详情：对比双方值 =================
        Console.WriteLine("【2】冲突详情：数据库当前值 vs 本地修改值");
        using (var db1 = DemoDbPaths.CreateContext())
        using (var db2 = DemoDbPaths.CreateContext())
        {
            // 两个上下文先各自读到同一版本（Version=1），再让 db1 保存推进版本 → 制造真实冲突
            var p1 = await db1.Products.FirstAsync(p => p.Id == productId);
            var p2 = await db2.Products.FirstAsync(p => p.Id == productId);   // 读到 Version=1（旧值）

            p1.Name = "改名（第二次）";
            p1.Version++;
            await db1.SaveChangesAsync();                                     // 库里版本推进到 2

            p2.Price = 99.99m;
            p2.Version++;
            try
            {
                await db2.SaveChangesAsync();                                  // WHERE Version=1 不匹配（库里已是 2）→ 冲突
                Console.WriteLine("    （异常！）冲突未触发");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                var entry = ex.Entries.Single();
                var databaseValues = await entry.GetDatabaseValuesAsync();
                Console.WriteLine("    属性        数据库当前值        本地修改值");
                foreach (var prop in entry.CurrentValues.Properties)
                {
                    var dbVal = databaseValues?[prop] ?? "(无)";
                    var curVal = entry.CurrentValues[prop] ?? "(无)";
                    if (!Equals(dbVal, curVal))
                    {
                        Console.WriteLine($"    {prop.Name,-12}  {dbVal,-18}  {curVal}");
                    }
                }
            }
        }

        // ================= 3. 策略一：以库为准（Reload 放弃本地修改） =================
        Console.WriteLine("【3】策略一：以库为准 —— Reload() 放弃本地修改");
        using (var db1 = DemoDbPaths.CreateContext())
        using (var db2 = DemoDbPaths.CreateContext())
        {
            var p1 = await db1.Products.FirstAsync(p => p.Id == productId);
            var p2 = await db2.Products.FirstAsync(p => p.Id == productId);
            p1.Name = "第一轮改名";
            p1.Version++;
            await db1.SaveChangesAsync();
            p2.Price = 1.00m;
            p2.Version++;
            try
            {
                await db2.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                var entry = ex.Entries.Single();
                await entry.ReloadAsync();                // 丢弃本地修改，加载数据库最新值
            }
            var after = await db2.Products.AsNoTracking().FirstAsync(p => p.Id == productId);
            Console.WriteLine($"    冲突后 Reload：名称={after.Name}，价格={after.Price:F2}（以数据库为准，本地价格修改被丢弃）");
        }

        // ================= 4. 策略二：强制覆盖（重置 OriginalValues 后保存） =================
        Console.WriteLine("【4】策略二：强制覆盖 —— 同步版本号后以本地修改为准");
        using (var db1 = DemoDbPaths.CreateContext())
        using (var db2 = DemoDbPaths.CreateContext())
        {
            var p1 = await db1.Products.FirstAsync(p => p.Id == productId);
            var p2 = await db2.Products.FirstAsync(p => p.Id == productId);
            p1.Name = "第二轮改名";
            p1.Version++;
            await db1.SaveChangesAsync();
            p2.Price = 2.00m;
            p2.Version++;
            try
            {
                await db2.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                var entry = ex.Entries.Single();
                var databaseValues = await entry.GetDatabaseValuesAsync();
                entry.OriginalValues.SetValues(databaseValues!);        // 同步 Version 等原始值，让 WHERE 能匹配
                var dbVersion = databaseValues!.GetValue<int>(nameof(Product.Version));
                entry.CurrentValues[nameof(Product.Version)] = dbVersion + 1;             // 覆盖写入产生新版本
                await db2.SaveChangesAsync();                           // 本地修改（价格 2.00）覆盖写入
            }
            var after = await db2.Products.AsNoTracking().FirstAsync(p => p.Id == productId);
            Console.WriteLine($"    冲突后覆盖：名称={after.Name}，价格={after.Price:F2}（以本地修改为准，覆盖了别人的名称修改）");
        }

        // ================= 5. 小结 =================
        Console.WriteLine();
        Console.WriteLine("小结：乐观并发靠并发令牌（版本号/rowversion）实现无锁检测；冲突处理按业务取舍——Reload 以库为准、SetValues 以本地为准、逐属性合并最精细。");
    }
}
