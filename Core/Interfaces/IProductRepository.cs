namespace Core.Interfaces;

public interface IProductRepository<T>
{
    public Task<List<T>> GetPaginetedProductsAsync(int page, int size, CancellationToken ct = default);

    public Task<int> SaveProductAsync(T product, CancellationToken cancellationToken = default);
    public Task<T?> GetProductByIdAsync(int id, CancellationToken cancellationToken = default);
    public Task DeleteProductAsync(int id, CancellationToken cancellationToken = default);
    public Task ThrowIfNameExistsAsync(string name, CancellationToken cancellationToken = default);
    public Task ThrowIfCategoryIdIsNotExistsAsync(int id, CancellationToken cancellationToken = default);

}