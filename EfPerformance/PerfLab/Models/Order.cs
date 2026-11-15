namespace PerfLab.Models;

// 2.5 ამოცანისთვის: შეკვეთას აქვს ორი კოლექცია
public class Order
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<OrderItem> Items { get; set; } = new();
    public List<Comment> Comments { get; set; } = new();
}

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string ProductName { get; set; } = null!;
    public int Quantity { get; set; }
}

public class Comment
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string Text { get; set; } = null!;
}
