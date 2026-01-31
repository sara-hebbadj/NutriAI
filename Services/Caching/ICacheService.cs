using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NutriAI.Services.Caching;

public interface ICacheService
{
    Task SetAsync<T>(string key, T data, TimeSpan ttl);
    Task<(bool found, T? data)> GetAsync<T>(string key);
    Task RemoveAsync(string key);
}
