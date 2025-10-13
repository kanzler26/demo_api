using AlphaLising.Extensions;
using Application.Mappings;
using Application.Service;
using Core.DTO;
using Core.Interfaces;
using Core.Models;
using Infrastructure.Auth;
using Infrastructure.Context;
using Infrastructure.Redis;
using Infrastructure.Repositories;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using AppContext = Infrastructure.Context.MyAppContext;
using Category = Core.Models.Category;

var builder = WebApplication.CreateBuilder(args);
MappingConfig.RegisterMappings();

builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddLogging();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDbContext<AppContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
        .LogTo(Console.WriteLine, LogLevel.Information));
builder.Services.AddConnections();
builder.Services.AddSwaggerGen(options =>
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AlphaLising API", Version = "v1", Description = "Api for AlphaLising for testing"
    })
);
builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
{
    var configuration = builder.Configuration.GetConnectionString("Redis");
    return ConnectionMultiplexer.Connect(configuration ?? throw new InvalidOperationException());
});
// builder.Services.AddServices();
builder.Services.AddScoped<ICacheService<ResponseCategoryDto>, RedisCacheService<ResponseCategoryDto>>();
builder.Services.AddScoped<ICacheService<ResponseProductDto>, RedisCacheService<ResponseProductDto>>();

builder.Services.AddScoped<ICategoryRepository<Category>, CategoryRepository>();
builder.Services.AddScoped<ICategoryCacheService, CategoryCacheService>();

builder.Services.AddScoped<ICategoryService<CreateCategoryRequest, ResponseCategoryDto>, CategoryService>();

builder.Services.AddScoped<IProductRepository<Product>, ProductRepository>();
builder.Services.AddScoped<IProductCacheService, ProductCacheService>();
builder.Services.AddScoped<IProductService<CreateProductRequest, ResponseProductDto>, ProductService>();

builder.Services.AddMapster();
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // 
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<MyAppContext>();
    context.Database.Migrate(); // применяет миграции
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        c.RoutePrefix = "swagger"; // по умолчанию доступен по /swagger
    });
    app.UseCors(policy => policy
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());
}

//переделать все dto на record
//реализовать самописный перехватчик для ошибок
//использовать подключенный Polly
//проверить слои на протечки
// добавить api gateway
//добавить RateLimiting
 app.UseCustomExceptionHandler();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();