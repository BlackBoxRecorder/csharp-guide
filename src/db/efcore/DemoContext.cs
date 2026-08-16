using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EFCoreDemo;

/// <summary>
/// 演示模型专用 DbContext —— 承载 Chinook 无法表达的教学主题。
/// 建库策略：统一由迁移（Database.Migrate）创建，见 Demo 10 迁移演示；
/// 因此演示库在 Program 启动时被强制删除，每次运行都是全新结构。
/// </summary>
public class DemoContext(DbContextOptions<DemoContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Course> Courses => Set<Course>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 应用程序集中的全部 IEntityTypeConfiguration 配置类（含 CategoryConfiguration + HasData 种子）
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DemoContext).Assembly);

        // —— 一对一：User ↔ UserProfile（外键唯一约束由 HasForeignKey 自动建立）——
        modelBuilder.Entity<User>()
            .HasOne(u => u.Profile)
            .WithOne(p => p.User)
            .HasForeignKey<UserProfile>(p => p.UserId);

        // —— 全局查询过滤：软删除（Product 的 IsDeleted）——
        // 所有查询自动追加 WHERE IsDeleted = 0；IgnoreQueryFilters() 可绕过
        modelBuilder.Entity<Product>()
            .HasQueryFilter(p => !p.IsDeleted);

        // —— 原生多对多：Student ↔ Course 无需配置，约定自动创建中间表 CourseStudent ——
    }
}

/// <summary>
/// 设计时工厂：供 `dotnet ef migrations add` 在无运行入口的情况下构建 DbContext。
/// 仅用于生成迁移，不会实际连接数据库。
/// </summary>
public class DemoContextFactory : IDesignTimeDbContextFactory<DemoContext>
{
    public DemoContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<DemoContext>()
            .UseSqlite("Data Source=DemoDemoDb.sqlite")
            .Options;
        return new DemoContext(options);
    }
}
