namespace ConspiracyClicker.Models;

public enum QuestRisk
{
    Low,      // Believers always return, partial rewards on fail
    Medium,   // Believers return, no reward on fail
    High      // Believers can be lost, big rewards on success
}

public class Quest
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required string FlavorText { get; init; }
    public required QuestRisk Risk { get; init; }
    public required double BelieversRequired { get; init; }
    public required int DurationSeconds { get; init; }
    public required double SuccessChance { get; init; } // 0.0 to 1.0
    public string Icon { get; init; } = "?";

    // Rewards on success
    public double EvidenceReward { get; init; }
    public double EvidenceMultiplier { get; init; } = 1.0; // Multiplied by current EPS
    public long TinfoilReward { get; init; }
    public double BelieverReward { get; init; } // Bonus believers gained
    public double ClickPowerBonus { get; init; } // Temporary click power bonus duration (seconds)
    public string? UnlockUpgradeId { get; init; }

    // Partial reward on fail (for Low risk)
    public double FailEvidenceMultiplier { get; init; } = 0.25;
}

public class ActiveQuest
{
    public required string QuestId { get; init; }
    public required DateTime StartTime { get; init; }
    public required DateTime EndTime { get; init; }
    public required double BelieversSent { get; init; }

    public bool IsComplete => DateTime.Now >= EndTime;
    public double Progress => Math.Min(1.0, (DateTime.Now - StartTime).TotalSeconds / (EndTime - StartTime).TotalSeconds);
    public TimeSpan TimeRemaining => EndTime > DateTime.Now ? EndTime - DateTime.Now : TimeSpan.Zero;
}
