using Microsoft.Extensions.Caching.Memory;

public interface ICacheService
{
    bool TryGetValue<TItem>(object key, out TItem value);
    void Set<TItem>(object key, TItem value, MemoryCacheEntryOptions options);
    void Remove(object key);
}