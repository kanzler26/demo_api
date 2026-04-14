using Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Context;

public class MyAppContext : DbContext
{
    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }

    public MyAppContext(DbContextOptions<MyAppContext> options) : base(options)
    {
    }

    // сложная конфигурация моделей вызывается один раз на контекст и кешируется
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Привести все имена таблиц к нижнему регистру (опционально, для совместимости)
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            entity.SetTableName(entity.GetTableName()?.ToLowerInvariant());
        }

        modelBuilder.Entity<Product>().HasOne(p => p.Category)
            .WithMany(c => c.Products).HasForeignKey(p => p.CategoryId);
        modelBuilder.Entity<Product>()
            .Property(p => p.Price)
            .HasPrecision(18, 2); // например: 9999999999999999.99
    }
}