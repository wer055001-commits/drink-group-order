using Microsoft.EntityFrameworkCore;
using Backend;

var builder = WebApplication.CreateBuilder(args);

// ── 資料庫（SQLite）────────────────────────────────
var dataDir = Path.Combine(builder.Environment.ContentRootPath, "Data");
Directory.CreateDirectory(dataDir);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite($"Data Source={Path.Combine(dataDir, "app.db")}"));

// ── JSON 設定檔 ───────────────────────────────────
builder.Services.AddSingleton(new JsonStore<SiteSettings>(
    Path.Combine(dataDir, "site-settings.json")));

// ── CORS ──────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors();

// 自動建立資料庫
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

// ── 健康檢查 ──────────────────────────────────────

app.MapGet("/api/health", () => new
{
    Status = "OK",
    ServerTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
    Environment = app.Environment.EnvironmentName,
    DotnetVersion = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription
});

// ── 待辦事項 CRUD ─────────────────────────────────

app.MapGet("/api/todos", async (AppDbContext db) =>
    await db.Todos.OrderByDescending(t => t.CreatedAt).ToListAsync());

app.MapPost("/api/todos", async (AppDbContext db, TodoItem item) =>
{
    item.CreatedAt = DateTime.Now;
    db.Todos.Add(item);
    await db.SaveChangesAsync();
    return Results.Created($"/api/todos/{item.Id}", item);
});

app.MapPut("/api/todos/{id}", async (int id, AppDbContext db, TodoItem input) =>
{
    var todo = await db.Todos.FindAsync(id);
    if (todo is null) return Results.NotFound();

    todo.Title = input.Title;
    todo.IsDone = input.IsDone;
    await db.SaveChangesAsync();
    return Results.Ok(todo);
});

app.MapDelete("/api/todos/{id}", async (int id, AppDbContext db) =>
{
    var todo = await db.Todos.FindAsync(id);
    if (todo is null) return Results.NotFound();

    db.Todos.Remove(todo);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// ── 網站設定（JSON 檔案）──────────────────────────

app.MapGet("/api/settings", async (JsonStore<SiteSettings> store) =>
    await store.LoadAsync());

app.MapPut("/api/settings", async (JsonStore<SiteSettings> store, SiteSettings input) =>
{
    await store.SaveAsync(input);
    return Results.Ok(input);
});

app.Run();

// ── 資料模型 ──────────────────────────────────────

/// <summary>網站設定（存為 JSON 檔案）</summary>
public class SiteSettings
{
    public string SiteName { get; set; } = "我的 VibeCoding 網站";
    public string Description { get; set; } = "用 AI 打造的第一個作品";
}
