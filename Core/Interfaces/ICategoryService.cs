using Core.DI;

namespace Core.Interfaces;

public interface ICategoryService<in C, R>
    where C : class
    where R : class
{
    public Task DeleteAsync(int id);
    public Task<R> GetCategoryByIdAsync(int id, CancellationToken cancellationToken);
    public Task<R> CreateCategoryAsync(C createDto, CancellationToken ct);
    public Task<List<R>> GetCategoriesAsync(int page, int size, CancellationToken ct);
}