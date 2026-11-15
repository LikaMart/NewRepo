using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductsApi.Data;
using ProductsApi.Models;

namespace ProductsApi.Controllers;

[ApiController]
[Route("products")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _db;

    public ProductsController(AppDbContext db)
    {
        _db = db;
    }

    // GET /products?page=1&size=20&search=Product 99
    // ოპტიმიზებული ვერსია: AsNoTracking, DTO-ზე Select, Skip/Take,
    // ძებნა StartsWith-ით, რომ Name-ის ინდექსი გამოიყენოს
    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductDto>>> Get(
        int page = 1, int size = 20, string? search = null)
    {
        page = Math.Max(page, 1);
        size = Math.Clamp(size, 1, 100);

        var query = _db.Products.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.Name.StartsWith(search));

        int total = await query.CountAsync();

        var items = await query
            .OrderBy(p => p.Id)
            .Skip((page - 1) * size)
            .Take(size)
            .Select(p => new ProductDto(p.Id, p.Name, p.Price, p.Category.Name))
            .ToListAsync();

        return new PagedResult<ProductDto>(page, size, total, items);
    }

    // GET /products/keyset?lastId=0&size=20&search=...
    // შემდეგი გვერდი Skip-ის გარეშე. პასუხში მოდის nextLastId, რომელიც
    // შემდეგ მოთხოვნაში უნდა გადაეცეს
    [HttpGet("keyset")]
    public async Task<ActionResult<KeysetResult<ProductDto>>> GetKeyset(
        int lastId = 0, int size = 20, string? search = null)
    {
        size = Math.Clamp(size, 1, 100);

        var query = _db.Products.AsNoTracking().Where(p => p.Id > lastId);
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.Name.StartsWith(search));

        var items = await query
            .OrderBy(p => p.Id)
            .Take(size)
            .Select(p => new ProductDto(p.Id, p.Name, p.Price, p.Category.Name))
            .ToListAsync();

        int? next = items.Count == size ? items[^1].Id : null;
        return new KeysetResult<ProductDto>(next, items);
    }

    // GET /products/naive?page=1&size=20&search=...
    // ოპტიმიზაციამდე ვერსია, მხოლოდ შედარებისთვის:
    // ყველაფერი იტვირთება ტრექინგით და ფილტრი/გვერდი მეხსიერებაში კეთდება
    [HttpGet("naive")]
    public ActionResult<PagedResult<ProductDto>> GetNaive(
        int page = 1, int size = 20, string? search = null)
    {
        page = Math.Max(page, 1);
        size = Math.Clamp(size, 1, 100);

        var all = _db.Products.Include(p => p.Category).ToList();
        if (!string.IsNullOrWhiteSpace(search))
            all = all.Where(p => p.Name.StartsWith(search)).ToList();

        var items = all
            .OrderBy(p => p.Id)
            .Skip((page - 1) * size)
            .Take(size)
            .Select(p => new ProductDto(p.Id, p.Name, p.Price, p.Category.Name))
            .ToList();

        return new PagedResult<ProductDto>(page, size, all.Count, items);
    }
}
