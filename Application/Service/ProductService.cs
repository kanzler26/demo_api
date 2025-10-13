using Core.DTO;
using Core.Interfaces;
using Core.Models;
using MapsterMapper;
using Microsoft.Extensions.Logging;

namespace Application.Service;

public class ProductService(IProductRepository<Product> repo, IMapper mapper, ILogger<ProductService> logger)
    : IProductService<CreateProductRequest, ResponseProductDto>
{
    private IProductRepository<Product> Repo { get; } = repo;
    private IMapper Mapper { get; } = mapper;
    private ILogger<ProductService> Logger { get; } = logger;

    public async Task<ResponseProductDto?> CreateProductAsync(CreateProductRequest request, CancellationToken ct)
    {
        await Repo.ThrowIfNameExistsAsync(request.Name, ct);
        await Repo.ThrowIfCategoryIdIsNotExistsAsync(request.CategoryId, ct);
        var product = Mapper.Map<Product>(request);
        await Repo.SaveProductAsync(product, ct);
        var responseDto = Mapper.Map<ResponseProductDto>(product);
        return responseDto;
    }

    public async Task<ResponseProductDto?> GetProductByIdAsync(int id, CancellationToken ct)
    {
        var product = await Repo.GetProductByIdAsync(id, ct);
        var productDto = Mapper.Map<ResponseProductDto>(product);
        return productDto;
    }

    public async Task DeleteProductAsync(int id)
    {
        await Repo.DeleteProductAsync(id);
    }


    public async Task<List<ResponseProductDto>> GetProductsAsync(int page, int size, CancellationToken ct)
    {
        var products = await Repo.GetPaginetedProductsAsync(page, size, ct);
        var productsDto = mapper.Map<List<ResponseProductDto>>(products);
        return productsDto;
    }
}