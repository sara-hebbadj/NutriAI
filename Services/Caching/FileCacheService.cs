using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace NutriAI.Services.Caching;

public class FileCacheService : ICacheService
{
    private readonly string _cacheDir;

    public FileCacheService()
    {
        _cacheDir = Path.Combine(FileSystem.AppDataDirectory, "cache");
        Directory.CreateDirectory(_cacheDir);
    }

    public async Task SetAsync<T>(string key, T data, TimeSpan ttl)
    {
        var envelope = new CacheEnvelope<T>
        {
            SavedAt = DateTimeOffset.UtcNow,
            Ttl = ttl,
            Data = data
        };

        var json = JsonSerializer.Serialize(envelope);
        await File.WriteAllTextAsync(GetPath(key), json);
    }

    public async Task<(bool found, T? data)> GetAsync<T>(string key)
    {
        var path = GetPath(key);
        if (!File.Exists(path))
            return (false, default);

        var json = await File.ReadAllTextAsync(path);
        var envelope = JsonSerializer.Deserialize<CacheEnvelope<T>>(json);

        if (envelope == null || envelope.IsExpired)
        {
            TryDelete(path);
            return (false, default);
        }

        return (true, envelope.Data);
    }

    public Task RemoveAsync(string key)
    {
        TryDelete(GetPath(key));
        return Task.CompletedTask;
    }

    private string GetPath(string key)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(key));
        return Path.Combine(_cacheDir, Convert.ToHexString(hash) + ".json");
    }

    private static void TryDelete(string path)
    {
        try { if (File.Exists(path)) File.Delete(path); } catch { }
    }
}
