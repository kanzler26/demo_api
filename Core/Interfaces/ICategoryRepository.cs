namespace Core.Interfaces;

public interface ICategoryRepository<T>
{
    public Task<int> SaveToDbAsync(T category, CancellationToken cancellationToken = default);
    public Task<T> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    public Task DeleteFromDbAsync(int id, CancellationToken cancellationToken = default);
    public Task<List<T>> GetPaginetedCategoriesAsync(int page, int size, CancellationToken cancellationToken = default);
    public Task ThrowIfNameExistsAsync(string name, CancellationToken cancellationToken = default);
}