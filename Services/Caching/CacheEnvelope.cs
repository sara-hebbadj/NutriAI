using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NutriAI.Services.Caching;

public class CacheEnvelope<T>
{
    public DateTimeOffset SavedAt { get; set; }
    public TimeSpan Ttl { get; set; }
    public T Data { get; set; }

    public bool IsExpired =>
        DateTimeOffset.UtcNow - SavedAt > Ttl;
}

