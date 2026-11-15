using Microsoft.EntityFrameworkCore;
using PerfLab.Data;
using PerfLab.Models;

namespace PerfLab;

public static class Part2
{
    // 2.2 Skip/Take პაგინაცია
    public static Task<List<Product>> GetPage(AppDbContext db, int page, int size) =>
        db.Products
            .AsNoTracking()
            .OrderBy(p => p.Id)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

    // 2.3 Keyset პაგინაცია: Skip-ის მაგივრად ვიწყებთ ბოლო ნანახი Id-ის შემდეგ
    public static Task<List<Product>> GetNext(AppDbContext db, int lastId, int size) =>
        db.Products
            .AsNoTracking()
            .Where(p => p.Id > lastId)
            .OrderBy(p => p.Id)
            .Take(size)
            .ToListAsync();

    public static async Task RunAsync()
    {
        Console.WriteLine("\n===== ნაწილი 2 =====");

        // ---------- 2.1 AsNoTracking ----------
        Console.WriteLine("\n--- 2.1 ---");
        await Bench.Measure("2.1 ტრექინგით", async () =>
        {
            using var db = Db.Create();
            var list = await db.Products.ToListAsync();
        });
        await Bench.Measure("2.1 AsNoTracking", async () =>
        {
            using var db = Db.Create();
            var list = await db.Products.AsNoTracking().ToListAsync();
        });

        // ---------- 2.2 Skip/Take ----------
        Console.WriteLine("\n--- 2.2 ---");
        const int size = 25;
        List<Product> page4000 = new();
        await Bench.Measure("2.2 GetPage(page = 1)", async () =>
        {
            using var db = Db.Create();
            await GetPage(db, 1, size);
        });
        await Bench.Measure("2.2 GetPage(page = 4000)", async () =>
        {
            using var db = Db.Create();
            page4000 = await GetPage(db, 4000, size);
        });

        // ---------- 2.3 Keyset ----------
        Console.WriteLine("\n--- 2.3 ---");
        int lastId;
        using (var db = Db.Create())
        {
            // წინა გვერდის (3999) ბოლო ჩანაწერის Id
            lastId = await db.Products.OrderBy(p => p.Id)
                .Skip(3999 * size - 1).Select(p => p.Id).FirstAsync();
        }
        List<Product> next = new();
        await Bench.Measure($"2.3 GetNext(lastId = {lastId})", async () =>
        {
            using var db = Db.Create();
            next = await GetNext(db, lastId, size);
        });
        bool same = next.Select(p => p.Id).SequenceEqual(page4000.Select(p => p.Id));
        Console.WriteLine($"  იგივე ჩანაწერებს აბრუნებს რაც page 4000: {same}");

        // ---------- 2.4 ბეჩინგი ----------
        Console.WriteLine("\n--- 2.4 ---");
        await Bench.Measure("2.4 (ა) ციკლში SaveChanges x1000", async () =>
        {
            using var db = Db.Create();
            for (int i = 1; i <= 1000; i++)
            {
                db.Products.Add(new Product
                {
                    Name = $"Batch A {i}",
                    Description = "test",
                    Price = 10,
                    CategoryId = 1
                });
                await db.SaveChangesAsync();
            }
        }, runs: 1);
        await Bench.Measure("2.4 (ბ) AddRange + ერთი SaveChanges", async () =>
        {
            using var db = Db.Create();
            var items = Enumerable.Range(1, 1000).Select(i => new Product
            {
                Name = $"Batch B {i}",
                Description = "test",
                Price = 10,
                CategoryId = 1
            });
            db.Products.AddRange(items);
            await db.SaveChangesAsync();
        }, runs: 1);

        // სატესტო ჩანაწერებს ვშლი, რომ ისევ 100 000 დარჩეს
        using (var db = Db.Create())
        {
            int deleted = await db.Products.Where(p => p.Name.StartsWith("Batch ")).ExecuteDeleteAsync();
            Console.WriteLine($"  წაიშალა {deleted} სატესტო ჩანაწერი");
        }

        // ---------- 2.5 AsSplitQuery ----------
        Console.WriteLine("\n--- 2.5 Include ერთი მოთხოვნით (SQL) ---");
        using (var db = Db.CreateWithLog())
        {
            var orders = await db.Orders.AsNoTracking()
                .Include(o => o.Items)
                .Include(o => o.Comments)
                .ToListAsync();
        }
        Console.WriteLine("\n--- 2.5 Include + AsSplitQuery (SQL) ---");
        using (var db = Db.CreateWithLog())
        {
            var orders = await db.Orders.AsNoTracking()
                .Include(o => o.Items)
                .Include(o => o.Comments)
                .AsSplitQuery()
                .ToListAsync();
        }

        using (var db = Db.Create())
        {
            // რამდენ სტრიქონს აბრუნებს ერთი JOIN-იანი მოთხოვნა (Items x Comments თითო შეკვეთაზე)
            int joinedRows = await db.OrderItems
                .Join(db.Comments, i => i.OrderId, c => c.OrderId, (i, c) => i.Id)
                .CountAsync();
            int splitRows = await db.Orders.CountAsync() + await db.OrderItems.CountAsync() + await db.Comments.CountAsync();
            Console.WriteLine($"\n  სტრიქონები ერთ მოთხოვნაში: {joinedRows}, სამ ცალკე მოთხოვნაში ჯამში: {splitRows}");
        }

        await Bench.Measure("2.5 Include (ერთი მოთხოვნა)", async () =>
        {
            using var db = Db.Create();
            await db.Orders.AsNoTracking().Include(o => o.Items).Include(o => o.Comments).ToListAsync();
        });
        await Bench.Measure("2.5 Include + AsSplitQuery", async () =>
        {
            using var db = Db.Create();
            await db.Orders.AsNoTracking().Include(o => o.Items).Include(o => o.Comments).AsSplitQuery().ToListAsync();
        });
    }
}
