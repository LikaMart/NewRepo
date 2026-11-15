using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PerfLab.Data;
using PerfLab.Models;

namespace PerfLab;

public class CategoryService
{
    private const string CacheKey = "categories";
    private readonly AppDbContext _db;
    private readonly IMemoryCache _cache;

    public CategoryService(AppDbContext db, IMemoryCache cache)
    {
        _db = db;
        _cache = cache;
    }

    // 4.1 ქეში 5 წუთით
    public async Task<List<Category>> GetCategories()
    {
        var result = await _cache.GetOrCreateAsync(CacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            return await _db.Categories.AsNoTracking().OrderBy(c => c.Id).ToListAsync();
        });
        return result!;
    }

    // 4.2 ცვლის სახელს და ასუფთავებს ქეშს
    public async Task UpdateCategory(int id, string newName)
    {
        var category = await _db.Categories.FindAsync(id)
            ?? throw new InvalidOperationException($"კატეგორია {id} ვერ მოიძებნა");
        category.Name = newName;
        await _db.SaveChangesAsync();

        _cache.Remove(CacheKey);
    }

    // მხოლოდ საჩვენებლად: იგივე, ოღონდ ქეშს არ ეხება
    public async Task UpdateCategoryWithoutInvalidation(int id, string newName)
    {
        var category = await _db.Categories.FindAsync(id)
            ?? throw new InvalidOperationException($"კატეგორია {id} ვერ მოიძებნა");
        category.Name = newName;
        await _db.SaveChangesAsync();
    }
}
