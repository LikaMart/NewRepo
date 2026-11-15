using Microsoft.EntityFrameworkCore;
using ProductsApi.Models;

namespace ProductsApi.Data;

// იგივე ბაზას იყენებს რასაც PerfLab (EfPerfDb). ცხრილები და ინდექსები
// PerfLab-ის მიგრაციებით იქმნება, ამიტომ აქ მიგრაციები არ მაქვს.
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(e => e.Property(c => c.Name).HasMaxLength(100));

        modelBuilder.Entity<Product>(e =>
        {
            e.Property(p => p.Name).HasMaxLength(200);
            e.Property(p => p.Price).HasPrecision(18, 2);
            e.HasIndex(p => p.Name);
            e.HasIndex(p => new { p.CategoryId, p.Price });
        });
    }
}
