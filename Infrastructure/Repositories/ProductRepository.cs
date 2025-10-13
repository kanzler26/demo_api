using Core;
using Core.DTO;
using Core.Interfaces;
using Core.Models;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using MyAppContext = Infrastructure.Context.MyAppContext;

namespace Infrastructure.Repositories;

public class ProductRepository(MyAppContext context) : IProductRepository<Product>
{
    private MyAppContext Context { get; set; } = context;

    public async Task<int> SaveProductAsync(Product responseProduct, CancellationToken ct = default)
    {
        await Context.AddAsync(responseProduct, ct);
        await Context.SaveChangesAsync(cancellationToken: ct);
        return responseProduct.Id;
    }


    public async Task<Product?> GetProductByIdAsync(int id, CancellationToken ct = default)
    {
        var product = await Context.Products.FindAsync(id, ct);
        if (product == null)
        {
            throw new NotFoundException("Product", id);
        }

        return product;
    }

    public async Task<List<Product>> GetPaginetedProductsAsync(int page, int size, CancellationToken ct = default)
    {
        return await Context.Products.AsNoTracking().Skip((page - 1) * size).Take(size).ToListAsync(ct);
    }

    public async Task DeleteProductAsync(int id, CancellationToken ct = default)
    {
        var product = await Context.FindAsync<ResponseProductDto>(id, ct);
        if (product != null)
        {
            Context.Remove(product);
            await Context.SaveChangesAsync(ct);
        }
    }

    public async Task ThrowIfNameExistsAsync(string name, CancellationToken ct = default)
    {
        if (await Context.Products.AsNoTracking().AnyAsync(c => c.Name == name, ct))
        {
            throw new BusinessException($"Product {name} already exists");
        }
    }

    public async Task ThrowIfCategoryIdIsNotExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        if (await Context.Categories.FirstOrDefaultAsync(c => c.Id == id, cancellationToken) == null)
        {
            throw new NotFoundException($"Category {id} isn't exists");
        }
    }
}