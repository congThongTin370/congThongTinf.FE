using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Caching.Distributed;

namespace congThongTinf.FE.Utilities;

/// <summary>
/// IDistributedCache lưu trên ổ đĩa để session không mất khi restart ứng dụng.
/// Mỗi key là một file (tên là SHA-256 của key). File gồm 8 byte thời điểm hết hạn (UTC ticks)
/// theo sau là dữ liệu. Chỉ dùng cho môi trường MVP/local, một tiến trình.
/// </summary>
public sealed class FileDistributedCache : IDistributedCache
{
    private const int HeaderLength = sizeof(long);
    private static readonly TimeSpan DefaultSlidingExpiration = TimeSpan.FromMinutes(30);

    private readonly string _directory;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public FileDistributedCache(string directory)
    {
        _directory = directory;
        Directory.CreateDirectory(_directory);
    }

    public byte[]? Get(string key)
    {
        return GetAsync(key).GetAwaiter().GetResult();
    }

    public async Task<byte[]?> GetAsync(string key, CancellationToken token = default)
    {
        var path = PathFor(key);
        await _lock.WaitAsync(token);
        try
        {
            if (!File.Exists(path))
            {
                return null;
            }

            var bytes = await File.ReadAllBytesAsync(path, token);
            if (bytes.Length < HeaderLength)
            {
                File.Delete(path);
                return null;
            }

            var expiresAt = new DateTime(BitConverter.ToInt64(bytes, 0), DateTimeKind.Utc);
            if (expiresAt <= DateTime.UtcNow)
            {
                File.Delete(path);
                return null;
            }

            return bytes[HeaderLength..];
        }
        finally
        {
            _lock.Release();
        }
    }

    public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
    {
        SetAsync(key, value, options).GetAwaiter().GetResult();
    }

    public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
    {
        var expiresAt = ResolveExpiration(options);
        var payload = new byte[HeaderLength + value.Length];
        BitConverter.GetBytes(expiresAt.Ticks).CopyTo(payload, 0);
        value.CopyTo(payload, HeaderLength);

        var path = PathFor(key);
        var tempPath = path + ".tmp";

        await _lock.WaitAsync(token);
        try
        {
            // Ghi ra file tạm rồi đổi tên để tránh đọc phải file ghi dở
            await File.WriteAllBytesAsync(tempPath, payload, token);
            File.Move(tempPath, path, overwrite: true);
        }
        finally
        {
            _lock.Release();
        }
    }

    public void Refresh(string key)
    {
        RefreshAsync(key).GetAwaiter().GetResult();
    }

    public async Task RefreshAsync(string key, CancellationToken token = default)
    {
        // Sliding expiration: đọc rồi ghi lại với thời hạn mới
        var value = await GetAsync(key, token);
        if (value != null)
        {
            await SetAsync(key, value, new DistributedCacheEntryOptions { SlidingExpiration = DefaultSlidingExpiration }, token);
        }
    }

    public void Remove(string key)
    {
        RemoveAsync(key).GetAwaiter().GetResult();
    }

    public async Task RemoveAsync(string key, CancellationToken token = default)
    {
        var path = PathFor(key);
        await _lock.WaitAsync(token);
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        finally
        {
            _lock.Release();
        }
    }

    private string PathFor(string key)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(key));
        return Path.Combine(_directory, Convert.ToHexString(hash) + ".bin");
    }

    private static DateTime ResolveExpiration(DistributedCacheEntryOptions options)
    {
        var now = DateTime.UtcNow;
        if (options.AbsoluteExpiration.HasValue)
        {
            return options.AbsoluteExpiration.Value.UtcDateTime;
        }
        if (options.AbsoluteExpirationRelativeToNow.HasValue)
        {
            return now.Add(options.AbsoluteExpirationRelativeToNow.Value);
        }
        if (options.SlidingExpiration.HasValue)
        {
            return now.Add(options.SlidingExpiration.Value);
        }
        return now.Add(DefaultSlidingExpiration);
    }
}
