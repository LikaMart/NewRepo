using System.Text;
using LibraryDemo.Models;
using Microsoft.EntityFrameworkCore;

Console.OutputEncoding = Encoding.UTF8;

using var db = new LibraryDemoDbContext();

// 1 ახალი წიგნი (ვამატებთ მხოლოდ ერთხელ)
if (!db.Books.Any(b => b.Title == "გლახის ნაამბობი"))
{
    db.Books.Add(new Book { Title = "გლახის ნაამბობი", Year = 1873, AuthorId = 1, Pages = 90 });
    db.SaveChanges();
}

// წიგნები ავტორებით
var books = db.Books.Include(b => b.Author).ToList();

foreach (var book in books)
{
    Console.WriteLine($"{book.Info} — {book.Author.FullName}, გვერდები: {book.Pages?.ToString() ?? "?"}");
}
