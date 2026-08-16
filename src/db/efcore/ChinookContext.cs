using Microsoft.EntityFrameworkCore;

namespace EFCoreDemo;

/// <summary>
/// Chinook 数据库的 DbContext。
/// 表名、列名与实体属性名一致（Chinook 表结构本身就是 PascalCase），
/// 因此只需在 OnModelCreating 中配置主键、关系和精度。
/// </summary>
public class ChinookContext(DbContextOptions<ChinookContext> options) : DbContext(options)
{
    public DbSet<Artist> Artists => Set<Artist>();
    public DbSet<Album> Albums => Set<Album>();
    public DbSet<Track> Tracks => Set<Track>();
    public DbSet<Genre> Genres => Set<Genre>();
    public DbSet<MediaType> MediaTypes => Set<MediaType>();
    public DbSet<Playlist> Playlists => Set<Playlist>();
    public DbSet<PlaylistTrack> PlaylistTracks => Set<PlaylistTrack>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceLine> InvoiceLines => Set<InvoiceLine>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // —— 表名映射：Chinook 表名为单数，与 DbSet 属性名（复数）不一致，需显式指定 ——
        modelBuilder.Entity<Artist>().ToTable("Artist");
        modelBuilder.Entity<Album>().ToTable("Album");
        modelBuilder.Entity<Track>().ToTable("Track");
        modelBuilder.Entity<Genre>().ToTable("Genre");
        modelBuilder.Entity<MediaType>().ToTable("MediaType");
        modelBuilder.Entity<Playlist>().ToTable("Playlist");
        modelBuilder.Entity<PlaylistTrack>().ToTable("PlaylistTrack");
        modelBuilder.Entity<Customer>().ToTable("Customer");
        modelBuilder.Entity<Employee>().ToTable("Employee");
        modelBuilder.Entity<Invoice>().ToTable("Invoice");
        modelBuilder.Entity<InvoiceLine>().ToTable("InvoiceLine");

        // —— 复合主键：多对多中间表 PlaylistTrack ——
        modelBuilder.Entity<PlaylistTrack>()
            .HasKey(pt => new { pt.PlaylistId, pt.TrackId });

        // —— 精度：金额字段 ——
        modelBuilder.Entity<Track>()
            .Property(t => t.UnitPrice)
            .HasPrecision(10, 2);
        modelBuilder.Entity<Invoice>()
            .Property(i => i.Total)
            .HasPrecision(10, 2);
        modelBuilder.Entity<InvoiceLine>()
            .Property(il => il.UnitPrice)
            .HasPrecision(10, 2);

        // —— 关系：歌手 1 — N 专辑 ——
        modelBuilder.Entity<Album>()
            .HasOne(a => a.Artist)
            .WithMany(ar => ar.Albums)
            .HasForeignKey(a => a.ArtistId);

        // —— 关系：专辑 1 — N 曲目 ——
        modelBuilder.Entity<Track>()
            .HasOne(t => t.Album)
            .WithMany(a => a.Tracks)
            .HasForeignKey(t => t.AlbumId);

        // —— 关系：流派 1 — N 曲目 ——
        modelBuilder.Entity<Track>()
            .HasOne(t => t.Genre)
            .WithMany(g => g.Tracks)
            .HasForeignKey(t => t.GenreId);

        // —— 关系：媒体类型 1 — N 曲目 ——
        modelBuilder.Entity<Track>()
            .HasOne(t => t.MediaType)
            .WithMany(m => m.Tracks)
            .HasForeignKey(t => t.MediaTypeId);

        // —— 关系：播放列表 N — N 曲目（经中间表 PlaylistTrack）——
        modelBuilder.Entity<PlaylistTrack>()
            .HasOne(pt => pt.Playlist)
            .WithMany(p => p.PlaylistTracks)
            .HasForeignKey(pt => pt.PlaylistId);
        modelBuilder.Entity<PlaylistTrack>()
            .HasOne(pt => pt.Track)
            .WithMany(t => t.PlaylistTracks)
            .HasForeignKey(pt => pt.TrackId);

        // —— 关系：员工自引用（上级 — 下属）——
        modelBuilder.Entity<Employee>()
            .HasOne(e => e.ReportsToManager)
            .WithMany(m => m.InverseReportsTo)
            .HasForeignKey(e => e.ReportsTo);

        // —— 关系：员工 1 — N 客户（销售代表）——
        modelBuilder.Entity<Customer>()
            .HasOne(c => c.SupportRep)
            .WithMany(e => e.Customers)
            .HasForeignKey(c => c.SupportRepId);

        // —— 关系：客户 1 — N 订单 ——
        modelBuilder.Entity<Invoice>()
            .HasOne(i => i.Customer)
            .WithMany(c => c.Invoices)
            .HasForeignKey(i => i.CustomerId);

        // —— 关系：订单 1 — N 订单明细 ——
        modelBuilder.Entity<InvoiceLine>()
            .HasOne(il => il.Invoice)
            .WithMany(i => i.InvoiceLines)
            .HasForeignKey(il => il.InvoiceId);

        // —— 关系：曲目 1 — N 订单明细 ——
        modelBuilder.Entity<InvoiceLine>()
            .HasOne(il => il.Track)
            .WithMany(t => t.InvoiceLines)
            .HasForeignKey(il => il.TrackId);
    }
}
