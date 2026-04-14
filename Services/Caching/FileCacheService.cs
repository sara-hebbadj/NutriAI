// Authorship note:
// Microsoft Learn documentation was used in this file for FileSystem.AppDataDirectory,
// System.Text.Json, File.ReadAllTextAsync, File.WriteAllTextAsync, SHA256, and Path.Combine.
// Copilot was used to help draft and refine the file-based cache implementation.
// The decision to use local JSON caching with expiry, hashed cache keys, and invalidation of expired cache
// for NutriAI was made and tested by the author.

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace NutriAI.Services.Caching;

public class FileCacheService : ICacheService
{
    private readonly string _cacheDir;

    public FileCacheService()
    {
        // Create a folder inside the app data directory where cache files will be stored.
        _cacheDir = Path.Combine(FileSystem.AppDataDirectory, "cache");
        Directory.CreateDirectory(_cacheDir);
    }

    public async Task SetAsync<T>(string key, T data, TimeSpan ttl)
    {
        // Wrap the data together with the time it was saved
        // and the length of time it is allowed to stay valid.
        var envelope = new CacheEnvelope<T>
        {
            SavedAt = DateTimeOffset.UtcNow,
            Ttl = ttl,
            Data = data
        };

        // Save the cache entry as JSON so it can be read back later.
        var json = JsonSerializer.Serialize(envelope);
        await File.WriteAllTextAsync(GetPath(key), json);
    }

    public async Task<(bool found, T? data)> GetAsync<T>(string key)
    {
        var path = GetPath(key);
        // If there is no file for this key, the cache does not contain the data.
        if (!File.Exists(path))
            return (false, default);

        // Read the cache file and convert it back into a CacheEnvelope object.
        var json = await File.ReadAllTextAsync(path);
        var envelope = JsonSerializer.Deserialize<CacheEnvelope<T>>(json);

        // If the file cannot be read properly or the cache has expired,
        // delete it and treat it as a cache miss.
        if (envelope == null || envelope.IsExpired)
        {
            TryDelete(path);
            return (false, default);
        }

        // If the cache is still valid, return the stored data.
        return (true, envelope.Data);
    }

    public Task RemoveAsync(string key)
    {
        // Remove this cache entry manually if it is no longer needed.
        TryDelete(GetPath(key));
        return Task.CompletedTask;
    }

    private string GetPath(string key)
    {
        // Convert the key into a SHA-256 hash so the filename is safe,
        // consistent, and does not depend on the original text format.
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(key));
        return Path.Combine(_cacheDir, Convert.ToHexString(hash) + ".json");
    }

    private static void TryDelete(string path)
    {
        // Cache deletion should not crash the app,
        // so any file deletion error is ignored here.
        try { if (File.Exists(path)) File.Delete(path); } catch { }
    }
}
