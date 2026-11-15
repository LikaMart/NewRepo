using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using ProductsApi.Data;
using ProductsApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddMemoryCache();
builder.Services.AddScoped<CategoryCache>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// თითო მოთხოვნის დროს ვწერ X-Elapsed-Ms ჰედერში, გაზომვისთვის
app.Use(async (context, next) =>
{
    var sw = Stopwatch.StartNew();
    context.Response.OnStarting(() =>
    {
        context.Response.Headers["X-Elapsed-Ms"] = sw.Elapsed.TotalMilliseconds.ToString("F1");
        return Task.CompletedTask;
    });
    await next();
});

app.MapControllers();

app.Run();
