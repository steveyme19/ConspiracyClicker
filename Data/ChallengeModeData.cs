using ConspiracyClicker.Utils;

namespace ConspiracyClicker.Data;

public enum ChallengeModeType
{
    Speedrun,       // Reach X evidence in Y seconds
    NoClick,        // Only passive income
    Minimalist,     // Max 1 of each generator
    NoPrestige,     // Reach X evidence without prestige
    RiskyBusiness,  // Complete X high-risk quests
    ClickMaster     // Get X clicks in Y seconds
}

public class ChallengeMode
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required string Rules { get; init; }
    public required ChallengeModeType Type { get; init; }
    public required double TargetValue { get; init; }
    public double TimeLimit { get; init; } // In seconds, 0 = no limit
    public int TinfoilReward { get; init; }
    public int IlluminatiTokenReward { get; init; }
    public string Icon { get; init; } = "🏆";
}

public static class ChallengeModeData
{
    public static readonly List<ChallengeMode> AllChallenges = new()
    {
        new ChallengeMode
        {
            Id = "speedrun_1m",
            Name = "Speed Conspiracy",
            Description = "Reach 1 Million evidence in 10 minutes",
            Rules = "Timer starts when you begin. No prestige bonuses.",
            Type = ChallengeModeType.Speedrun,
            TargetValue = 1_000_000,
            TimeLimit = 600,
            TinfoilReward = 25,
            Icon = IconHelper.ChallengeModes.Speedrun
        },
        new ChallengeMode
        {
            Id = "speedrun_1b",
            Name = "Lightning Research",
            Description = "Reach 1 Billion evidence in 30 minutes",
            Rules = "Timer starts when you begin. No prestige bonuses.",
            Type = ChallengeModeType.Speedrun,
            TargetValue = 1_000_000_000,
            TimeLimit = 1800,
            TinfoilReward = 75,
            Icon = IconHelper.ChallengeModes.Lightning
        },
        new ChallengeMode
        {
            Id = "no_click",
            Name = "The Observer",
            Description = "Reach 100 Million evidence without clicking",
            Rules = "You cannot click the Eye. Only passive income allowed.",
            Type = ChallengeModeType.NoClick,
            TargetValue = 100_000_000,
            TinfoilReward = 50,
            Icon = IconHelper.ChallengeModes.Observer
        },
        new ChallengeMode
        {
            Id = "minimalist",
            Name = "The Minimalist",
            Description = "Reach 1 Billion evidence with max 1 of each generator",
            Rules = "You can only own 1 of each generator type.",
            Type = ChallengeModeType.Minimalist,
            TargetValue = 1_000_000_000,
            TinfoilReward = 100,
            Icon = IconHelper.ChallengeModes.Minimalist
        },
        new ChallengeMode
        {
            Id = "no_prestige",
            Name = "True Believer",
            Description = "Reach 1 Trillion evidence without prestiging",
            Rules = "Reach the prestige threshold without actually prestiging.",
            Type = ChallengeModeType.NoPrestige,
            TargetValue = 1_000_000_000_000,
            TinfoilReward = 150,
            IlluminatiTokenReward = 5,
            Icon = IconHelper.ChallengeModes.NoPrestige
        },
        new ChallengeMode
        {
            Id = "risky_business",
            Name = "High Roller",
            Description = "Complete 10 high-risk quests in a single run",
            Rules = "Complete high-risk quests. Failing resets count.",
            Type = ChallengeModeType.RiskyBusiness,
            TargetValue = 10,
            TinfoilReward = 80,
            Icon = IconHelper.ChallengeModes.Risky
        },
        new ChallengeMode
        {
            Id = "click_master",
            Name = "Click Frenzy Master",
            Description = "Click 500 times in 60 seconds",
            Rules = "Mash that eye! Only manual clicks count.",
            Type = ChallengeModeType.ClickMaster,
            TargetValue = 500,
            TimeLimit = 60,
            TinfoilReward = 30,
            Icon = IconHelper.ChallengeModes.ClickMaster
        }
    };

    public static ChallengeMode? GetById(string id) => AllChallenges.FirstOrDefault(c => c.Id == id);
}
