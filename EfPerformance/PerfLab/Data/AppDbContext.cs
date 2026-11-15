using Microsoft.EntityFrameworkCore;
using PerfLab.Models;

namespace PerfLab.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Customer> Customers => Set<Customer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(e =>
        {
            e.Property(c => c.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Product>(e =>
        {
            // nvarchar(max)-ზე ინდექსი ვერ დაიდება, ამიტომ სიგრძე აუცილებელია
            e.Property(p => p.Name).HasMaxLength(200);
            e.Property(p => p.Price).HasPrecision(18, 2);

            // 3.1
            e.HasIndex(p => p.Name);

            // 3.2 ფილტრი CategoryId-ით + დალაგება Price-ით
            e.HasIndex(p => new { p.CategoryId, p.Price });
        });

        // 3.3
        modelBuilder.Entity<Customer>(e =>
        {
            e.Property(c => c.FullName).HasMaxLength(100);
            e.Property(c => c.Email).HasMaxLength(256);
            e.HasIndex(c => c.Email).IsUnique();
        });
    }
}
