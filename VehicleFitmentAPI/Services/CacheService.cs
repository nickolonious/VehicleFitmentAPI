using Microsoft.Extensions.Caching.Memory;

public class CacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;

    public CacheService(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public bool TryGetValue<TItem>(object key, out TItem value)
    {
        return _memoryCache.TryGetValue(key, out value);
    }

    public void Set<TItem>(object key, TItem value, MemoryCacheEntryOptions options)
    {
        _memoryCache.Set(key, value, options);
    }

    public void Remove(object key)
    {
        _memoryCache.Remove(key);
    }
}