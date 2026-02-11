using System.Text.Json;
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

builder.Services.AddSingleton(new JsonStore<DrinkOptions>(
    Path.Combine(dataDir, "drink-options.json")));

// ── CORS ──────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy.WithOrigins("http://localhost:5173")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
        else
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
    });
});

var app = builder.Build();

// ── 靜態檔案（佈署時提供畫面）─────────────────────
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseCors();

// 自動建立資料庫 & 初始菜單
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    if (!db.DrinkShops.Any())
    {
        SeedData.Initialize(db);
    }
}

// 初始化飲料選項
var drinkOptionsStore = app.Services.GetRequiredService<JsonStore<DrinkOptions>>();
var currentOptions = await drinkOptionsStore.LoadAsync();
if (currentOptions.SweetLevels.Count == 0)
{
    await drinkOptionsStore.SaveAsync(new DrinkOptions());
}

// ── 健康檢查 ──────────────────────────────────────

app.MapGet("/api/health", () => new
{
    Status = "OK",
    ServerTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
    Environment = app.Environment.EnvironmentName,
    DotnetVersion = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription
});

// ── 飲料店與菜單 ────────────────────────────────────

app.MapGet("/api/shops", async (AppDbContext db) =>
    await db.DrinkShops
        .Where(s => s.IsActive)
        .OrderBy(s => s.SortOrder)
        .Select(s => new
        {
            s.Id,
            s.Name,
            MenuItemCount = db.MenuItems.Count(m => m.ShopId == s.Id && m.IsActive)
        })
        .ToListAsync());

app.MapGet("/api/shops/{id}", async (int id, AppDbContext db) =>
{
    var shop = await db.DrinkShops.FirstOrDefaultAsync(s => s.Id == id && s.IsActive);
    if (shop is null) return Results.NotFound();

    var items = await db.MenuItems
        .Where(m => m.ShopId == id && m.IsActive)
        .OrderBy(m => m.SortOrder)
        .ToListAsync();

    var categories = items
        .GroupBy(m => m.Category)
        .Select(g => new
        {
            Name = g.Key,
            Items = g.Select(m => new
            {
                m.Id,
                m.Name,
                m.PriceMedium,
                m.PriceLarge
            }).ToList()
        })
        .ToList();

    return Results.Ok(new
    {
        Shop = new { shop.Id, shop.Name },
        Categories = categories
    });
});

app.MapGet("/api/drink-options", async (JsonStore<DrinkOptions> store) =>
    await store.LoadAsync());

// ── 團購單 ─────────────────────────────────────────

app.MapGet("/api/group-orders", async (string? status, AppDbContext db) =>
{
    var query = db.GroupOrders
        .Include(g => g.Shop)
        .Include(g => g.OrderItems)
        .AsQueryable();

    if (!string.IsNullOrEmpty(status))
    {
        query = query.Where(g => g.Status == status);
    }

    var orders = await query
        .OrderByDescending(g => g.CreatedAt)
        .ToListAsync();

    // 自動截止已過期的團購
    foreach (var order in orders.Where(o => o.Status == "開放中" && o.Deadline < DateTime.Now))
    {
        order.Status = "已截止";
    }
    await db.SaveChangesAsync();

    return orders.Select(g => new
    {
        g.Id,
        ShopName = g.Shop.Name,
        g.CreatorName,
        g.Title,
        Deadline = g.Deadline.ToString("yyyy-MM-ddTHH:mm:ss"),
        g.Status,
        ItemCount = g.OrderItems.Select(o => o.PersonName).Distinct().Count(),
        TotalCups = g.OrderItems.Sum(o => o.Quantity),
        TotalPrice = g.OrderItems.Sum(o => o.SubTotal),
        CreatedAt = g.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss")
    });
});

app.MapPost("/api/group-orders", async (AppDbContext db, CreateGroupOrderRequest input) =>
{
    var shop = await db.DrinkShops.FindAsync(input.ShopId);
    if (shop is null) return Results.BadRequest("找不到這家飲料店");

    var order = new GroupOrder
    {
        ShopId = input.ShopId,
        CreatorName = input.CreatorName.Trim(),
        Title = input.Title?.Trim(),
        Deadline = input.Deadline,
        Status = "開放中",
        CreatedAt = DateTime.Now
    };

    db.GroupOrders.Add(order);
    await db.SaveChangesAsync();

    return Results.Created($"/api/group-orders/{order.Id}", new { order.Id });
});

