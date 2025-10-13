using Core;
using Core.Interfaces;
using Core.Models;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class CategoryRepository(MyAppContext context) : ICategoryRepository<Category>
{
    private MyAppContext Context { get; set; } = context;

    public async Task<int> SaveToDbAsync(Category category, CancellationToken ct = default)
    {
        await Context.AddAsync(category, ct);
        await Context.SaveChangesAsync(cancellationToken: ct);
        return category.Id;
    }

    public async Task<Category> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var category = await Context.Categories.FindAsync(id,ct);
        if (category == null)
        {
            throw new NotFoundException("Category", id);
        }

        return category;
    }


    public async Task DeleteFromDbAsync(int id, CancellationToken ct = default )
    {
        var category = await Context.Categories.FindAsync(id,ct);
        if (category == null)
        {
            throw new NotFoundException("Category", id);
        }

        Context.Remove(category);
        await Context.SaveChangesAsync(ct);
    }

    public async Task<List<Category>> GetPaginetedCategoriesAsync(int page, int size, CancellationToken ct = default)
    {
         var res = await Context.Categories.AsNoTracking().Skip((page - 1) * size).Take(size).ToListAsync(ct);
        return res;
    }

    public async Task ThrowIfNameExistsAsync(string name, CancellationToken ct = default)
    {
        if (await Context.Categories.AsNoTracking().AnyAsync(c => c.Name == name, ct))
        {
            throw new BusinessException($"Category {name} already exists");
        }
    }
}