namespace ConspiracyClicker.Data;

public enum ChallengeType
{
    CollectEvidence,
    ClickCount,
    CompleteQuests,
    CriticalHits,
    ComboCount
}

// Stored in GameState - contains all data needed
public class StoredChallenge
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required ChallengeType Type { get; init; }
    public required double Target { get; init; }
    public required int TinfoilReward { get; init; }
    public double Progress { get; set; }
    public bool Completed { get; set; }
    public bool Claimed { get; set; }
}

public static class DailyChallengeData
{
    // Static challenge templates with fixed targets
    private static readonly List<(string id, string name, string desc, ChallengeType type, double target, int reward)> ChallengeTemplates = new()
    {
        ("clicks_50", "Warm Up", "Click 50 times", ChallengeType.ClickCount, 50, 2),
        ("clicks_200", "Clicker", "Click 200 times", ChallengeType.ClickCount, 200, 4),
        ("clicks_500", "Click Enthusiast", "Click 500 times", ChallengeType.ClickCount, 500, 8),
        ("clicks_1000", "Click Frenzy", "Click 1000 times", ChallengeType.ClickCount, 1000, 15),

        ("crits_5", "Lucky Strikes", "Land 5 critical hits", ChallengeType.CriticalHits, 5, 3),
        ("crits_15", "Critical Thinker", "Land 15 critical hits", ChallengeType.CriticalHits, 15, 7),
        ("crits_30", "Precision Master", "Land 30 critical hits", ChallengeType.CriticalHits, 30, 12),

        ("combos_2", "Combo Starter", "Trigger 2 combo bursts", ChallengeType.ComboCount, 2, 4),
        ("combos_5", "Combo King", "Trigger 5 combo bursts", ChallengeType.ComboCount, 5, 8),
        ("combos_10", "Combo Legend", "Trigger 10 combo bursts", ChallengeType.ComboCount, 10, 15),

        ("quests_1", "Quest Beginner", "Complete 1 quest", ChallengeType.CompleteQuests, 1, 5),
        ("quests_2", "Quest Master", "Complete 2 quests", ChallengeType.CompleteQuests, 2, 10),
    };

    /// <summary>
    /// Generate daily challenges using date as seed for consistency
    /// </summary>
    public static List<StoredChallenge> GenerateDailyChallenges(DateTime date)
    {
        // Use date as seed so same day = same challenges
        int seed = date.Year * 10000 + date.Month * 100 + date.Day;
        var seededRandom = new Random(seed);

        // Shuffle and pick 3
        var shuffled = ChallengeTemplates
            .OrderBy(_ => seededRandom.Next())
            .Take(3)
            .Select(t => new StoredChallenge
            {
                Id = t.id,
                Name = t.name,
                Description = t.desc,
                Type = t.type,
                Target = t.target,
                TinfoilReward = t.reward,
                Progress = 0,
                Completed = false,
                Claimed = false
            })
            .ToList();

        return shuffled;
    }
}
