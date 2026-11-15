using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PerfLab.Data;

namespace PerfLab;

public static class Part4
{
    public static async Task RunAsync()
    {
        Console.WriteLine("\n===== ნაწილი 4 =====");

        var services = new ServiceCollection();
        services.AddMemoryCache();
        services.AddDbContext<AppDbContext>(o => o.UseSqlServer(Db.ConnectionString));
        services.AddScoped<CategoryService>();
        using var provider = services.BuildServiceProvider();

        // კავშირის "გათბობა", რომ პირველ გამოძახებაში მარტო ბაზის დრო ჩანდეს
        using (var scope = provider.CreateScope())
        {
            var warm = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await warm.Categories.AsNoTracking().ToListAsync();
        }

        // ---------- 4.1 ----------
        Console.WriteLine("\n--- 4.1 ---");
        using (var scope = provider.CreateScope())
        {
            var service = scope.ServiceProvider.GetRequiredService<CategoryService>();
            for (int i = 1; i <= 3; i++)
            {
                var sw = Stopwatch.StartNew();
                var list = await service.GetCategories();
                sw.Stop();
                string label = $"4.1 GetCategories() გამოძახება #{i}";
                Console.WriteLine($"  {label}: {sw.Elapsed.TotalMilliseconds:F3} ms ({list.Count} კატეგორია)");
                Bench.Results.Add((label, sw.Elapsed.TotalMilliseconds));
            }
        }

        // ---------- 4.2 ----------
        Console.WriteLine("\n--- 4.2 ---");
        string original;
        using (var scope = provider.CreateScope())
        {
            var service = scope.ServiceProvider.GetRequiredService<CategoryService>();
            original = (await service.GetCategories()).First(c => c.Id == 2).Name;
            Console.WriteLine($"  ახლა: {original}");

            await service.UpdateCategoryWithoutInvalidation(2, "Books & Magazines");
            var stale = (await service.GetCategories()).First(c => c.Id == 2).Name;
            Console.WriteLine($"  ბაზაში შევცვალე, ქეში არ გავასუფთავე: GetCategories აბრუნებს {stale}");
        }

        using (var scope = provider.CreateScope())
        {
            var service = scope.ServiceProvider.GetRequiredService<CategoryService>();
            await service.UpdateCategory(2, "Books & Magazines");
            var fresh = (await service.GetCategories()).First(c => c.Id == 2).Name;
            Console.WriteLine($"  UpdateCategory ქეშის გასუფთავებით: GetCategories აბრუნებს {fresh}");

            // ძველ სახელს ვაბრუნებ
            await service.UpdateCategory(2, original);
        }
    }
}
