using System.Data;
using Application.Service;
using Core.DTO;
using Core.Interfaces;
using Core.Models;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Category = Core.Models.Category;

namespace AlphaLising.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController(
    ICategoryCacheService categoryCache,
    ICategoryService<CreateCategoryRequest, ResponseCategoryDto> categoryService,
    IConfiguration configuration
) : ControllerBase
{
    private readonly ICategoryService<CreateCategoryRequest, ResponseCategoryDto> _categoryService = categoryService;
    private ICategoryCacheService CategoryCache { get; } = categoryCache;
    private IConfiguration Configuration { get; } = configuration;

    [HttpGet]
    public async Task<ActionResult<List<ResponseCategoryDto>>> GetCategories(CancellationToken ct, int page = 1,
        int pageSize = 100)
    {
        var cachedData = await CategoryCache.GetCachedCategoriesAsync(page, pageSize, ct);
        if (cachedData != null)
        {
            return Ok(cachedData);
        }

        var categoriesDto = await _categoryService.GetCategoriesAsync(page, pageSize, ct);
        await CategoryCache.SetCacheCategoriesAsync(categoriesDto, page, pageSize, ct);
        return Ok(categoriesDto);
    }

    [HttpGet]
    [Route("{id}")]
    public async Task<ActionResult<Category>> GetCategory(int id, CancellationToken ct)
    {
        var res = await _categoryService.GetCategoryByIdAsync(id, ct);
        return Ok(res);
    }

    [HttpPost]
    public async Task<ActionResult<Category>> Post(CreateCategoryRequest request, CancellationToken ct)
    {
        var responseDto = await _categoryService.CreateCategoryAsync(request, ct);
        await CategoryCache.InvalidateAllCategoriesAsync(ct);
        return CreatedAtAction(nameof(GetCategory), new { responseDto.Id },
            value: responseDto);
        ;
    }

    // [HttpPost("bulkcatergories")]
    // public async Task<IActionResult> CreateBulksAsync(CancellationToken ct, int rows = 50_000)
    // {
    //     await using var connection = new SqlConnection(Configuration.GetConnectionString("DefaultConnection"));
    //     await connection.OpenAsync(); //  

    //     await using var transaction = connection.BeginTransaction(); // ← Опционально: транзакция
    //     using var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction);
    //     bulkCopy.DestinationTableName = "Categories";

    //     // 🔥 Обязательно: добавь маппинг колонок!
    //     bulkCopy.ColumnMappings.Add("Name", "Name");

    //     var table = new DataTable();
    //     table.Columns.Add("Name", typeof(string));


    //     for (int i = 0; i < rows; i++)
    //     {
    //         table.Rows.Add($"CategoryName {i}");
    //     }

    //     try
    //     {
    //         await bulkCopy.WriteToServerAsync(table);
    //         await transaction.CommitAsync(); // ← фиксируем
    //         Console.WriteLine($"✅ Успешно вставлено {rows} записей");
    //     }
    //     catch (Exception ex)
    //     {
    //         await transaction.RollbackAsync();
    //         Console.WriteLine($"❌ Ошибка: {ex.Message}");
    //         throw;
    //     }

    //     return Created();
    // }
}