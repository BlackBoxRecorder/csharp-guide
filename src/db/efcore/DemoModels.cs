using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFCoreDemo;

// ============================================================
// 演示模型（DemoContext 专用）
//
// 用途：承载 Chinook 库无法表达的教学主题——
//   一对一（User/UserProfile）、原生多对多（Student/Course）、
//   数据注解（Product）、配置类 + 种子数据（Category）、
//   软删除/全局过滤与并发令牌（Product 的 IsDeleted/Version）。
//
// 说明：模型结构与 docs/basic/db/efcore 文档中的示例保持一致，
//       便于「读文档 → 跑演示」一一对应。
// ============================================================

/// <summary>
/// 商品表（Product）实体 —— 演示「数据注解」配置方式。
/// 配置全部通过特性完成：[Table] 表名、[Key] 主键、[Column] 列名、
/// [Required]/[MaxLength] 约束、[Precision] 精度、[NotMapped] 忽略映射、
/// [ConcurrencyCheck] 并发令牌。
/// </summary>
[Table("Product")]
public class Product
{
    [Key]
    [Column("product_id")]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("product_name")]
    public string Name { get; set; } = "";

    /// <summary>金额字段：SQLite 下用 Precision 注解控制精度。</summary>
    [Precision(10, 2)]
    public decimal Price { get; set; }

    /// <summary>该属性不映射到数据库（仅内存中使用）。</summary>
    [NotMapped]
    public string? TempData { get; set; }

    /// <summary>软删除标记：配合全局查询过滤（HasQueryFilter）演示。</summary>
    public bool IsDeleted { get; set; }

    /// <summary>商品描述：由 v2 迁移（AddProductDescription）新增的列，演示迁移升级。</summary>
    public string? Description { get; set; }

    /// <summary>
    /// 并发令牌：乐观并发演示。
    /// 注意：SQLite 没有 SQL Server 那样的原生 rowversion（[Timestamp] 不会自动更新），
    /// 因此用 [ConcurrencyCheck] 标记的版本号代替，每次修改前手动 Version++。
    /// </summary>
    [ConcurrencyCheck]
    public int Version { get; set; }

    public int CategoryId { get; set; }

    /// <summary>正向导航：商品所属分类。</summary>
    public virtual Category? Category { get; set; }
}

/// <summary>
/// 分类表（Category）实体 —— 通过独立的 IEntityTypeConfiguration
/// 配置类（CategoryConfiguration）完成映射，并演示 HasData 种子数据。
/// </summary>
public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = "";

    /// <summary>反向导航：该分类下的商品集合。</summary>
    public virtual List<Product> Products { get; set; } = [];
}

/// <summary>用户表（User）实体 —— 一对一关系的「一」端。</summary>
public class User
{
    public int Id { get; set; }
    public string UserName { get; set; } = "";

    /// <summary>正向导航：用户的资料（一对一）。</summary>
    public virtual UserProfile? Profile { get; set; }
}

/// <summary>用户资料表（UserProfile）实体 —— 一对一关系的「一」端（外键持有方）。</summary>
public class UserProfile
{
    public int Id { get; set; }
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";

    /// <summary>外键：一对一关系中，外键持有方需要唯一约束。</summary>
    public int UserId { get; set; }

    /// <summary>正向导航：资料所属用户。</summary>
    public virtual User? User { get; set; }
}

/// <summary>
/// 学生表（Student）实体 —— 原生多对多（EF Core 5+ 自动创建中间表 CourseStudent）。
/// 导航属性标记 virtual，供延迟加载代理（UseLazyLoadingProxies）重写。
/// </summary>
public class Student
{
    public int Id { get; set; }
    public string Name { get; set; } = "";

    /// <summary>反向导航：学生选修的课程集合（多对多）。</summary>
    public virtual ICollection<Course> Courses { get; set; } = [];
}

/// <summary>课程表（Course）实体 —— 原生多对多的「多」端。</summary>
public class Course
{
    public int Id { get; set; }
    public string Name { get; set; } = "";

    /// <summary>反向导航：选修该课程的学生集合（多对多）。</summary>
    public virtual ICollection<Student> Students { get; set; } = [];
}

/// <summary>
/// Category 的独立配置类 —— 演示 IEntityTypeConfiguration + ApplyConfigurationsFromAssembly。
/// </summary>
public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Category");

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        // 种子数据：HasData 要求主键显式指定，随迁移一并写入数据库
        builder.HasData(
            new Category { Id = 1, Name = "手机" },
            new Category { Id = 2, Name = "电脑" },
            new Category { Id = 3, Name = "配件" });
    }
}
