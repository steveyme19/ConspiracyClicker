using ConspiracyClicker.Models;

namespace ConspiracyClicker.Data;

/// <summary>
/// Builds the two payoff options offered for each conspiracy.
///
/// Option A is always the conspiracy's original reward, so a save made before the fork existed
/// keeps exactly the bonuses it already had. Option B trades the click bonus - the part that
/// stops mattering once generators dominate - for something on a different axis, rotating
/// through a handful of archetypes so consecutive conspiracies do not offer the same deal.
/// Magnitudes are derived from the conspiracy's own numbers rather than hand-tuned per entry.
/// </summary>
public static class ConspiracyRewardData
{
    public const string OptionA = "a";
    public const string OptionB = "b";

    private static readonly Dictionary<string, (ConspiracyReward a, ConspiracyReward b)> Rewards;

    static ConspiracyRewardData()
    {
        Rewards = new Dictionary<string, (ConspiracyReward, ConspiracyReward)>(StringComparer.Ordinal);

        int index = 0;
        foreach (var conspiracy in ConspiracyData.AllConspiracies)
        {
            Rewards[conspiracy.Id] = (BuildOriginal(conspiracy), BuildAlternative(conspiracy, index));
            index++;
        }
    }

    /// <summary>The conspiracy exactly as it behaved before the fork existed.</summary>
    private static ConspiracyReward BuildOriginal(Conspiracy conspiracy) => new()
    {
        Id = OptionA,
        Name = "Go Public",
        Description = BuildOriginalDescription(conspiracy),
        ClickBonus = conspiracy.ClickBonus,
        MultiplierBonus = conspiracy.MultiplierBonus,
        TinfoilReward = conspiracy.TinfoilReward
    };

    private static string BuildOriginalDescription(Conspiracy conspiracy)
    {
        var parts = new List<string>();
        if (conspiracy.ClickBonus > 0) parts.Add($"+{conspiracy.ClickBonus:0.##} click power");
        if (conspiracy.MultiplierBonus > 1.0) parts.Add($"x{conspiracy.MultiplierBonus:0.##} evidence");
        if (conspiracy.TinfoilReward > 0) parts.Add($"+{conspiracy.TinfoilReward} tinfoil");
        return parts.Count > 0 ? string.Join(", ", parts) : "No bonus";
    }

    private static ConspiracyReward BuildAlternative(Conspiracy conspiracy, int index) => (index % 5) switch
    {
        0 => new ConspiracyReward
        {
            Id = OptionB,
            Name = "Recruitment Drive",
            Description = "x1.5 believers from every source",
            BelieverMultiplier = 1.5,
            MultiplierBonus = conspiracy.MultiplierBonus,
            TinfoilReward = conspiracy.TinfoilReward
        },
        1 => new ConspiracyReward
        {
            Id = OptionB,
            Name = "Sell The Documentary",
            Description = $"+{conspiracy.TinfoilReward * 6} tinfoil instead of the click bonus",
            TinfoilReward = conspiracy.TinfoilReward * 6,
            MultiplierBonus = conspiracy.MultiplierBonus
        },
        2 => new ConspiracyReward
        {
            Id = OptionB,
            Name = "Pattern Recognition",
            Description = "+4% critical hit chance",
            CritChanceBonus = 0.04,
            MultiplierBonus = conspiracy.MultiplierBonus,
            TinfoilReward = conspiracy.TinfoilReward
        },
        3 => new ConspiracyReward
        {
            Id = OptionB,
            Name = "Activate The Cells",
            Description = "Quests resolve 20% faster",
            QuestSpeedMultiplier = 0.8,
            MultiplierBonus = conspiracy.MultiplierBonus,
            TinfoilReward = conspiracy.TinfoilReward
        },
        _ => new ConspiracyReward
        {
            Id = OptionB,
            Name = "Bury The Lede",
            Description = $"x{conspiracy.MultiplierBonus + 0.25:0.##} evidence instead of the click bonus",
            MultiplierBonus = conspiracy.MultiplierBonus + 0.25,
            TinfoilReward = conspiracy.TinfoilReward
        }
    };

    public static (ConspiracyReward a, ConspiracyReward b) GetOptions(string conspiracyId) =>
        Rewards.TryGetValue(conspiracyId, out var pair) ? pair : (Empty, Empty);

    /// <summary>
    /// The reward a player actually took. Anything proven before the fork existed - or proven
    /// without a recorded choice - resolves to option A, which is the original behaviour.
    /// </summary>
    public static ConspiracyReward Resolve(string conspiracyId, IReadOnlyDictionary<string, string> choices)
    {
        var (a, b) = GetOptions(conspiracyId);
        return choices.TryGetValue(conspiracyId, out var picked) && picked == OptionB ? b : a;
    }

    private static readonly ConspiracyReward Empty = new()
    {
        Id = OptionA,
        Name = "Unknown",
        Description = "No bonus"
    };
}
