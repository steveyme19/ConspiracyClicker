namespace ConspiracyClicker.Models;

/// <summary>
/// One of the two payoffs offered when a conspiracy is proven.
///
/// Proving used to be automatic and free - a checklist that filled itself in the moment
/// lifetime evidence crossed a line. Making it a fork turns each of the twenty-five into a
/// decision, and because the options sit on different axes the right answer changes as a run
/// develops: the click payoff is strong early and worthless once generators dominate.
/// </summary>
public class ConspiracyReward
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }

    public double ClickBonus { get; init; }
    public double MultiplierBonus { get; init; } = 1.0;
    public double BelieverMultiplier { get; init; } = 1.0;
    public int TinfoilReward { get; init; }
    public double CritChanceBonus { get; init; }
    public double QuestSpeedMultiplier { get; init; } = 1.0;
}
