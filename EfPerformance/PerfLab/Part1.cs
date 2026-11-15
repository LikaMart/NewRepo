using Microsoft.EntityFrameworkCore;
using PerfLab.Data;

namespace PerfLab;

public static class Part1
{
    public static async Task RunAsync()
    {
        Console.WriteLine("\n===== ნაწილი 1 =====");

        // ---------- 1.1 SQL-ის ნახვა ----------
        Console.WriteLine("\n--- 1.1 ToQueryString (მოთხოვნა არ სრულდება) ---");
        using (var db = Db.CreateWithLog())
        {
            var query = db.Products.Where(p => p.Price > 1000);
            Console.WriteLine(query.ToQueryString());

            Console.WriteLine("\n--- 1.1 LogTo (ლოგი ჩნდება მხოლოდ შესრულებისას) ---");
            var list = await query.AsNoTracking().ToListAsync();
            Console.WriteLine($"მოიძებნა {list.Count} პროდუქტი");
        }

        // ---------- 1.2 Select * vs Projection ----------
        Console.WriteLine("\n--- 1.2 ---");
        await Bench.Measure("1.2 (ა) ყველა სვეტი", async () =>
        {
            using var db = Db.Create();
            var all = await db.Products.AsNoTracking().ToListAsync();
        });
        await Bench.Measure("1.2 (ბ) მხოლოდ Name, Price", async () =>
        {
            using var db = Db.Create();
            var small = await db.Products.AsNoTracking()
                .Select(p => new { p.Name, p.Price })
                .ToListAsync();
        });

        // ---------- 1.3 შეცდომები ----------
        Console.WriteLine("\n--- 1.3 ---");
        int badQueries = 0;
        int badPrinted = 0;
        await Bench.Measure("1.3 თავდაპირველი კოდი", async () =>
        {
            using var db = Db.CreateWithCounter(() => badQueries++);

            var all = db.Products.ToList();
            var expensive = all.Where(p => p.Price > 1000).ToList();
            if (expensive.Count() > 0)
            {
                foreach (var p in expensive)
                {
                    var cat = db.Categories.First(c => c.Id == p.CategoryId);
                    // Console.WriteLine($"{p.Name} - {cat.Name}");
                    // 50 000 ხაზის ბეჭდვა თვითონ წაიღებს ბევრ დროს, ამიტომ მხოლოდ ვითვლი
                    badPrinted++;
                }
            }
        }, runs: 1);
        Console.WriteLine($"  SQL ბრძანებები: {badQueries}, ხაზები: {badPrinted}");

        int goodQueries = 0;
        int goodPrinted = 0;
        await Bench.Measure("1.3 გასწორებული კოდი", async () =>
        {
            goodQueries = 0;
            goodPrinted = 0;
            using var db = Db.CreateWithCounter(() => goodQueries++);

            var expensive = await db.Products
                .AsNoTracking()
                .Where(p => p.Price > 1000)
                .Select(p => new { p.Name, CategoryName = p.Category.Name })
                .ToListAsync();

            foreach (var p in expensive)
            {
                // Console.WriteLine($"{p.Name} - {p.CategoryName}");
                goodPrinted++;
            }
        });
        Console.WriteLine($"  SQL ბრძანებები: {goodQueries}, ხაზები: {goodPrinted}");
    }
}
