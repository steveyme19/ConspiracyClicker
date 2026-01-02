namespace ConspiracyClicker.Utils;

public static class NumberFormatter
{
    private static readonly (double threshold, string suffix)[] Suffixes =
    {
        (1e15, "Q"),
        (1e12, "T"),
        (1e9, "B"),
        (1e6, "M"),
        (1e3, "K"),
    };

    public static string Format(double value)
    {
        if (value < 1000)
        {
            return value < 10 ? value.ToString("F1") : Math.Floor(value).ToString("F0");
        }

        foreach (var (threshold, suffix) in Suffixes)
        {
            if (value >= threshold)
            {
                double scaled = value / threshold;
                return scaled < 10
                    ? $"{scaled:F2}{suffix}"
                    : scaled < 100
                        ? $"{scaled:F1}{suffix}"
                        : $"{scaled:F0}{suffix}";
            }
        }

        return value.ToString("F0");
    }

    public static string FormatPerSecond(double value)
    {
        return $"{Format(value)}/sec";
    }
}
