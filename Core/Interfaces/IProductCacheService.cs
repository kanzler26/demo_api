using Core.DTO;
using Core.Models;

namespace Core.Interfaces;

public interface IProductCacheService
{
    public Task InvalidateAllProductsAsync(CancellationToken ct);
     public Task<List<ResponseProductDto>?> GetCachedProductsAsync(int page, int pageSize, CancellationToken ct);
     public Task SetCacheProductsAsync(List<ResponseProductDto> categoriesDto, int page, int pageSize, CancellationToken ct);


    public string GetCacheKey(int page, int pageSize);
}