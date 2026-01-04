namespace ConspiracyClicker.Models;

public class Conspiracy
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required string FlavorText { get; init; }
    public required double EvidenceCost { get; init; }
    public required double ClickBonus { get; init; }
    public double MultiplierBonus { get; init; } = 1.0;
    public int TinfoilReward { get; init; }
    public string Icon { get; init; } = "?";
}
