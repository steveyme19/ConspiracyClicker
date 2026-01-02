using ConspiracyClicker.Data;
using ConspiracyClicker.Models;

namespace ConspiracyClicker.Core;

public class GameState
{
    // Primary resources
    public double Evidence { get; set; }
    public double TotalEvidenceEarned { get; set; }
    public double Believers { get; set; }
    public double BusyBelievers { get; set; } // On quests
    public int Tinfoil { get; set; }

    // Prestige currencies
    public int IlluminatiTokens { get; set; }
    public int GlitchTokens { get; set; }

    // Statistics
    public long TotalClicks { get; set; }
    public double TotalPlayTimeSeconds { get; set; }
    public DateTime LastSaveTime { get; set; } = DateTime.Now;
    public int QuestsCompleted { get; set; }
    public int QuestsFailed { get; set; }
    public double BelieversLost { get; set; }

    // Owned generators: GeneratorId -> Count
    public Dictionary<string, int> Generators { get; set; } = new();

    // Purchased upgrades
    public HashSet<string> PurchasedUpgrades { get; set; } = new();

    // Proven conspiracies
    public HashSet<string> ProvenConspiracies { get; set; } = new();

    // Achievements
    public HashSet<string> UnlockedAchievements { get; set; } = new();

    // Tinfoil Shop purchases
    public HashSet<string> TinfoilShopPurchases { get; set; } = new();

    // Critical hit stats
    public long CriticalClicks { get; set; }

    // Prestige upgrades
    public HashSet<string> IlluminatiUpgrades { get; set; } = new();
    public HashSet<string> MatrixUpgrades { get; set; } = new();

    // Prestige stats
    public int TimesAscended { get; set; }
    public int TimesMatrixBroken { get; set; }
    public double TotalIlluminatiTokensEarned { get; set; }

    // Skill tree
    public HashSet<string> UnlockedSkills { get; set; } = new();
    public int SkillPoints { get; set; }

    // Daily challenges - stores full challenge data
    public DateTime LastDailyChallengeDate { get; set; } = DateTime.MinValue;
    public List<StoredChallenge> DailyChallenges { get; set; } = new();

    // Daily challenge tracking stats
    public int TodayClicks { get; set; }
    public double TodayEvidence { get; set; }
    public int TodayQuestsCompleted { get; set; }
    public int TodayCriticalHits { get; set; }
    public int TodayCombos { get; set; }

    // Active quests
    public List<ActiveQuest> ActiveQuests { get; set; } = new();

    // Combo meter (0.0 to 1.0)
    public double ComboMeter { get; set; }
    public DateTime LastClickTime { get; set; } = DateTime.MinValue;
    public int ComboClicks { get; set; }

    // Active events
    public bool GoldenEyeActive { get; set; }
    public DateTime GoldenEyeEndTime { get; set; }
    public bool WhistleBlowerActive { get; set; }
    public DateTime WhistleBlowerEndTime { get; set; }
    public double WhistleBlowerX { get; set; }
    public double WhistleBlowerY { get; set; }

    // Challenge Mode
    public string? ActiveChallengeId { get; set; }
    public DateTime ChallengeStartTime { get; set; }
    public double ChallengeProgress { get; set; }
    public int ChallengeClickCount { get; set; }
    public int ChallengeHighRiskQuestsCompleted { get; set; }
    public HashSet<string> CompletedChallenges { get; set; } = new();

    public double AvailableBelievers => Math.Max(0, Believers - BusyBelievers);

    public int GetGeneratorCount(string id)
    {
        return Generators.TryGetValue(id, out var count) ? count : 0;
    }

    public void AddGenerator(string id, int amount = 1)
    {
        if (!Generators.ContainsKey(id))
            Generators[id] = 0;
        Generators[id] += amount;
    }
}
