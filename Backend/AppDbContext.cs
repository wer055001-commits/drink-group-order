using Microsoft.EntityFrameworkCore;

namespace Backend;

// ── 資料模型 ──────────────────────────────────────

/// <summary>待辦事項</summary>
public class TodoItem
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public bool IsDone { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

// ── 資料庫設定 ────────────────────────────────────

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<TodoItem> Todos => Set<TodoItem>();
}
