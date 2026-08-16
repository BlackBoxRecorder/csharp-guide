using Microsoft.EntityFrameworkCore;

namespace EFCoreDemo.Demos;

/// <summary>
/// 演示 9：进阶关系 —— 一对一 / 原生多对多 / 延迟加载。
///
/// 知识点：
///   · 一对一：外键必须唯一（HasOne/WithOne 自动建立唯一约束），重复关联会报错
///   · 原生多对多：EF Core 5+ 两端 ICollection 导航即可，自动创建中间表 CourseStudent
///   · 延迟加载：导航属性标记 virtual + UseLazyLoadingProxies，访问时才查库（注意 N+1）
///
/// 说明：本 Demo 运行前演示库需已由迁移创建（MigrateAsync 幂等保证）；
///       每个小节使用独立的 DbContext，避免跟踪图残留干扰演示结果。
/// </summary>
public static class RelationshipAdvancedDemo
{
    public static async Task Run()
    {
        // ================= 0. 准备：先建库并写入演示数据 =================
        using (var db = DemoDbPaths.CreateContext())
        {
            await db.Database.MigrateAsync();
            if (!await db.Students.AnyAsync())
            {
                var student = new Student { Name = "张三" };
                student.Courses.Add(new Course { Name = "C# 编程" });
                student.Courses.Add(new Course { Name = "数据库原理" });
                await db.Students.AddAsync(student);
                await db.SaveChangesAsync();
                Console.WriteLine("【0】已写入演示数据：学生张三 + 两门课程");
            }
        }

        // ================= 1. 一对一：User ↔ UserProfile =================
        Console.WriteLine("【1】一对一：User ↔ UserProfile（外键唯一）");
        using (var db = DemoDbPaths.CreateContext())
        {
            // 元数据：外键必须唯一，才是一对一
            var fk = db.Model
                .FindEntityType(typeof(UserProfile))!
                .GetForeignKeys()
                .Single(f => f.PrincipalEntityType.ClrType == typeof(User));
            Console.WriteLine($"    外键 UserProfile.UserId 唯一约束：{fk.IsUnique}（一对一特征）");

            // 写入数据：用户 + 资料
            var user = new User { UserName = "zhangsan" };
            user.Profile = new UserProfile { Email = "zhangsan@example.com", Phone = "13800000000" };
            await db.Users.AddAsync(user);
            await db.SaveChangesAsync();
            Console.WriteLine($"    已写入用户（Id={user.Id}）及资料（UserProfileId={user.Profile!.Id}）");

            // Include 加载：一对一用引用导航
            var loaded = await db.Users
                .Include(u => u.Profile)
                .FirstAsync(u => u.Id == user.Id);
            Console.WriteLine($"    Include 查询：{loaded.UserName} → {loaded.Profile?.Email} / {loaded.Profile?.Phone}");
        }

        // 唯一约束生效：用独立上下文插入重复外键（避免同上下文身份解析干扰），应触发唯一约束
        using (var db = DemoDbPaths.CreateContext())
        {
            try
            {
                var duplicate = new UserProfile { UserId = 1, Email = "dup@example.com", Phone = "000" };
                await db.UserProfiles.AddAsync(duplicate);
                await db.SaveChangesAsync();
                Console.WriteLine("    （异常！）重复的外键居然写入成功了");
            }
            catch (DbUpdateException)
            {
                Console.WriteLine("    重复绑定同一用户的资料 → DbUpdateException（唯一约束生效）");
            }
        }

        // ================= 2. 原生多对多：Student ↔ Course =================
        Console.WriteLine("【2】原生多对多：Student ↔ Course（自动中间表）");
        using (var db = DemoDbPaths.CreateContext())
        {
            // 元数据：SkipNavigation 自动生成中间表
            var nav = db.Model
                .FindEntityType(typeof(Student))!
                .GetSkipNavigations()
                .Single();
            Console.WriteLine($"    多对多导航：Student.{nav.Name}，中间表：{nav.JoinEntityType!.GetTableName()}");

            // Include 查询：无需手动触碰中间表
            var student = await db.Students
                .Include(s => s.Courses)
                .FirstAsync(s => s.Name == "张三");
            Console.WriteLine($"    {student.Name} 选修了 {student.Courses.Count} 门课：");
            foreach (var c in student.Courses)
            {
                Console.WriteLine($"      · {c.Name}");
            }

            // 生成的 SQL：JOIN 自动创建的中间表
            var sql = db.Students
                .Where(s => s.Name == "张三")
                .Include(s => s.Courses)
                .ToQueryString();
            Console.WriteLine("    生成的 SQL（JOIN CourseStudent）：");
            foreach (var line in sql.Split('\n'))
            {
                Console.WriteLine($"      {line}");
            }
        }

        // ================= 3. 延迟加载：UseLazyLoadingProxies =================
        Console.WriteLine("【3】延迟加载：访问导航属性时才查库");
        using (var db = DemoDbPaths.CreateContext())
        {
            var student = await db.Students.FirstAsync(s => s.Name == "张三");
            Console.WriteLine($"    普通上下文（无代理）：访问 Courses 时 {student.Courses.Count} 门（未自动加载）");
        }

        using (var db = DemoDbPaths.CreateLazyLoadingContext())
        {
            var student = await db.Students.FirstAsync(s => s.Name == "张三");
            Console.WriteLine($"    延迟加载上下文：访问 Courses 时自动触发查询，共 {student.Courses.Count} 门：");
            foreach (var c in student.Courses)
            {
                Console.WriteLine($"      · {c.Name}");
            }
            Console.WriteLine("    原理：导航属性标记 virtual，代理类重写访问器，首次访问时自动执行查询");
            Console.WriteLine("    注意：延迟加载容易引发 N+1 查询，批量场景慎用");
        }

        // ================= 4. 小结 =================
        Console.WriteLine();
        Console.WriteLine("小结：一对一靠唯一外键；多对多靠自动中间表（两端 ICollection 导航）；延迟加载用 virtual + UseLazyLoadingProxies，按需取数但小心 N+1。");
    }
}
