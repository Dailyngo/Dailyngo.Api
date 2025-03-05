using EveryDaily.Core;
using Microsoft.Extensions.Caching.Memory;

namespace EveryDaily.Application.Services.Cache;

public interface ICacheService
{
    Task<string?> GetAsync(string key);
    Task SetAsync(string key, string value, TimeSpan? expiration = null);
    Task DeleteAsync(string key);
    Task<bool> ExistsAsync(string key);
}


public class CacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly IRedisService _redisService;

    public CacheService(IMemoryCache memoryCache, IRedisService redisService)
    {
        _memoryCache = memoryCache;
        _redisService = redisService;
    }

    public async Task<string?> GetAsync(string key)
    {
        if (_memoryCache.TryGetValue(key, out string value))
        {
            return value;
        }

        var redisValue = await _redisService.GetStringAsync(key);
        if (redisValue != null)
        {
            _memoryCache.Set(key, redisValue, TimeSpan.FromMinutes(10)); // 10 dakika süreyle cache'liyoruz
            return redisValue;
        }

        return null;
    }

    public async Task SetAsync(string key, string value, TimeSpan? expiration = null)
    {
        _memoryCache.Set(key, value, expiration ?? TimeSpan.FromMinutes(10));

        await _redisService.SetStringAsync(key, value, expiration ?? TimeSpan.FromMinutes(10));
    }

    public async Task DeleteAsync(string key)
    {
        _memoryCache.Remove(key);

        await _redisService.DeleteAsync(key);
    }
    public async Task<bool> ExistsAsync(string key)
    {
        if (_memoryCache.TryGetValue(key, out _))
        {
            return true;
        }

        return await _redisService.ExistsAsync(key);
    }
}
