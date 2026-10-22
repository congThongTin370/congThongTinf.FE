using Microsoft.Extensions.Caching.Memory;

namespace congThongTinf.FE.Utilities;

/// <summary>
/// Cache đọc công khai: trong thời gian tươi thì không gọi API (chống spam F5),
/// hết hạn tươi thì gọi lại; API lỗi thì dùng bản cũ còn trong cache.
/// </summary>
public static class PublicReadCache
{
    private static readonly TimeSpan FreshFor = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan KeepFor = TimeSpan.FromMinutes(10);

    public static bool TryGetFresh<T>(IMemoryCache cache, string key, out T? value)
    {
        if (cache.TryGetValue(key, out Entry<T>? entry) && entry != null)
        {
            value = entry.Value;
            return DateTime.UtcNow < entry.FreshUntil;
        }

        value = default;
        return false;
    }

    public static bool TryGetStale<T>(IMemoryCache cache, string key, out T? value)
    {
        if (cache.TryGetValue(key, out Entry<T>? entry) && entry != null)
        {
            value = entry.Value;
            return true;
        }

        value = default;
        return false;
    }

    public static void Set<T>(IMemoryCache cache, string key, T value)
    {
        cache.Set(
            key,
            new Entry<T> { Value = value, FreshUntil = DateTime.UtcNow.Add(FreshFor) },
            KeepFor);
    }

    private sealed class Entry<T>
    {
        public required T Value { get; init; }
        public DateTime FreshUntil { get; init; }
    }
}
