using Microsoft.EntityFrameworkCore;

namespace Backend;

// ── 資料模型 ──────────────────────────────────────

/// <summary>飲料店</summary>
public class DrinkShop
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public List<MenuItem> MenuItems { get; set; } = [];
}

/// <summary>菜單品項</summary>
public class MenuItem
{
    public int Id { get; set; }
    public int ShopId { get; set; }
    public DrinkShop Shop { get; set; } = null!;
    public required string Name { get; set; }
    public required string Category { get; set; }
    public int PriceMedium { get; set; }
    public int PriceLarge { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
}

/// <summary>團購單</summary>
public class GroupOrder
{
    public int Id { get; set; }
    public int ShopId { get; set; }
    public DrinkShop Shop { get; set; } = null!;
    public required string CreatorName { get; set; }
    public string? Title { get; set; }
    public DateTime Deadline { get; set; }
    public string Status { get; set; } = "開放中";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public List<OrderItem> OrderItems { get; set; } = [];
}

/// <summary>個人點單</summary>
public class OrderItem
{
    public int Id { get; set; }
    public int GroupOrderId { get; set; }
    public GroupOrder GroupOrder { get; set; } = null!;
    public int MenuItemId { get; set; }
    public MenuItem MenuItem { get; set; } = null!;
    public required string PersonName { get; set; }
    public required string Size { get; set; }
    public required string SweetLevel { get; set; }
    public required string IceLevel { get; set; }
    public string? Toppings { get; set; }
    public int Quantity { get; set; } = 1;
    public string? Note { get; set; }
    public int SubTotal { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

// ── 資料庫設定 ────────────────────────────────────

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<DrinkShop> DrinkShops => Set<DrinkShop>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<GroupOrder> GroupOrders => Set<GroupOrder>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MenuItem>().HasIndex(m => m.ShopId);
        modelBuilder.Entity<OrderItem>().HasIndex(o => o.GroupOrderId);
        modelBuilder.Entity<GroupOrder>().HasIndex(g => g.Status);
    }
}
