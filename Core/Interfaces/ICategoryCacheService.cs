using Core.DTO;
using Core.Models;

namespace Core.Interfaces;

public interface ICategoryCacheService
{
    public Task InvalidateAllCategoriesAsync(CancellationToken ct);
    public Task<List<ResponseCategoryDto>?> GetCachedCategoriesAsync(int page, int pageSize, CancellationToken ct);
    public Task SetCacheCategoriesAsync(List<ResponseCategoryDto> categoriesDto, int page, int pageSize, CancellationToken ct);

    public string GetCacheKey(int page, int pageSize);
}