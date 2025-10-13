using Core.DI.LiftetimeAttributes;
using Core.DTO;
using Core.Interfaces;
using Core.Models;
using MapsterMapper;
using Microsoft.Extensions.Logging;

namespace Application.Service;

[Scoped]
public class ProductCacheService(
    ILogger<ProductCacheService> logger,
    ICacheService<ResponseProductDto> cache,
    IMapper mapper,
    int cacheDuration = 10) : IProductCacheService
{
    private IMapper Mapper { get; } = mapper;
    private const string TrackerKey = "cache:category_keys";
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(cacheDuration);


    public string GetCacheKey(int page, int pageSize) =>
        $"products:page:{page}:size:{pageSize}";

    public async Task SetCacheProductsAsync(List<ResponseProductDto> item, int page, int pageSize, CancellationToken ct)
    {
        var key = GetCacheKey(page, pageSize);
        await cache.SetAsync(key, item, _cacheDuration, ct: ct);

        // Отслеживаем ключ
        var tracker = await cache.GetAsync<HashSet<string>>(TrackerKey, ct) ?? [];
        tracker.Add(key);
        await cache.SetAsync(TrackerKey, tracker, TimeSpan.FromDays(1), ct: ct);
    }

    public async Task<List<ResponseProductDto>?> GetCachedProductsAsync(int page, int pageSize, CancellationToken ct)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 1000);
        var key = GetCacheKey(page, pageSize);
        return await cache.GetAsync<List<ResponseProductDto>>(key, ct);
    }

    public async Task InvalidateAllProductsAsync(CancellationToken ct)
    {
        try
        {
            await cache.ClearAllTrackedKeys(TrackerKey, ct);
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
        }
    }
}