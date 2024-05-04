using System.Collections.Generic;

namespace TheArchive.Core.Bootstrap;

public class CachedAssembly<T> where T : ICacheable
{
    public List<T> CacheItems { get; set; }

    public string Hash { get; set; }
}
