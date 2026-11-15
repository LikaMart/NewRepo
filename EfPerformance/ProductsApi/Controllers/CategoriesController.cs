using Microsoft.AspNetCore.Mvc;
using ProductsApi.Models;
using ProductsApi.Services;

namespace ProductsApi.Controllers;

[ApiController]
[Route("categories")]
public class CategoriesController : ControllerBase
{
    private readonly CategoryCache _categories;

    public CategoriesController(CategoryCache categories)
    {
        _categories = categories;
    }

    // GET /categories (IMemoryCache, 5 წუთი)
    [HttpGet]
    public Task<List<CategoryDto>> Get() => _categories.GetAll();

    // PUT /categories/2?name=Books  სახელს ცვლის და ქეშს ასუფთავებს
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Rename(int id, string name)
    {
        return await _categories.Rename(id, name) ? NoContent() : NotFound();
    }
}
