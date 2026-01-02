namespace ConspiracyClicker.Data;

public static class IconData
{
    // Geometric symbols for the Illuminati aesthetic
    public static readonly Dictionary<string, string> GeneratorIcons = new()
    {
        ["red_string"] = "◇─◇",
        ["suspicious_neighbor"] = "◉",
        ["basement_researcher"] = "▽",
        ["blogspot_blog"] = "◈",
        ["youtube_channel"] = "▷",
        ["discord_server"] = "◎",
        ["am_radio"] = "))))",
        ["podcast"] = "◉◉◉",
        ["truth_conference"] = "△△△",
        ["netflix_doc"] = "▣",
        ["spy_satellite"] = "◉⟐◉",
        ["shadow_government"] = "◬"
    };

    public static readonly Dictionary<string, string> ConspiracyIcons = new()
    {
        ["birds_arent_real"] = "▷◇◁",
        ["flat_earth"] = "▭",
        ["moon_landing"] = "◐",
        ["lizard_people"] = "§§",
        ["australia_fake"] = "◇̷",
        ["finland_myth"] = "∅",
        ["mattress_laundering"] = "▯▯▯",
        ["time_invention"] = "◷",
        ["simulation"] = "⟦⟧",
        ["you_are_conspiracy"] = "◬◉◬"
    };

    public static readonly Dictionary<string, string> UpgradeIcons = new()
    {
        // Click power
        ["reinforced_tinfoil"] = "◠◠",
        ["magnifying_glass"] = "◎",
        ["red_marker"] = "◉",
        ["cork_board"] = "▣",
        ["night_vision"] = "◉◉",
        ["mechanical_keyboard"] = "▤▤▤",

        // Multipliers
        ["third_eye_drops"] = "◉△◉",
        ["caffeine_iv"] = "☕→",
        ["quantum_fingers"] = "◇◇◇",

        // EPS to click
        ["momentum_theory"] = "→→",
        ["synergy_doctrine"] = "◇⟷◇",
        ["unified_field"] = "⊕",
        ["clickchain_reaction"] = "◉→◉→◉",
        ["infinite_recursion"] = "∞",

        // Generator boosts
        ["premium_string"] = "◇══◇",
        ["neighborhood_watch"] = "◉◉◉",
        ["ergonomic_chair"] = "⌂",
        ["seo_optimization"] = "▲▲▲",
        ["clickbait_thumbnails"] = "▷!",

        // Global
        ["viral_momentum"] = "◉→→→",
        ["mass_awakening"] = "◉◉◉◉",
        ["truth_singularity"] = "⊛"
    };

    public static readonly Dictionary<string, string> QuestIcons = new()
    {
        ["coffee_run"] = "☕",
        ["library_search"] = "▤",
        ["dumpster_dive"] = "▼",
        ["government_tour"] = "◬",
        ["ham_radio"] = "≋",
        ["tech_conference"] = "◈",
        ["warehouse_raid"] = "▣!",
        ["server_breach"] = "◎◎",
        ["bunker_expedition"] = "▽▽",
        ["satellite_hack"] = "◉⟐◉"
    };

    public static readonly Dictionary<QuestRiskLevel, string> RiskIcons = new()
    {
        [QuestRiskLevel.Low] = "◇",
        [QuestRiskLevel.Medium] = "◈",
        [QuestRiskLevel.High] = "◆!"
    };

    public enum QuestRiskLevel { Low, Medium, High }
}
