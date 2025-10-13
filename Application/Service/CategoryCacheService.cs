using Core.DTO;
using Core.Interfaces;
using Core.Models;
using MapsterMapper;
using Microsoft.Extensions.Logging;

namespace Application.Service;

public class CategoryCacheService(
    ILogger<CategoryCacheService> logger,
    ICacheService<ResponseCategoryDto> cache,
    IMapper mapper,
    int cacheDuration = 10
) : ICategoryCacheService
{
    private IMapper Mapper { get; } = mapper;
    private const string TrackerKey = "cache:category_keys";
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(cacheDuration);

    public string GetCacheKey(int page, int pageSize) =>
        $"categories:page:{page}:size:{pageSize}";

    public async Task SetCacheCategoriesAsync(List<ResponseCategoryDto> categoriesDto, int page, int pageSize,
        CancellationToken ct)
    {
        var key = GetCacheKey(page, pageSize);
        var categories = Mapper.Map<List<Category>>(categoriesDto);
        await cache.SetAsync(key, categories, _cacheDuration, ct: ct);

        // Отслеживаем ключ
        var tracker = await cache.GetAsync<HashSet<string>>(TrackerKey, ct) ?? [];
        tracker.Add(key);
        await cache.SetAsync(TrackerKey, tracker, TimeSpan.FromDays(1), ct: ct);
    }

    public async Task<List<ResponseCategoryDto>?> GetCachedCategoriesAsync(int page, int pageSize, CancellationToken ct)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 1000);
        var key = GetCacheKey(page, pageSize);
        return await cache.GetAsync<List<ResponseCategoryDto>>(key, ct);
    }

    public async Task InvalidateAllCategoriesAsync(CancellationToken ct)
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