app.MapGet("/api/group-orders/{id}", async (int id, AppDbContext db) =>
{
    var order = await db.GroupOrders
        .Include(g => g.Shop)
        .Include(g => g.OrderItems)
            .ThenInclude(o => o.MenuItem)
        .FirstOrDefaultAsync(g => g.Id == id);

    if (order is null) return Results.NotFound();

    // 自動截止已過期的團購
    if (order.Status == "開放中" && order.Deadline < DateTime.Now)
    {
        order.Status = "已截止";
        await db.SaveChangesAsync();
    }

    var byPerson = order.OrderItems
        .GroupBy(o => o.PersonName)
        .Select(g => new
        {
            Name = g.Key,
            Items = g.Select(o => new
            {
                o.Id,
                o.MenuItemId,
                MenuItemName = o.MenuItem.Name,
                o.PersonName,
                o.Size,
                o.SweetLevel,
                o.IceLevel,
                Toppings = string.IsNullOrEmpty(o.Toppings)
                    ? Array.Empty<string>()
                    : JsonSerializer.Deserialize<string[]>(o.Toppings) ?? Array.Empty<string>(),
                o.Quantity,
                o.Note,
                o.SubTotal
            }).ToList(),
            PersonTotal = g.Sum(o => o.SubTotal)
        })
        .ToList();

    return Results.Ok(new
    {
        order.Id,
        Shop = new { order.Shop.Id, order.Shop.Name },
        order.CreatorName,
        order.Title,
        Deadline = order.Deadline.ToString("yyyy-MM-ddTHH:mm:ss"),
        order.Status,
        CreatedAt = order.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss"),
        Summary = new
        {
            TotalItems = order.OrderItems.Select(o => o.PersonName).Distinct().Count(),
            TotalCups = order.OrderItems.Sum(o => o.Quantity),
            TotalPrice = order.OrderItems.Sum(o => o.SubTotal),
            ByPerson = byPerson
        }
    });
});

app.MapPut("/api/group-orders/{id}/status", async (int id, AppDbContext db, UpdateStatusRequest input) =>
{
    var order = await db.GroupOrders.FindAsync(id);
    if (order is null) return Results.NotFound();

    if (input.Status != "已截止" && input.Status != "已結單" && input.Status != "開放中")
    {
        return Results.BadRequest("狀態只能是：開放中、已截止、已結單");
    }

    order.Status = input.Status;
    await db.SaveChangesAsync();
    return Results.Ok(new { order.Id, order.Status });
});

