namespace ConspiracyClicker.Utils;

public static class NumberFormatter
{
    private static readonly (double threshold, string suffix)[] Suffixes =
    {
        (1e66, "UnVi"),  // Unvigintillion
        (1e63, "Vi"),    // Vigintillion
        (1e60, "NoD"),   // Novemdecillion
        (1e57, "OcD"),   // Octodecillion
        (1e54, "SpD"),   // Septendecillion
        (1e51, "SxD"),   // Sexdecillion
        (1e48, "QiD"),   // Quindecillion
        (1e45, "QaD"),   // Quattuordecillion
        (1e42, "TrD"),   // Tredecillion
        (1e39, "DuD"),   // Duodecillion
        (1e36, "UnD"),   // Undecillion
        (1e33, "Dc"),    // Decillion
        (1e30, "No"),    // Nonillion
        (1e27, "Oc"),    // Octillion
        (1e24, "Sp"),    // Septillion
        (1e21, "Sx"),    // Sextillion
        (1e18, "Qi"),    // Quintillion
        (1e15, "Qa"),    // Quadrillion
        (1e12, "T"),     // Trillion
        (1e9, "B"),      // Billion
        (1e6, "M"),      // Million
        (1e3, "K"),      // Thousand
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
