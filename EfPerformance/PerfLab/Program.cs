using PerfLab;

// გაშვება: dotnet run            ყველა ნაწილი
//          dotnet run -- 2 3     მხოლოდ მითითებული ნაწილები
var parts = args.Length > 0 ? args : new[] { "1", "2", "3", "4" };

await Seeder.SeedAsync();

foreach (var part in parts)
{
    switch (part)
    {
        case "1": await Part1.RunAsync(); break;
        case "2": await Part2.RunAsync(); break;
        case "3": await Part3.RunAsync(); break;
        case "4": await Part4.RunAsync(); break;
    }
}

Bench.PrintSummary();
