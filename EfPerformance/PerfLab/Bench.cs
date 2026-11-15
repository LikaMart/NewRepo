using System.Diagnostics;

namespace PerfLab;

// დროის გაზომვა. პირველი გაშვება "თბება", ამიტომ ვუშვებ რამდენჯერმე
// და შედეგად ვიღებ ბოლო გაშვებას.
public static class Bench
{
    public static readonly List<(string Label, double Ms)> Results = new();

    public static async Task<double> Measure(string label, Func<Task> action, int runs = 3)
    {
        var times = new List<double>();
        for (int i = 0; i < runs; i++)
        {
            var sw = Stopwatch.StartNew();
            await action();
            sw.Stop();
            times.Add(sw.Elapsed.TotalMilliseconds);
        }

        var last = times[^1];
        Console.WriteLine($"  {label}: {string.Join(" / ", times.Select(t => t.ToString("F1")))} ms");
        Results.Add((label, last));
        return last;
    }

    public static void PrintSummary()
    {
        Console.WriteLine();
        Console.WriteLine("===== შედეგები (ბოლო გაშვება) =====");
        foreach (var (label, ms) in Results)
            Console.WriteLine($"{label,-55} {ms,10:F1} ms");
    }
}
