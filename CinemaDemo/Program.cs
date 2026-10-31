using System.Globalization;
using System.Text;
using CinemaDemo.Models;
using Microsoft.EntityFrameworkCore;

Console.OutputEncoding = Encoding.UTF8;

using var db = new CinemaContext();

// სეანსები დროის მიხედვით: დრო · ფილმი · ფასი · დარბაზი
var sessions = db.Sessions
    .Include(s => s.Movie)
    .Include(s => s.Hall)
    .OrderBy(s => s.StartsAt)
    .ToList();

foreach (var s in sessions)
{
    var price = s.Price.ToString("0.00", CultureInfo.InvariantCulture);
    Console.WriteLine($"{s.Time} · {s.Movie.Title} · {price} ₾ · {s.Hall.Name}");
}

// ⭐ ბონუსი: დარბაზი უკვე არსებული სახელით
Console.WriteLine();
Console.WriteLine("ბონუსი: ვამატებთ დარბაზს სახელით \"VIP დარბაზი\"...");
try
{
    db.Halls.Add(new Hall { Name = "VIP დარბაზი", Seats = 30 });
    db.SaveChanges();
}
catch (DbUpdateException ex)
{
    Console.WriteLine("ბაზამ არ დაუშვა:");
    Console.WriteLine(ex.InnerException?.Message);
}
