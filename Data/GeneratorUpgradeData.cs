using ConspiracyClicker.Models;

namespace ConspiracyClicker.Data;

public static class GeneratorUpgradeData
{
    // Global bonus types rotate through generators
    private static readonly GeneratorUpgradeType[] GlobalBonusTypes = new[]
    {
        GeneratorUpgradeType.GlobalClickPower,
        GeneratorUpgradeType.GlobalQuestSpeed,
        GeneratorUpgradeType.GlobalTinfoilGain,
        GeneratorUpgradeType.GlobalCritChance,
        GeneratorUpgradeType.GlobalCritDamage,
        GeneratorUpgradeType.GlobalGoldenEye,
        GeneratorUpgradeType.GlobalBelieverGain,
        GeneratorUpgradeType.GlobalEpsMultiplier
    };

    private static readonly string[] GlobalBonusNames = new[]
    {
        "Click Mastery",
        "Swift Operations",
        "Foil Collector",
        "Lucky Strike",
        "Critical Focus",
        "Golden Vision",
        "Magnetic Personality",
        "Evidence Amplifier"
    };

    private static readonly string[] GlobalBonusDescriptions = new[]
    {
        "+200% click power",
        "-50% quest duration",
        "+100% tinfoil from all sources",
        "+15% critical hit chance",
        "+200% critical hit damage",
        "Golden Eyes appear 100% more often",
        "+100% believers from all sources",
        "+50% evidence per second"
    };

    private static readonly double[] GlobalBonusValues = new[]
    {
        3.0,   // Click power multiplier (3x = +200%)
        0.50,  // Quest duration multiplier (50% = -50% duration)
        2.0,   // Tinfoil multiplier (2x = +100%)
        0.15,  // Crit chance flat bonus (+15%)
        3.0,   // Crit damage multiplier (3x = +200%)
        2.0,   // Golden eye frequency multiplier (2x = +100%)
        2.0,   // Believer multiplier (2x = +100%)
        1.50   // EPS multiplier (1.5x = +50%)
    };

    public static readonly List<GeneratorUpgrade> AllUpgrades;

    static GeneratorUpgradeData()
    {
        AllUpgrades = new List<GeneratorUpgrade>();
        int genIndex = 0;

        foreach (var generator in GeneratorData.AllGenerators)
        {
            // Level 25: Production x10
            AllUpgrades.Add(new GeneratorUpgrade
            {
                Id = $"{generator.Id}_lvl25",
                GeneratorId = generator.Id,
                Name = $"{generator.Name} Boost I",
                Description = "x10 production from this generator",
                UnlockLevel = 25,
                Type = GeneratorUpgradeType.ProductionMultiplier,
                Value = 10.0,
                Icon = "⬆"
            });

            // Level 50: Global bonus (rotates)
            int globalIndex = genIndex % GlobalBonusTypes.Length;
            AllUpgrades.Add(new GeneratorUpgrade
            {
                Id = $"{generator.Id}_lvl50",
                GeneratorId = generator.Id,
                Name = GlobalBonusNames[globalIndex],
                Description = GlobalBonusDescriptions[globalIndex],
                UnlockLevel = 50,
                Type = GlobalBonusTypes[globalIndex],
                Value = GlobalBonusValues[globalIndex],
                Icon = GetGlobalIcon(GlobalBonusTypes[globalIndex])
            });

            // Level 75: Believer bonus (replaces cost reduction)
            AllUpgrades.Add(new GeneratorUpgrade
            {
                Id = $"{generator.Id}_lvl75",
                GeneratorId = generator.Id,
                Name = $"{generator.Name} Recruitment",
                Description = "x2 believers from this generator",
                UnlockLevel = 75,
                Type = GeneratorUpgradeType.BelieverBonus,
                Value = 2.0,
                Icon = "👥"
            });

            // Level 100: Production x10 (reduced from x50)
            AllUpgrades.Add(new GeneratorUpgrade
            {
                Id = $"{generator.Id}_lvl100",
                GeneratorId = generator.Id,
                Name = $"{generator.Name} Boost II",
                Description = "x10 production from this generator",
                UnlockLevel = 100,
                Type = GeneratorUpgradeType.ProductionMultiplier,
                Value = 10.0,
                Icon = "⬆⬆"
            });

            // Level 150: Global bonus (second one, offset)
            int globalIndex2 = (genIndex + 4) % GlobalBonusTypes.Length;
            AllUpgrades.Add(new GeneratorUpgrade
            {
                Id = $"{generator.Id}_lvl150",
                GeneratorId = generator.Id,
                Name = $"Advanced {GlobalBonusNames[globalIndex2]}",
                Description = GlobalBonusDescriptions[globalIndex2],
                UnlockLevel = 150,
                Type = GlobalBonusTypes[globalIndex2],
                Value = GlobalBonusValues[globalIndex2],
                Icon = GetGlobalIcon(GlobalBonusTypes[globalIndex2])
            });

            // Level 200: Believer bonus (second one)
            AllUpgrades.Add(new GeneratorUpgrade
            {
                Id = $"{generator.Id}_lvl200",
                GeneratorId = generator.Id,
                Name = $"{generator.Name} Evangelism",
                Description = "x3 believers from this generator",
                UnlockLevel = 200,
                Type = GeneratorUpgradeType.BelieverBonus,
                Value = 3.0,
                Icon = "👥"
            });

            // Level 250: Production x250 (mastery boost)
            AllUpgrades.Add(new GeneratorUpgrade
            {
                Id = $"{generator.Id}_lvl250",
                GeneratorId = generator.Id,
                Name = $"{generator.Name} Mastery",
                Description = "x250 production from this generator",
                UnlockLevel = 250,
                Type = GeneratorUpgradeType.ProductionMultiplier,
                Value = 250.0,
                Icon = "👑"
            });

            genIndex++;
        }
    }

    private static string GetGlobalIcon(GeneratorUpgradeType type) => type switch
    {
        GeneratorUpgradeType.GlobalClickPower => "👆",
        GeneratorUpgradeType.GlobalQuestSpeed => "⚡",
        GeneratorUpgradeType.GlobalTinfoilGain => "◇",
        GeneratorUpgradeType.GlobalCritChance => "🎯",
        GeneratorUpgradeType.GlobalCritDamage => "💥",
        GeneratorUpgradeType.GlobalGoldenEye => "👁",
        GeneratorUpgradeType.GlobalBelieverGain => "👥",
        GeneratorUpgradeType.GlobalEpsMultiplier => "📈",
        _ => "★"
    };

    public static IEnumerable<GeneratorUpgrade> GetUpgradesForGenerator(string generatorId)
    {
        return AllUpgrades.Where(u => u.GeneratorId == generatorId).OrderBy(u => u.UnlockLevel);
    }

    public static GeneratorUpgrade? GetById(string id)
    {
        return AllUpgrades.FirstOrDefault(u => u.Id == id);
    }

    public static IEnumerable<GeneratorUpgrade> GetAvailableUpgrades(string generatorId, int level, HashSet<string> purchased)
    {
        return AllUpgrades.Where(u =>
            u.GeneratorId == generatorId &&
            level >= u.UnlockLevel &&
            !purchased.Contains(u.Id));
    }

    public static IEnumerable<GeneratorUpgrade> GetPurchasedUpgrades(string generatorId, HashSet<string> purchased)
    {
        return AllUpgrades.Where(u =>
            u.GeneratorId == generatorId &&
            purchased.Contains(u.Id));
    }
}
