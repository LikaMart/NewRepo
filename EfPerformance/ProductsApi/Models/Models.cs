namespace ProductsApi.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
}

// რასაც API აბრუნებს. Description აქ საერთოდ არ გვჭირდება.
public record ProductDto(int Id, string Name, decimal Price, string CategoryName);

public record CategoryDto(int Id, string Name);

public record PagedResult<T>(int Page, int Size, int Total, List<T> Items);

public record KeysetResult<T>(int? NextLastId, List<T> Items);
