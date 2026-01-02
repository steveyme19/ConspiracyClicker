namespace ConspiracyClicker.Models;

public enum UpgradeType
{
    ClickPower,      // Adds flat click power
    ClickMultiplier, // Multiplies click power
    EpsToClick,      // Adds % of EPS to each click
    GeneratorBoost,  // Boosts specific generator
    GlobalBoost      // Boosts all production
}

public class Upgrade
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required string FlavorText { get; init; }
    public required UpgradeType Type { get; init; }
    public required double Value { get; init; }
    public string? TargetGeneratorId { get; init; }

    // Cost can be Evidence or Tinfoil
    public double EvidenceCost { get; init; }
    public int TinfoilCost { get; init; }

    // Unlock condition
    public double RequiredEvidence { get; init; }
    public int RequiredGeneratorCount { get; init; }
    public string? RequiredGeneratorId { get; init; }
}
