using Core.DI;
using Core.DI.LiftetimeAttributes;

namespace Core.Interfaces;

[Scoped]
public interface IProductService<C,R> 
    where C : class
    where R : class
{
    public Task DeleteProductAsync(int id);
    public Task<R?> GetProductByIdAsync(int id, CancellationToken cancellationToken);
    public Task<R?> CreateProductAsync(C createDto, CancellationToken ct);
    public Task<List<R>> GetProductsAsync(int page, int size, CancellationToken ct);
}