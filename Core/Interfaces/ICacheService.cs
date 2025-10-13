namespace Core.Interfaces;

public interface ICacheService<K>
{
    Task<K?> GetAsync<K>(string key, CancellationToken ct = default);
    Task SetAsync<K>(string key, K value, TimeSpan? absoluteExpire = null, 
        TimeSpan? slidingExpire = null, CancellationToken ct = default);
    Task RemoveAsync(string key, CancellationToken ct = default);
    Task ClearAllTrackedKeys(string trackerKey, CancellationToken ct = default);
}