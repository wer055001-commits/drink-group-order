using System.Text.Json;

namespace Backend;

/// <summary>
/// 泛型 JSON 檔案存取工具。
/// 將任意型別的資料序列化為 JSON 檔案，存放在 Data/ 目錄下。
/// </summary>
public class JsonStore<T> where T : class, new()
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _lock = new(1, 1);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public JsonStore(string filePath)
    {
        _filePath = filePath;

        var dir = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);
    }

    /// <summary>讀取資料，檔案不存在時回傳預設值</summary>
    public async Task<T> LoadAsync()
    {
        if (!File.Exists(_filePath))
            return new T();

        await _lock.WaitAsync();
        try
        {
            var json = await File.ReadAllTextAsync(_filePath);
            return JsonSerializer.Deserialize<T>(json, JsonOptions) ?? new T();
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>寫入資料</summary>
    public async Task SaveAsync(T data)
    {
        await _lock.WaitAsync();
        try
        {
            var json = JsonSerializer.Serialize(data, JsonOptions);
            await File.WriteAllTextAsync(_filePath, json);
        }
        finally
        {
            _lock.Release();
        }
    }
}
