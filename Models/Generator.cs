namespace ConspiracyClicker.Models;

public class Generator
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string FlavorText { get; init; }
    public required double BaseCost { get; init; }
    public double CostMultiplier { get; init; } = 1.15;
    public required double BaseProduction { get; init; }
    public int BelieverBonus { get; init; }

    public double GetCost(int owned)
    {
        return BaseCost * Math.Pow(CostMultiplier, owned);
    }

    public double GetProduction(int owned, double multiplier = 1.0)
    {
        return BaseProduction * owned * multiplier;
    }
}
