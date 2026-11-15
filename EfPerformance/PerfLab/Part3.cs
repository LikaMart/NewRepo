using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using PerfLab.Data;
using PerfLab.Models;

namespace PerfLab;

public static class Part3
{
    // ბაზას გადავიყვან კონკრეტულ მიგრაციაზე (იგივეა რაც Update-Database <name>)
    static async Task MigrateTo(string migration)
    {
        using var db = Db.Create();
        await db.GetService<IMigrator>().MigrateAsync(migration);
        Console.WriteLine($"  [ბაზა გადავიდა მიგრაციაზე: {migration}]");
    }

    public static async Task RunAsync()
    {
        Console.WriteLine("\n===== ნაწილი 3 =====");

        // ---------- 3.1 ინდექსი Name-ზე ----------
        Console.WriteLine("\n--- 3.1 ---");
        await MigrateTo("Initial"); // ინდექსების გარეშე

        Func<Task> byName = async () =>
        {
            using var db = Db.Create();
            var found = await db.Products.AsNoTracking()
                .Where(p => p.Name == "Product 99999")
                .ToListAsync();
        };

        await Bench.Measure("3.1 Name == ... ინდექსის გარეშე", byName);
        await MigrateTo("AddNameIndex");
        await Bench.Measure("3.1 Name == ... ინდექსით", byName);

        // ---------- 3.2 კომპოზიტური ინდექსი ----------
        Console.WriteLine("\n--- 3.2 ---");
        Func<Task> byCategoryOrderByPrice = async () =>
        {
            using var db = Db.Create();
            var list = await db.Products.AsNoTracking()
                .Where(p => p.CategoryId == 3)
                .OrderBy(p => p.Price)
                .Take(50)
                .ToListAsync();
        };

        await Bench.Measure("3.2 მხოლოდ IX_Products_CategoryId", byCategoryOrderByPrice);
        await MigrateTo("AddCategoryPriceIndex");
        await Bench.Measure("3.2 IX_Products_CategoryId_Price", byCategoryOrderByPrice);

        // ---------- 3.3 უნიკალური ინდექსი ----------
        Console.WriteLine("\n--- 3.3 ---");
        await MigrateTo("AddCustomerEmailUnique");

        using (var db = Db.Create())
        {
            await db.Customers.ExecuteDeleteAsync();

            db.Customers.Add(new Customer { FullName = "Giorgi", Email = "test@mail.ge" });
            await db.SaveChangesAsync();
            Console.WriteLine("  პირველი კლიენტი დაემატა");

            db.Customers.Add(new Customer { FullName = "Nino", Email = "test@mail.ge" });
            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"  შეცდომა: {ex.GetType().Name}");
                if (ex.InnerException is SqlException sql)
                    Console.WriteLine($"  SqlException #{sql.Number}: {sql.Message}");
            }
        }
    }
}
