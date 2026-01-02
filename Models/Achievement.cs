namespace ConspiracyClicker.Models;

public enum AchievementType
{
    TotalEvidence,
    TotalClicks,
    GeneratorOwned,
    TotalGenerators,
    ConspiraciesProven,
    PlayTime,
    Special,
    TimesAscended,
    TimesMatrixBroken,
    QuestsCompleted,
    TotalTinfoil,
    CriticalClicks
}

public class Achievement
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public string? FlavorText { get; init; }
    public required AchievementType Type { get; init; }
    public required double Threshold { get; init; }
    public string? TargetId { get; init; }

    // Rewards
    public int TinfoilReward { get; init; }
    public double ClickBonusReward { get; init; }
    public double MultiplierReward { get; init; } = 1.0;
}
