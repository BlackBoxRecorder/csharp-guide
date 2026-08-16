using Microsoft.EntityFrameworkCore;

namespace EFCoreDemo.Demos;

/// <summary>
/// 演示 6：事务 —— 自动事务 / 手动事务提交 / 手动回滚 / 异常自动回滚。
///
/// 知识点：
///   · 一次 SaveChanges 天然就是一个事务，要么全成功要么全失败
///   · BeginTransaction 开启手动事务，可包含多次 SaveChanges
///   · 事务未 Commit 就释放（Dispose）时自动回滚
///   · 回滚不会清除跟踪图中的实体，因此每个场景使用独立的 DbContext 演示
/// </summary>
public static class TransactionDemo
{
    public static void Run()
    {
        // ================= 1. 自动事务：一次 SaveChanges 写多张表 =================
        Console.WriteLine("【1】自动事务：客户 + 订单 + 订单明细一次写入");
        using (var db = DbPaths.CreateContext())
        {
            var customer = new Customer
            {
                FirstName = "演示", LastName = "客户",
                Email = "demo@example.com", Country = "China",
            };
            var invoice = new Invoice
            {
                Customer = customer,
                InvoiceDate = DateTime.Now,
                Total = 1.98m,
            };
            invoice.InvoiceLines.Add(new InvoiceLine
            {
                TrackId = 1,                 // 引用已有曲目
                UnitPrice = 0.99m,
                Quantity = 2,
            });
            db.Invoices.Add(invoice);
            var saved = db.SaveChanges();    // 隐式事务：3 张表 3 行，全部成功
            Console.WriteLine($"    一次 SaveChanges 写入 {saved} 行（CustomerId={customer.CustomerId}, InvoiceId={invoice.InvoiceId}）");
        }

        // ================= 2. 手动事务：提交 =================
        Console.WriteLine("【2】手动事务：插入歌手并提交");
        using (var db = DbPaths.CreateContext())
        {
            using (var tx = db.Database.BeginTransaction())
            {
                db.Artists.Add(new Artist { Name = "事务提交歌手" });
                db.SaveChanges();
                tx.Commit();                 // 显式提交
                Console.WriteLine("    事务已提交");
            }

            var committed = db.Artists.Any(a => a.Name == "事务提交歌手");
            Console.WriteLine($"    提交后查询：{(committed ? "数据已持久化" : "数据丢失")}");
        }

        // ================= 3. 手动事务：回滚 =================
        Console.WriteLine("【3】手动事务：插入歌手后回滚");
        using (var db = DbPaths.CreateContext())
        {
            using (var tx = db.Database.BeginTransaction())
            {
                db.Artists.Add(new Artist { Name = "事务回滚歌手" });
                db.SaveChanges();            // 事务内已写入（未提交）
                tx.Rollback();               // 显式回滚
                Console.WriteLine("    事务已回滚");
            }

            var rolledBack = db.Artists.Any(a => a.Name == "事务回滚歌手");
            Console.WriteLine($"    回滚后查询：{(rolledBack ? "数据存在（异常！）" : "数据不存在，回滚生效")}");
        }

        // ================= 4. 异常自动回滚：不 Commit 直接释放 =================
        Console.WriteLine("【4】异常自动回滚：事务中途抛出异常");
        using (var db = DbPaths.CreateContext())
        {
            using (var tx = db.Database.BeginTransaction())
            {
                try
                {
                    db.Artists.Add(new Artist { Name = "异常回滚歌手" });
                    db.SaveChanges();
                    throw new InvalidOperationException("模拟业务中途失败"); // 未 Commit
                }
                catch (InvalidOperationException)
                {
                    Console.WriteLine("    捕获异常：事务未提交");
                }
            }                                // using 释放事务 → 自动回滚

            var autoRolledBack = db.Artists.Any(a => a.Name == "异常回滚歌手");
            Console.WriteLine($"    异常后查询：{(autoRolledBack ? "数据存在（异常！）" : "数据不存在，自动回滚生效")}");
        }

        // ================= 5. 小结 =================
        Console.WriteLine();
        Console.WriteLine("小结：SaveChanges 自带事务；多步写入用 BeginTransaction 控制提交/回滚；事务对象释放时未提交会自动回滚。");
    }
}
