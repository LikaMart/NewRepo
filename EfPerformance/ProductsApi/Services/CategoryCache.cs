using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using ProductsApi.Data;
using ProductsApi.Models;

namespace ProductsApi.Services;

public class CategoryCache
{
    public const string CacheKey = "categories";
    private readonly AppDbContext _db;
    private readonly IMemoryCache _cache;

    public CategoryCache(AppDbContext db, IMemoryCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<List<CategoryDto>> GetAll()
    {
        var list = await _cache.GetOrCreateAsync(CacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            return await _db.Categories
                .AsNoTracking()
                .OrderBy(c => c.Id)
                .Select(c => new CategoryDto(c.Id, c.Name))
                .ToListAsync();
        });
        return list!;
    }

    public async Task<bool> Rename(int id, string name)
    {
        int updated = await _db.Categories
            .Where(c => c.Id == id)
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.Name, name));

        _cache.Remove(CacheKey);
        return updated > 0;
    }
}
