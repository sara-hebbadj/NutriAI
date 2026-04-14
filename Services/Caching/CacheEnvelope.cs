// Authorship note:
// This file was written with support from Copilot for the generic cache-wrapper structure.
// Microsoft Learn documentation was used for DateTimeOffset and TimeSpan concepts.
// The TTL-based expiry idea, use of SavedAt + Ttl metadata, and its role in NutriAI caching
// were selected and integrated by the author.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NutriAI.Services.Caching;

// This class wraps the cached data together with the metadata
// needed to know when it was saved and when it should expire.
public class CacheEnvelope<T>
{
    // The exact time when this data was stored in the cache.
    public DateTimeOffset SavedAt { get; set; }

    // TTL = Time To Live.
    // This tells the app how long the cached data can be reused
    // before it should be treated as outdated.
    public TimeSpan Ttl { get; set; }

    // The actual cached object, for example a list of recipes.
    public T Data { get; set; }

    // If the current time is later than SavedAt + Ttl,
    // the cache is considered expired.
    public bool IsExpired =>
        DateTimeOffset.UtcNow - SavedAt > Ttl;
}

