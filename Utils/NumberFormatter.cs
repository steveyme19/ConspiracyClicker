namespace ConspiracyClicker.Utils;

public static class NumberFormatter
{
    private static readonly (double threshold, string suffix)[] Suffixes =
    {
        // Ultra high tiers (for ridiculous late game)
        (1e303, "Ce"),   // Centillion
        (1e300, "NNg"),  // Novemnonagintillion
        (1e297, "ONg"),  // Octononagintillion
        (1e294, "SNg"),  // Septemnonagintillion
        (1e291, "SxNg"), // Sexnonagintillion
        (1e288, "QiNg"), // Quinnonagintillion
        (1e285, "QaNg"), // Quattuornonagintillion
        (1e282, "TrNg"), // Trenonagintillion
        (1e279, "DuNg"), // Duononagintillion
        (1e276, "UnNg"), // Unnonagintillion
        (1e273, "Ng"),   // Nonagintillion
        (1e270, "NOc"),  // Novemoctogintillion
        (1e267, "OOc"),  // Octooctogintillion
        (1e264, "SOc"),  // Septemoctogintillion
        (1e261, "SxOc"), // Sexoctogintillion
        (1e258, "QiOc"), // Quinoctogintillion
        (1e255, "QaOc"), // Quattuoroctogintillion
        (1e252, "TrOc"), // Treoctogintillion
        (1e249, "DuOc"), // Duooctogintillion
        (1e246, "UnOc"), // Unoctogintillion
        (1e243, "Og"),   // Octogintillion
        (1e240, "NSp"),  // Novemseptuagintillion
        (1e237, "OSp"),  // Octoseptuagintillion
        (1e234, "SSp"),  // Septemseptuagintillion
        (1e231, "SxSp"), // Sexseptuagintillion
        (1e228, "QiSp"), // Quinseptuagintillion
        (1e225, "QaSp"), // Quattuorseptuagintillion
        (1e222, "TrSp"), // Treseptuagintillion
        (1e219, "DuSp"), // Duoseptuagintillion
        (1e216, "UnSp"), // Unseptuagintillion
        (1e213, "Spg"),  // Septuagintillion
        (1e210, "NSx"),  // Novemsexagintillion
        (1e207, "OSx"),  // Octosexagintillion
        (1e204, "SSx"),  // Septemsexagintillion
        (1e201, "SxSx"), // Sexsexagintillion
        (1e198, "QiSx"), // Quinsexagintillion
        (1e195, "QaSx"), // Quattuorsexagintillion
        (1e192, "TrSx"), // Tresexagintillion
        (1e189, "DuSx"), // Duosexagintillion
        (1e186, "UnSx"), // Unsexagintillion
        (1e183, "Sxg"),  // Sexagintillion
        (1e180, "NQi"),  // Novemquinquagintillion
        (1e177, "OQi"),  // Octoquinquagintillion
        (1e174, "SQi"),  // Septemquinquagintillion
        (1e171, "SxQi"), // Sexquinquagintillion
        (1e168, "QiQi"), // Quinquinquagintillion
        (1e165, "QaQi"), // Quattuorquinquagintillion
        (1e162, "TrQi"), // Trequinquagintillion
        (1e159, "DuQi"), // Duoquinquagintillion
        (1e156, "UnQi"), // Unquinquagintillion
        (1e153, "Qig"),  // Quinquagintillion
        (1e150, "NQa"),  // Novemquadragintillion
        (1e147, "OQa"),  // Octoquadragintillion
        (1e144, "SQa"),  // Septemquadragintillion
        (1e141, "SxQa"), // Sexquadragintillion
        (1e138, "QiQa"), // Quinquadragintillion
        (1e135, "QaQa"), // Quattuorquadragintillion
        (1e132, "TrQa"), // Trequadragintillion
        (1e129, "DuQa"), // Duoquadragintillion
        (1e126, "UnQa"), // Unquadragintillion
        (1e123, "Qag"),  // Quadragintillion
        (1e120, "NTr"),  // Novemtrigintillion
        (1e117, "OTr"),  // Octotrigintillion
        (1e114, "STr"),  // Septemtrigintillion
        (1e111, "SxTr"), // Sextrigintillion
        (1e108, "QiTr"), // Quintrigintillion
        (1e105, "QaTr"), // Quattuortrigintillion
        (1e102, "TrTr"), // Tretrigintillion
        (1e99, "DuTr"),  // Duotrigintillion
        (1e96, "UnTr"),  // Untrigintillion
        (1e93, "Trg"),   // Trigintillion
        (1e90, "NVi"),   // Novemvigintillion
        (1e87, "OVi"),   // Octovigintillion
        (1e84, "SVi"),   // Septemvigintillion
        (1e81, "SxVi"),  // Sexvigintillion
        (1e78, "QiVi"),  // Quinvigintillion
        (1e75, "QaVi"),  // Quattuorvigintillion
        (1e72, "TrVi"),  // Trevigintillion
        (1e69, "DuVi"),  // Duovigintillion
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
        // Handle special values
        if (double.IsInfinity(value)) return "∞";
        if (double.IsNaN(value)) return "NaN";

        if (value < 1000)
        {
            return value < 10 ? value.ToString("F1") : Math.Floor(value).ToString("F0");
        }

        // Check if beyond our highest tier - use scientific notation
        if (value >= 1e306)
        {
            // Use compact scientific notation for truly astronomical numbers
            int exp = (int)Math.Floor(Math.Log10(value));
            double mantissa = value / Math.Pow(10, exp);
            return $"{mantissa:F2}e{exp}";
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

    /// <summary>
    /// Format a value that should always be displayed as a whole number (believers, clicks, counts, etc.)
    /// </summary>
    public static string FormatInteger(double value)
    {
        // Handle special values
        if (double.IsInfinity(value)) return "∞";
        if (double.IsNaN(value)) return "NaN";

        value = Math.Floor(value);

        if (value < 1000)
        {
            return value.ToString("F0");
        }

        // Check if beyond our highest tier - use scientific notation
        if (value >= 1e306)
        {
            int exp = (int)Math.Floor(Math.Log10(value));
            double mantissa = value / Math.Pow(10, exp);
            return $"{mantissa:F2}e{exp}";
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
}
