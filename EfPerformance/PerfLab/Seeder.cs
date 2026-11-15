using Microsoft.EntityFrameworkCore;
using PerfLab.Data;
using PerfLab.Models;

namespace PerfLab;

public static class Seeder
{
    public static async Task SeedAsync()
    {
        using var db = Db.Create();
        await db.Database.MigrateAsync();

        if (!await db.Categories.AnyAsync())
        {
            db.Categories.AddRange(
                new Category { Name = "Electronics" },
                new Category { Name = "Books" },
                new Category { Name = "Clothes" },
                new Category { Name = "Sports" },
                new Category { Name = "Home" });
            await db.SaveChangesAsync();
        }

        if (!await db.Products.AnyAsync())
        {
            Console.WriteLine("ვამატებ 100 000 პროდუქტს...");
            for (int i = 0; i < 100; i++)
            {
                var batch = Enumerable.Range(1, 1000).Select(n => new Product
                {
                    Name = $"Product {i * 1000 + n}",
                    Description = new string('x', 200),
                    Price = Random.Shared.Next(1, 2000),
                    CategoryId = Random.Shared.Next(1, 6)
                });
                db.Products.AddRange(batch);
                await db.SaveChangesAsync();
                db.ChangeTracker.Clear();
            }
        }

        // 2.5-ისთვის: 200 შეკვეთა, თითოში 10 პოზიცია და 10 კომენტარი
        if (!await db.Orders.AnyAsync())
        {
            for (int o = 1; o <= 200; o++)
            {
                var order = new Order { CreatedAt = DateTime.Now.AddDays(-o) };
                for (int k = 1; k <= 10; k++)
                {
                    order.Items.Add(new OrderItem { ProductName = $"Product {o * 10 + k}", Quantity = k });
                    order.Comments.Add(new Comment { Text = $"Comment {k} for order {o}" });
                }
                db.Orders.Add(order);
            }
            await db.SaveChangesAsync();
        }

        Console.WriteLine($"ბაზაში: {await db.Products.CountAsync()} პროდუქტი, {await db.Categories.CountAsync()} კატეგორია");
    }
}
