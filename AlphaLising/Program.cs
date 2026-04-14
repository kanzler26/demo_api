using System.Diagnostics;
using AlphaLising.Extensions;
using Application.Mappings;
using Application.Service;
using AspNetCoreRateLimit;
using Core.DTO;
using Core.Interfaces;
using Core.Models;
using Infrastructure.Auth;
using Infrastructure.Redis;
using Infrastructure.Repositories;
using Infrastructure.Seeding;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Serilog;
using Serilog.Events;
using StackExchange.Redis;
using AppContext = Infrastructure.Context.MyAppContext;
using Category = Core.Models.Category;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOptions();
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(options =>
{
    options.EnableEndpointRateLimiting = true;
    options.StackBlockedRequests = false;
    options.GeneralRules =
    [
        new() { Endpoint = "*", Period = "1s", Limit = 5 } // макс 5 запросов в секунду с одного IP
    ];
});
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

builder.Services.AddHttpClient("siem", client =>
{
    client.BaseAddress = new Uri("https://siem.com");
    client.Timeout = Timeout.InfiniteTimeSpan; // Делегируем таймаут политике Polly
})
.AddResilienceHandler("siem-pipeline", builder => // ← builder — это ResiliencePipelineBuilder
 {
     builder.AddRetry(new RetryStrategyOptions<HttpResponseMessage>
     {
         MaxRetryAttempts = 3,
         Delay = TimeSpan.FromSeconds(2),
         BackoffType = DelayBackoffType.Exponential,
         // Defines which results/exceptions to retry
         ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
             .Handle<HttpRequestException>()
             .HandleResult(r => r.StatusCode == System.Net.HttpStatusCode.RequestTimeout)
     })
     .AddCircuitBreaker(new CircuitBreakerStrategyOptions<HttpResponseMessage>
     {
         FailureRatio = 0.5,
         SamplingDuration = TimeSpan.FromSeconds(10),
         BreakDuration = TimeSpan.FromSeconds(30)
     });
 });

MappingConfig.RegisterMappings();

builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddLogging();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDbContext<AppContext>(options => options
    .UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddConnections();
builder.Services.AddSwaggerGen(options =>
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AlphaLising API",
        Version = "v1",
        Description = "Api for AlphaLising for testing"
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

if (builder.Environment.IsDevelopment()) builder.Services.AddHostedService<DatabaseSeeder>();

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();
try
{
    Log.Information("🚀 Starting demo_api...");
    // Подключаем Serilog к ASP.NET Core
    builder.Host.UseSerilog();

    var app = builder.Build();
    // После app.UseRouting(), перед app.UseAuthorization()
    app.Use(async (context, next) =>
    {
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Append("X-Frame-Options", "DENY");
        context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
        await next();
    });
    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
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
    else
    {
        app.UseExceptionHandler("/error");
        app.UseHsts();
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
    app.UseRouting();
    app.UseIpRateLimiting();

    app.MapControllers();
    // В пайплайне, после app.UseRouting():
    app.Use(async (context, next) =>
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        var stopwatch = Stopwatch.StartNew();

        logger.LogInformation("📥 Request: {Method} {Path} from {IP}",
            context.Request.Method,
            context.Request.Path,
            context.Connection.RemoteIpAddress);

        await next();

        stopwatch.Stop();
        logger.LogInformation("📤 Response: {StatusCode} in {ElapsedMs}ms",
            context.Response.StatusCode,
            stopwatch.ElapsedMilliseconds);
    });

    // ... остальной пайплайн (UseAuthorization, MapControllers)

    Log.Information("✅ Application started, listening on {Urls}",
        Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? "default");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "❌ Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}