namespace ConspiracyClicker.Models;

public enum GeneratorUpgradeType
{
    // Generator-specific (3 per generator)
    ProductionMultiplier,    // Multiplies this generator's output
    CostReduction,           // Reduces this generator's cost
    BelieverBonus,           // Increases believer output from this generator

    // Global bonuses (1 per generator - rotates through these)
    GlobalClickPower,        // Increases click power
    GlobalQuestSpeed,        // Reduces quest duration
    GlobalTinfoilGain,       // Increases tinfoil from all sources
    GlobalCritChance,        // Increases critical hit chance
    GlobalCritDamage,        // Increases critical hit multiplier
    GlobalGoldenEye,         // Golden eyes appear more often
    GlobalBelieverGain,      // More believers from all sources
    GlobalEpsMultiplier      // Multiplies all EPS
}

public class GeneratorUpgrade
{
    public required string Id { get; init; }
    public required string GeneratorId { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required int UnlockLevel { get; init; } // 25, 50, 75, or 100
    public required GeneratorUpgradeType Type { get; init; }
    public required double Value { get; init; } // Multiplier or flat bonus depending on type
    public string Icon { get; init; } = "★";
}
