using System.Diagnostics;
using System.IO;

namespace ConspiracyClicker.Utils;

/// <summary>
/// Development-only frame instrumentation. Completely inert unless CC_PERFLOG=1 is set in the
/// environment, so it costs a single already-evaluated bool check in a normal run.
///
/// Used to measure the cost of a UI refresh against a known save, which is the only honest way
/// to tell whether a change to the refresh path actually helped.
///
///   CC_PERFLOG=1            enable sampling
///   CC_PERFRUN=slot:seconds auto-load that save slot, run headlong for N seconds, then quit
///   CC_PERFOUT=path         where to write the summary (default: alongside the save files)
/// </summary>
public static class PerfLog
{
    public static readonly bool Enabled =
        Environment.GetEnvironmentVariable("CC_PERFLOG") == "1";

    private static readonly List<double> Samples = new();
    private static readonly Stopwatch Clock = new();

    public static void Begin()
    {
        if (!Enabled) return;
        Clock.Restart();
    }

    public static void End()
    {
        if (!Enabled) return;
        Clock.Stop();
        Samples.Add(Clock.Elapsed.TotalMilliseconds);
    }

    /// <summary>Tab index to sit on during an automated run, if CC_PERFTAB is set.</summary>
    public static int? Tab()
    {
        string? spec = Environment.GetEnvironmentVariable("CC_PERFTAB");
        return int.TryParse(spec, out int index) ? index : null;
    }

    /// <summary>Returns (slot, seconds) if an automated perf run was requested.</summary>
    public static (int slot, double seconds)? AutoRun()
    {
        string? spec = Environment.GetEnvironmentVariable("CC_PERFRUN");
        if (string.IsNullOrWhiteSpace(spec)) return null;

        string[] parts = spec.Split(':');
        if (parts.Length != 2) return null;
        if (!int.TryParse(parts[0], out int slot)) return null;
        if (!double.TryParse(parts[1], out double seconds)) return null;

        return (slot, seconds);
    }

    public static void Dump(string label)
    {
        if (!Enabled) return;

        string path = Environment.GetEnvironmentVariable("CC_PERFOUT")
            ?? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ConspiracyClicker", "perf.txt");

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        if (Samples.Count == 0)
        {
            File.WriteAllText(path, $"{label}: no samples\n");
            return;
        }

        var sorted = Samples.OrderBy(s => s).ToList();
        double mean = sorted.Average();
        double p50 = sorted[sorted.Count / 2];
        double p95 = sorted[(int)(sorted.Count * 0.95)];
        double max = sorted[^1];
        double total = sorted.Sum();

        File.WriteAllText(path,
            $"{label}\n" +
            $"refreshes   {sorted.Count}\n" +
            $"mean ms     {mean:F3}\n" +
            $"p50 ms      {p50:F3}\n" +
            $"p95 ms      {p95:F3}\n" +
            $"max ms      {max:F3}\n" +
            $"total ms    {total:F1}\n");
    }
}