app.MapDelete("/api/group-orders/{id}", async (int id, AppDbContext db) =>
{
    var order = await db.GroupOrders
        .Include(g => g.OrderItems)
        .FirstOrDefaultAsync(g => g.Id == id);

    if (order is null) return Results.NotFound();

    db.OrderItems.RemoveRange(order.OrderItems);
    db.GroupOrders.Remove(order);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// ── 個人點單 ───────────────────────────────────────

app.MapPost("/api/group-orders/{groupId}/items", async (int groupId, AppDbContext db,
    JsonStore<DrinkOptions> optionsStore, CreateOrderItemRequest input) =>
{
    var order = await db.GroupOrders.FindAsync(groupId);
    if (order is null) return Results.NotFound("找不到這筆團購");
    if (order.Status != "開放中") return Results.BadRequest("團購已截止，無法再點餐");

    var menuItem = await db.MenuItems.FindAsync(input.MenuItemId);
    if (menuItem is null) return Results.BadRequest("找不到這個品項");

    // 計算小計
    var options = await optionsStore.LoadAsync();
    var basePrice = input.Size == "大杯" ? menuItem.PriceLarge : menuItem.PriceMedium;
    var toppingsPrice = 0;
    var toppingsList = input.Toppings ?? [];
    foreach (var topping in toppingsList)
    {
        var match = options.Toppings.FirstOrDefault(t => t.Name == topping);
        if (match is not null) toppingsPrice += match.Price;
    }
    var subTotal = (basePrice + toppingsPrice) * input.Quantity;

    var item = new OrderItem
    {
        GroupOrderId = groupId,
        MenuItemId = input.MenuItemId,
        PersonName = input.PersonName.Trim(),
        Size = input.Size,
        SweetLevel = input.SweetLevel,
        IceLevel = input.IceLevel,
        Toppings = toppingsList.Length > 0 ? JsonSerializer.Serialize(toppingsList) : null,
        Quantity = input.Quantity,
        Note = input.Note?.Trim(),
        SubTotal = subTotal,
        CreatedAt = DateTime.Now
    };

    db.OrderItems.Add(item);
    await db.SaveChangesAsync();

    return Results.Created($"/api/group-orders/{groupId}/items/{item.Id}", new
    {
        item.Id,
        item.SubTotal
    });
});

app.MapPut("/api/group-orders/{groupId}/items/{itemId}", async (int groupId, int itemId,
    AppDbContext db, JsonStore<DrinkOptions> optionsStore, CreateOrderItemRequest input) =>
{
    var item = await db.OrderItems.FirstOrDefaultAsync(o => o.Id == itemId && o.GroupOrderId == groupId);
    if (item is null) return Results.NotFound();

    var menuItem = await db.MenuItems.FindAsync(input.MenuItemId);
    if (menuItem is null) return Results.BadRequest("找不到這個品項");

    var options = await optionsStore.LoadAsync();
    var basePrice = input.Size == "大杯" ? menuItem.PriceLarge : menuItem.PriceMedium;
    var toppingsPrice = 0;
    var toppingsList = input.Toppings ?? [];
    foreach (var topping in toppingsList)
    {
        var match = options.Toppings.FirstOrDefault(t => t.Name == topping);
        if (match is not null) toppingsPrice += match.Price;
    }

    item.MenuItemId = input.MenuItemId;
    item.PersonName = input.PersonName.Trim();
    item.Size = input.Size;
    item.SweetLevel = input.SweetLevel;
    item.IceLevel = input.IceLevel;
    item.Toppings = toppingsList.Length > 0 ? JsonSerializer.Serialize(toppingsList) : null;
    item.Quantity = input.Quantity;
    item.Note = input.Note?.Trim();
    item.SubTotal = (basePrice + toppingsPrice) * input.Quantity;

    await db.SaveChangesAsync();
    return Results.Ok(new { item.Id, item.SubTotal });
});

app.MapDelete("/api/group-orders/{groupId}/items/{itemId}", async (int groupId, int itemId, AppDbContext db) =>
{
    var item = await db.OrderItems.FirstOrDefaultAsync(o => o.Id == itemId && o.GroupOrderId == groupId);
    if (item is null) return Results.NotFound();

    db.OrderItems.Remove(item);
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

// ── SPA 路由支援（非 API 的路徑都回傳首頁）──────
app.MapFallbackToFile("index.html");

app.Run();

// ── 請求模型 ──────────────────────────────────────

public class CreateGroupOrderRequest
{
    public int ShopId { get; set; }
    public required string CreatorName { get; set; }
    public string? Title { get; set; }
    public DateTime Deadline { get; set; }
}

public class CreateOrderItemRequest
{
    public int MenuItemId { get; set; }
    public required string PersonName { get; set; }
    public required string Size { get; set; }
    public required string SweetLevel { get; set; }
    public required string IceLevel { get; set; }
    public string[]? Toppings { get; set; }
    public int Quantity { get; set; } = 1;
    public string? Note { get; set; }
}

public class UpdateStatusRequest
{
    public required string Status { get; set; }
}

// ── 飲料選項設定 ──────────────────────────────────

public class DrinkOptions
{
    public List<string> SweetLevels { get; set; } = ["正常甜", "少糖", "半糖", "微糖", "無糖"];
    public List<string> IceLevels { get; set; } = ["正常冰", "少冰", "微冰", "去冰", "完全去冰", "溫", "熱"];
    public List<string> Sizes { get; set; } = ["中杯", "大杯"];
    public List<ToppingOption> Toppings { get; set; } =
    [
        new() { Name = "珍珠", Price = 10 },
        new() { Name = "波霸", Price = 10 },
        new() { Name = "椰果", Price = 10 },
        new() { Name = "仙草", Price = 10 },
        new() { Name = "粉條", Price = 10 },
        new() { Name = "芋圓", Price = 15 },
        new() { Name = "布丁", Price = 15 },
        new() { Name = "蘆薈", Price = 15 },
        new() { Name = "寒天", Price = 10 },
    ];
}

public class ToppingOption
{
    public required string Name { get; set; }
    public int Price { get; set; }
}

/// <summary>網站設定（存為 JSON 檔案）</summary>
public class SiteSettings
{
    public string SiteName { get; set; } = "團購飲料系統";
    public string Description { get; set; } = "和同事朋友一起訂飲料！";
}

// ── 種子資料 ──────────────────────────────────────

public static class SeedData
{
    public static void Initialize(AppDbContext db)
    {
        // 50嵐
        var shop1 = new DrinkShop { Name = "50嵐", SortOrder = 1 };
        db.DrinkShops.Add(shop1);
        db.SaveChanges();
        AddMenuItems(db, shop1.Id, new (string category, string name, int medium, int large)[]
        {
            ("純茶類", "四季春茶", 25, 35),
            ("純茶類", "阿里山冷露", 30, 40),
            ("純茶類", "鮮柚綠", 40, 50),
            ("純茶類", "8冰綠", 25, 35),
            ("純茶類", "四季春青茶", 25, 35),
            ("找好茶", "黃金烏龍", 30, 40),
            ("奶茶類", "波霸奶茶", 40, 50),
            ("奶茶類", "燕麥奶茶", 45, 55),
            ("特調類", "阿華田", 45, 55),
            ("特調類", "巧克力牛奶", 50, 60),
        });

        // 清心福全
        var shop2 = new DrinkShop { Name = "清心福全", SortOrder = 2 };
        db.DrinkShops.Add(shop2);
        db.SaveChanges();
        AddMenuItems(db, shop2.Id, new (string category, string name, int medium, int large)[]
        {
            ("經典茶飲", "珍珠綠茶", 30, 40),
            ("經典茶飲", "冬瓜茶", 25, 30),
            ("經典茶飲", "烏龍綠茶", 25, 35),
            ("奶茶系列", "珍珠奶茶", 40, 50),
            ("奶茶系列", "仙草奶凍", 45, 55),
            ("特調系列", "冬瓜檸檬", 35, 45),
            ("特調系列", "多多綠茶", 40, 50),
            ("鮮果系列", "鳳梨冰茶", 45, 55),
        });

        // 可不可熟成紅茶
        var shop3 = new DrinkShop { Name = "可不可熟成紅茶", SortOrder = 3 };
        db.DrinkShops.Add(shop3);
        db.SaveChanges();
        AddMenuItems(db, shop3.Id, new (string category, string name, int medium, int large)[]
        {
            ("熟成紅茶", "熟成紅茶", 30, 40),
            ("熟成紅茶", "太妃紅茶", 30, 40),
            ("熟成紅茶", "麗春紅茶", 35, 45),
            ("熟成奶茶", "熟成奶茶", 45, 55),
            ("熟成奶茶", "雲朵奶茶", 55, 65),
            ("熟成拿鐵", "熟成紅拿鐵", 55, 65),
            ("熟成拿鐵", "雪花白玉拿鐵", 60, 70),
            ("季節限定", "白玉歐蕾", 55, 65),
        });

        // 迷客夏
        var shop4 = new DrinkShop { Name = "迷客夏", SortOrder = 4 };
        db.DrinkShops.Add(shop4);
        db.SaveChanges();
        AddMenuItems(db, shop4.Id, new (string category, string name, int medium, int large)[]
        {
            ("綠光牧場鮮乳", "綠光鮮乳茶", 55, 65),
            ("綠光牧場鮮乳", "芋頭鮮乳", 60, 70),
            ("經典紅茶", "大正紅茶", 25, 35),
            ("經典紅茶", "娜杯紅茶", 30, 40),
            ("手作特調", "柳橙綠", 45, 55),
            ("手作特調", "芝芝芒果", 65, 75),
            ("手搖奶茶", "珍珠奶茶", 45, 55),
            ("手搖奶茶", "椰果奶茶", 45, 55),
        });
    }

    private static void AddMenuItems(AppDbContext db, int shopId,
        (string category, string name, int medium, int large)[] items)
    {
        var sort = 1;
        foreach (var (category, name, medium, large) in items)
        {
            db.MenuItems.Add(new MenuItem
            {
                ShopId = shopId,
                Name = name,
                Category = category,
                PriceMedium = medium,
                PriceLarge = large,
                SortOrder = sort++
            });
        }
        db.SaveChanges();
    }
}
