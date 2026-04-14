using System.Data;
using System.Text.Json;
using Application.Service;
using Core.DTO;
using Core.Interfaces;
using Core.Models;
using Infrastructure.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyAppContext = Infrastructure.Context.MyAppContext;

namespace AlphaLising.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(
    IProductCacheService productCacheService,
    IProductService<CreateProductRequest, ResponseProductDto> productService,
    IConfiguration configuration
) : ControllerBase
{
    private IConfiguration Configuration { get; } = configuration;
    private readonly IProductService<CreateProductRequest, ResponseProductDto> _productService = productService;
    private IProductCacheService ProductCacheService { get; } = productCacheService;

    [HttpGet]
    public async Task<ActionResult<List<ResponseProductDto>>> GetProducts(CancellationToken ct, int page = 1,
        int pageSize = 100)
    {
        var cachedData = await ProductCacheService.GetCachedProductsAsync(page, pageSize, ct);
        if (cachedData != null)
        {
            return Ok(cachedData);
        }

        var productsDto = await _productService.GetProductsAsync(page, pageSize, ct);
        await ProductCacheService.SetCacheProductsAsync(productsDto, page, pageSize, ct);
        return Ok(productsDto);
    }

    [HttpGet]
    [Route("{id}")]
    public async Task<ActionResult<Product>> GetProduct(int id, CancellationToken ct)
    {
        var res = await _productService.GetProductByIdAsync(id, ct);
        return Ok(res);
    }

    [HttpPost]
    public async Task<ActionResult<Product>> Post(CreateProductRequest request, CancellationToken ct)
    {
        var responseDto = await _productService.CreateProductAsync(request, ct);
        await ProductCacheService.InvalidateAllProductsAsync(ct);
        return CreatedAtAction(nameof(GetProduct), new { responseDto!.Id },
            value: responseDto);
        ;
    }

    // [HttpPost("bulkproducts")]
    // public async Task<IActionResult> CreateBulksAsync(CancellationToken ct, int rows = 50_000)
    // {
    //     await using var connection = new SqlConnection(Configuration.GetConnectionString("DefaultConnection"));
    //     await connection.OpenAsync(ct); //  

    //     await using var transaction = connection.BeginTransaction(); // ← Опционально: транзакция
    //     using var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction);
    //     bulkCopy.DestinationTableName = "Products";

    //     // 🔥 Обязательно: добавить маппинг колонок!
    //     bulkCopy.ColumnMappings.Add("Name", "Name");
    //     bulkCopy.ColumnMappings.Add("Price", "Price");
    //     bulkCopy.ColumnMappings.Add("CategoryId", "CategoryId");

    //     var table = new DataTable();
    //     table.Columns.Add("Name", typeof(string));
    //     table.Columns.Add("Price", typeof(decimal));
    //     table.Columns.Add("CategoryId", typeof(int));

    //     for (int i = 0; i < rows; i++)
    //     {
    //         table.Rows.Add($"Product {i}", i * 10, i);
    //     }

    //     try
    //     {
    //         await bulkCopy.WriteToServerAsync(table, ct);
    //         await transaction.CommitAsync(ct);
    //         Console.WriteLine($"✅ Успешно вставлено {rows} записей");
    //     }
    //     catch (Exception ex)
    //     {
    //         await transaction.RollbackAsync(ct);
    //         Console.WriteLine($"❌ Ошибка: {ex.Message}");
    //         throw;
    //     }

    //     return Created();
    // }
}