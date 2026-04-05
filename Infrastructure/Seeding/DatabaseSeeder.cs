using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Context; // 
using Core.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Infrastructure.Seeding;

public class DatabaseSeeder : IHostedService
{

    private readonly ILogger<DatabaseSeeder> _logger;
    private readonly IServiceProvider _serviceProvider;

    public DatabaseSeeder(ILogger<DatabaseSeeder> logger, IServiceProvider serviceProvider)
    => (_logger, _serviceProvider) = (logger, serviceProvider);


    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scoped = _serviceProvider.CreateScope();
        var context = scoped.ServiceProvider.GetRequiredService<MyAppContext>();
        await context.Database.MigrateAsync(cancellationToken: cancellationToken);
        try
        {
            _logger.LogInformation("Check any data before Seeding database");
            if (!await context.Categories.AnyAsync(cancellationToken))
            {
                await SeedingData(context, cancellationToken);
                _logger.LogInformation("Database seeded is successful");
            }
            else
            {
                _logger.LogInformation("Database already seeded");
            }

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Seeding fail");

        }
    }

    private static async Task SeedingData(MyAppContext context, CancellationToken cancellationToken)
    {
        await using var transaction = context.Database.BeginTransaction();
        try
        {
            List<Category> categories = [..Enumerable.Range(1, 1000).Select(i => new Category
                    {
                        Name = $"Category {i}",
                        Products = [..Enumerable.Range(1, 5).Select(i => new Product
                        {
                         Name = $"Product {i}",
                        })]
                    })];
            await context.Categories.AddRangeAsync(categories, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
