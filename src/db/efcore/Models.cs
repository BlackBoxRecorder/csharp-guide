namespace EFCoreDemo;

/// <summary>歌手表（Artist）实体。</summary>
public class Artist
{
    public int ArtistId { get; set; }
    public string? Name { get; set; }

    /// <summary>反向导航：该歌手的专辑集合（一对多）。</summary>
    public List<Album> Albums { get; set; } = [];
}

/// <summary>专辑表（Album）实体。</summary>
public class Album
{
    public int AlbumId { get; set; }
    public string Title { get; set; } = "";
    public int ArtistId { get; set; }

    /// <summary>正向导航：所属歌手。</summary>
    public Artist? Artist { get; set; }

    /// <summary>反向导航：专辑中的曲目集合（一对多）。</summary>
    public List<Track> Tracks { get; set; } = [];
}

/// <summary>曲目表（Track）实体。</summary>
public class Track
{
    public int TrackId { get; set; }
    public string Name { get; set; } = "";
    public int? AlbumId { get; set; }
    public int MediaTypeId { get; set; }
    public int? GenreId { get; set; }
    public string? Composer { get; set; }
    public int Milliseconds { get; set; }
    public long? Bytes { get; set; }
    public decimal UnitPrice { get; set; }

    /// <summary>正向导航：所属专辑。</summary>
    public Album? Album { get; set; }

    /// <summary>正向导航：所属流派。</summary>
    public Genre? Genre { get; set; }

    /// <summary>正向导航：所属媒体类型。</summary>
    public MediaType? MediaType { get; set; }

    /// <summary>反向导航：多对多中间表集合。</summary>
    public List<PlaylistTrack> PlaylistTracks { get; set; } = [];

    /// <summary>反向导航：曲目被哪些订单明细引用。</summary>
    public List<InvoiceLine> InvoiceLines { get; set; } = [];
}

/// <summary>流派表（Genre）实体。</summary>
public class Genre
{
    public int GenreId { get; set; }
    public string? Name { get; set; }

    /// <summary>反向导航：该流派的曲目集合。</summary>
    public List<Track> Tracks { get; set; } = [];
}

/// <summary>媒体类型表（MediaType）实体。</summary>
public class MediaType
{
    public int MediaTypeId { get; set; }
    public string? Name { get; set; }

    /// <summary>反向导航：该媒体类型的曲目集合。</summary>
    public List<Track> Tracks { get; set; } = [];
}

/// <summary>播放列表表（Playlist）实体。</summary>
public class Playlist
{
    public int PlaylistId { get; set; }
    public string? Name { get; set; }

    /// <summary>反向导航：多对多中间表集合。</summary>
    public List<PlaylistTrack> PlaylistTracks { get; set; } = [];
}

/// <summary>
/// 播放列表与曲目的多对多中间表（PlaylistTrack）。
/// 复合主键为 (PlaylistId, TrackId)，需在 OnModelCreating 中显式配置。
/// </summary>
public class PlaylistTrack
{
    public int PlaylistId { get; set; }
    public int TrackId { get; set; }

    public Playlist? Playlist { get; set; }
    public Track? Track { get; set; }
}

/// <summary>客户表（Customer）实体。</summary>
public class Customer
{
    public int CustomerId { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string? Company { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string? Phone { get; set; }
    public string? Fax { get; set; }
    public string Email { get; set; } = "";
    public int? SupportRepId { get; set; }

    /// <summary>正向导航：负责该客户的销售代表（员工）。</summary>
    public Employee? SupportRep { get; set; }

    /// <summary>反向导航：该客户的订单集合。</summary>
    public List<Invoice> Invoices { get; set; } = [];
}

/// <summary>员工表（Employee）实体。</summary>
public class Employee
{
    public int EmployeeId { get; set; }
    public string LastName { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string? Title { get; set; }
    public int? ReportsTo { get; set; }
    public DateTime? BirthDate { get; set; }
    public DateTime? HireDate { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string? Phone { get; set; }
    public string? Fax { get; set; }
    public string? Email { get; set; }

    /// <summary>正向导航：汇报对象（上级），自引用关系。</summary>
    public Employee? ReportsToManager { get; set; }

    /// <summary>反向导航：下属员工集合（自引用）。</summary>
    public List<Employee> InverseReportsTo { get; set; } = [];

    /// <summary>反向导航：该员工负责的客户集合。</summary>
    public List<Customer> Customers { get; set; } = [];
}

/// <summary>订单表（Invoice）实体。</summary>
public class Invoice
{
    public int InvoiceId { get; set; }
    public int CustomerId { get; set; }
    public DateTime InvoiceDate { get; set; }
    public string? BillingAddress { get; set; }
    public string? BillingCity { get; set; }
    public string? BillingState { get; set; }
    public string? BillingCountry { get; set; }
    public string? BillingPostalCode { get; set; }
    public decimal Total { get; set; }

    /// <summary>正向导航：下单客户。</summary>
    public Customer? Customer { get; set; }

    /// <summary>反向导航：订单明细集合（一对多）。</summary>
    public List<InvoiceLine> InvoiceLines { get; set; } = [];
}

/// <summary>订单明细表（InvoiceLine）实体。</summary>
public class InvoiceLine
{
    public int InvoiceLineId { get; set; }
    public int InvoiceId { get; set; }
    public int TrackId { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }

    /// <summary>正向导航：所属订单。</summary>
    public Invoice? Invoice { get; set; }

    /// <summary>正向导航：明细对应的曲目。</summary>
    public Track? Track { get; set; }
}
