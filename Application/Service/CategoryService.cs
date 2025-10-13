using Core;
using Core.DTO;
using Core.Interfaces;
using Core.Models;
using MapsterMapper;
using Microsoft.Extensions.Logging;

namespace Application.Service;

public class CategoryService(
    ICategoryRepository<Category> repo,
    ILogger<CategoryService> logger,
    IMapper mapper
)
    : ICategoryService<CreateCategoryRequest, ResponseCategoryDto>
{
    private ICategoryRepository<Category> Repo { get; } = repo;
    private ILogger<CategoryService> Logger { get; } = logger;
    private IMapper Mapper { get; } = mapper;

    public async Task<ResponseCategoryDto> CreateCategoryAsync(CreateCategoryRequest request, CancellationToken ct)
    {
        await Repo.ThrowIfNameExistsAsync(request.Name, ct);
        var category = Mapper.Map<Category>(request);
        await Repo.SaveToDbAsync(category, ct);
        var responseDto = Mapper.Map<ResponseCategoryDto>(category);
        return responseDto;
    }

    public async Task<List<ResponseCategoryDto>> GetCategoriesAsync(int page, int size, CancellationToken ct)
    {
        var categories = await Repo.GetPaginetedCategoriesAsync(page, size, ct);
        var categoriesDto = Mapper.Map<List<ResponseCategoryDto>>(categories);
        return categoriesDto;
    }

    public async Task<ResponseCategoryDto> GetCategoryByIdAsync(int id, CancellationToken ct)
    {
        var category = await Repo.GetByIdAsync(id, ct);
        var categoryDto = Mapper.Map<ResponseCategoryDto>(category);
        return categoryDto;
    }

    public async Task DeleteAsync(int id)
    {
        await Repo.DeleteFromDbAsync(id);
    }
}