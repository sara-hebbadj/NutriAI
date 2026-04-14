// Authorship note:
// This interface was written by the author.
// Microsoft documentation was used as reference for async Task-based method signatures.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NutriAI.Services.Caching;

// This interface defines the main cache operations used by the app.
// It keeps the rest of the system independent from one specific cache implementation.
public interface ICacheService
{
    // Save data in the cache using a key and a chosen expiry time.
    Task SetAsync<T>(string key, T data, TimeSpan ttl);

    // Try to get data from the cache.
    // 'found' shows whether valid cached data exists.
    Task<(bool found, T? data)> GetAsync<T>(string key);

    // Remove a cache entry manually.
    Task RemoveAsync(string key);
}
