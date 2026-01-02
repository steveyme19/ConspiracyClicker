using ConspiracyClicker.Utils;

namespace ConspiracyClicker.Data;

public class MatrixUpgrade
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required string FlavorText { get; init; }
    public required int GlitchCost { get; init; }
    public string Icon { get; init; } = "⟁";
}

public static class MatrixData
{
    // Matrix Break requires 5+ Illuminati ascensions and 100+ total Illuminati tokens earned
    public const int MATRIX_ASCENSION_REQUIREMENT = 5;
    public const double MATRIX_TOKEN_REQUIREMENT = 100;
    public const double GLITCH_TOKEN_SCALING = 10; // 1 glitch token per 10 Illuminati tokens spent

    public static readonly List<MatrixUpgrade> MatrixUpgrades = new()
    {
        new MatrixUpgrade
        {
            Id = "reality_warp",
            Name = "Reality Warp",
            Description = "x3 evidence per second permanently",
            FlavorText = "The code bends to your will.",
            GlitchCost = 1,
            Icon = IconHelper.Matrix.RealityWarp
        },
        new MatrixUpgrade
        {
            Id = "neo_clicking",
            Name = "Neo Clicking",
            Description = "Clicks scale with 2% of EPS instead of 1%",
            FlavorText = "I know kung fu... clicking.",
            GlitchCost = 1,
            Icon = IconHelper.Matrix.NeoClicking
        },
        new MatrixUpgrade
        {
            Id = "agent_infiltration",
            Name = "Agent Infiltration",
            Description = "+100% quest success rate (before cap)",
            FlavorText = "You've turned their agents into your agents.",
            GlitchCost = 2,
            Icon = IconHelper.Matrix.Agent
        },
        new MatrixUpgrade
        {
            Id = "source_code_access",
            Name = "Source Code Access",
            Description = "x2 believers from all sources",
            FlavorText = "You can see the strings that control them.",
            GlitchCost = 2,
            Icon = IconHelper.Matrix.SourceCode
        },
        new MatrixUpgrade
        {
            Id = "bullet_time",
            Name = "Bullet Time",
            Description = "x5 critical hit multiplier",
            FlavorText = "Dodge this.",
            GlitchCost = 3,
            Icon = IconHelper.Matrix.BulletTime
        },
        new MatrixUpgrade
        {
            Id = "architect_meeting",
            Name = "Architect Meeting",
            Description = "-50% all generator costs",
            FlavorText = "The one who built the Matrix owes you a favor.",
            GlitchCost = 4,
            Icon = IconHelper.Matrix.Architect
        },
        new MatrixUpgrade
        {
            Id = "red_pill_factory",
            Name = "Red Pill Factory",
            Description = "+5 Tinfoil per minute passively",
            FlavorText = "Mass awakening, one pill at a time.",
            GlitchCost = 5,
            Icon = IconHelper.Matrix.RedPill
        },
        new MatrixUpgrade
        {
            Id = "oracle_vision",
            Name = "Oracle Vision",
            Description = "Quests show exact success chance",
            FlavorText = "You've already seen the outcome.",
            GlitchCost = 3,
            Icon = IconHelper.Matrix.Oracle
        },
        new MatrixUpgrade
        {
            Id = "zion_mainframe",
            Name = "Zion Mainframe",
            Description = "Keep 10% of evidence after Illuminati prestige",
            FlavorText = "A backup in the last human city.",
            GlitchCost = 8,
            Icon = IconHelper.Matrix.Zion
        },
        new MatrixUpgrade
        {
            Id = "the_one",
            Name = "The One",
            Description = "x10 all production permanently",
            FlavorText = "You are The One. The prophecy is fulfilled.",
            GlitchCost = 15,
            Icon = IconHelper.Matrix.TheOne
        }
    };

    public static MatrixUpgrade? GetById(string id) => MatrixUpgrades.FirstOrDefault(u => u.Id == id);

    public static int CalculateGlitchTokensEarned(double totalIlluminatiTokensSpent)
    {
        // 1 glitch token per 10 Illuminati tokens ever spent
        return (int)Math.Floor(totalIlluminatiTokensSpent / GLITCH_TOKEN_SCALING);
    }

    public static bool CanBreakMatrix(int timesAscended, double totalIlluminatiTokensEarned)
    {
        return timesAscended >= MATRIX_ASCENSION_REQUIREMENT &&
               totalIlluminatiTokensEarned >= MATRIX_TOKEN_REQUIREMENT;
    }
}
