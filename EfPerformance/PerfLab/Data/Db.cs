using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace PerfLab.Data;

public static class Db
{
    public const string ConnectionString =
        "Server=.;Database=EfPerfDb;Trusted_Connection=True;TrustServerCertificate=True";

    // ჩვეულებრივი კონტექსტი, ლოგის გარეშე (გაზომვებისთვის)
    public static AppDbContext Create()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(ConnectionString, o => o.CommandTimeout(300))
            .Options;
        return new AppDbContext(options);
    }

    // კონტექსტი, რომელიც SQL-ს კონსოლში წერს
    public static AppDbContext CreateWithLog()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(ConnectionString)
            .LogTo(Console.WriteLine, new[] { DbLoggerCategory.Database.Command.Name }, LogLevel.Information)
            .Options;
        return new AppDbContext(options);
    }

    // ითვლის რამდენი SQL ბრძანება გაეშვა (N+1-ის საჩვენებლად)
    public static AppDbContext CreateWithCounter(Action onCommand)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(ConnectionString)
            .LogTo(_ => onCommand(), new[] { RelationalEventId.CommandExecuted })
            .Options;
        return new AppDbContext(options);
    }
}

// dotnet ef migrations add ... ამას იყენებს
public class DesignTimeFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args) => Db.Create();
}
