using Microsoft.EntityFrameworkCore;
using TravelerNotes.Entitites;

namespace TravelerNotes.Db;

public class TravelerNotesDb:DbContext
{
    public DbSet<Country> Countries { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Trip> Trips { get; set; }
    public DbSet<Place> Places { get; set; }
    public DbSet<Tag> Tags { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Tag ორივე მხრიდან (Place და Trip) Country-ზე ჯაჭვურად იშლება და SQL Server
        // ასეთ "multiple cascade paths"-ს არ უშვებს. ამიტომ Place -> Tag წაშლას
        // ბაზის ნაცვლად EF აკეთებს (ClientCascade).
        modelBuilder.Entity<Tag>()
            .HasOne(t => t.Place)
            .WithMany(p => p.Tags)
            .HasForeignKey(t => t.PlaceId)
            .OnDelete(DeleteBehavior.ClientCascade);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("Data Source=GUGA;Integrated Security=True;Database=TravelerNotes;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Application Name=\"SQL Server Management Studio\";Command Timeout=0");
    }
}
