using System.Text.Json;
using Core.Interfaces;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Infrastructure.Redis;

public class RedisCacheService<TU>(IConnectionMultiplexer redis, ILogger<TU> logger) : ICacheService<TU>
{
    private IDatabase Database => redis.GetDatabase();


    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        try
        {
            var cached = await Database.StringGetAsync(key);
            return !cached.IsNullOrEmpty
                ? JsonSerializer.Deserialize<T>(cached!)
                : default;
        }
        catch (RedisException e)
        {
            logger.LogError($"Redis GET error for key {key}: {e.Message}");
            return default;
        }
    }


    public async Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpire = null,
        TimeSpan? slidingExpire = null, CancellationToken ct = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(value);
            var expire = absoluteExpire ?? slidingExpire;
            await Database.StringSetAsync(key, json, expire);
        }
        catch (RedisException e)
        {
            logger.LogError($"Redis SET error for key {key}: {e.Message}");
        }
    }

    public async Task RemoveAsync(string key, CancellationToken ct = default)
    {
        try
        {
            await Database.KeyDeleteAsync(key);
        }
        catch (RedisException ex)
        {
            Console.WriteLine($"Redis DELETE error for key {key}: {ex.Message}");
        }
    }

    public async Task ClearAllTrackedKeys(string trackerKey, CancellationToken ct = default)
    {
        try
        {
            var trackedKeysJson = await Database.StringGetAsync(trackerKey);
            if (!trackedKeysJson.IsNullOrEmpty)
            {
                var keys = JsonSerializer.Deserialize<HashSet<string>>(trackedKeysJson!);
                if (keys != null)
                {
                    foreach (var key in keys)
                    {
                        await Database.KeyDeleteAsync(key);
                    }
                }
            }

            await Database.KeyDeleteAsync(trackerKey); // удалить сам трекер
        }
        catch (RedisException ex)
        {
            Console.WriteLine($"Redis CLEAR tracked keys error: {ex.Message}");
        }
    }
}