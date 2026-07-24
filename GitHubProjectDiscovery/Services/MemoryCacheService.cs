namespace GitHubProjectDiscovery.Services;

public class MemoryCacheService
{
    private readonly Dictionary<string, CacheEntry> _cache = new(StringComparer.OrdinalIgnoreCase);

    public bool TryGet(string key, out string value)
    {
        value = string.Empty;
        if (!_cache.TryGetValue(key, out CacheEntry? entry)) return false;
        if (entry.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            _cache.Remove(key);
            return false;
        }
        value = entry.Value;
        return true;
    }

    public void Set(string key, string value, TimeSpan duration) =>
        _cache[key] = new CacheEntry(value, DateTimeOffset.UtcNow.Add(duration));

    private record CacheEntry(string Value, DateTimeOffset ExpiresAt);
}
