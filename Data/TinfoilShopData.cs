namespace ConspiracyClicker.Data;

public enum TinfoilUpgradeType
{
    ClickPower,
    EpsMultiplier,
    QuestSuccess,
    BelieverBonus,
    AutoClicker,
    CriticalChance
}

public class TinfoilUpgrade
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required string FlavorText { get; init; }
    public required int TinfoilCost { get; init; }
    public required TinfoilUpgradeType Type { get; init; }
    public required double Value { get; init; }
    public string Icon { get; init; } = "◆";
}

public static class TinfoilShopData
{
    public static readonly List<TinfoilUpgrade> AllUpgrades = new()
    {
        // Click Power
        new TinfoilUpgrade
        {
            Id = "tinfoil_hat_basic",
            Name = "Basic Tinfoil Hat",
            Description = "+25% click power permanently",
            FlavorText = "Standard protection against mind control rays.",
            TinfoilCost = 10,
            Type = TinfoilUpgradeType.ClickPower,
            Value = 1.25,
            Icon = "◠"
        },
        new TinfoilUpgrade
        {
            Id = "tinfoil_hat_reinforced",
            Name = "Reinforced Tinfoil Hat",
            Description = "+50% click power permanently",
            FlavorText = "Double-layered for extra protection.",
            TinfoilCost = 50,
            Type = TinfoilUpgradeType.ClickPower,
            Value = 1.50,
            Icon = "◠◠"
        },
        new TinfoilUpgrade
        {
            Id = "tinfoil_hat_deluxe",
            Name = "Deluxe Tinfoil Crown",
            Description = "+100% click power permanently",
            FlavorText = "Fit for a conspiracy king.",
            TinfoilCost = 200,
            Type = TinfoilUpgradeType.ClickPower,
            Value = 2.0,
            Icon = "♔"
        },

        // EPS Multiplier
        new TinfoilUpgrade
        {
            Id = "deep_state_contact",
            Name = "Deep State Contact",
            Description = "+15% evidence per second",
            FlavorText = "You know a guy who knows a guy.",
            TinfoilCost = 25,
            Type = TinfoilUpgradeType.EpsMultiplier,
            Value = 1.15,
            Icon = "◬"
        },
        new TinfoilUpgrade
        {
            Id = "deep_state_insider",
            Name = "Deep State Insider",
            Description = "+35% evidence per second",
            FlavorText = "They invited you to the secret meetings.",
            TinfoilCost = 100,
            Type = TinfoilUpgradeType.EpsMultiplier,
            Value = 1.35,
            Icon = "◬◬"
        },
        new TinfoilUpgrade
        {
            Id = "deep_state_operative",
            Name = "Deep State Operative",
            Description = "+75% evidence per second",
            FlavorText = "You ARE the deep state now.",
            TinfoilCost = 500,
            Type = TinfoilUpgradeType.EpsMultiplier,
            Value = 1.75,
            Icon = "◬◉◬"
        },

        // Quest Success
        new TinfoilUpgrade
        {
            Id = "lucky_rabbit_foot",
            Name = "Lucky Rabbit's Foot",
            Description = "+5% quest success rate",
            FlavorText = "Not so lucky for the rabbit.",
            TinfoilCost = 15,
            Type = TinfoilUpgradeType.QuestSuccess,
            Value = 0.05,
            Icon = "♣"
        },
        new TinfoilUpgrade
        {
            Id = "four_leaf_clover",
            Name = "Four-Leaf Clover",
            Description = "+10% quest success rate",
            FlavorText = "Genetically modified for extra luck.",
            TinfoilCost = 75,
            Type = TinfoilUpgradeType.QuestSuccess,
            Value = 0.10,
            Icon = "♧"
        },
        new TinfoilUpgrade
        {
            Id = "lucky_horseshoe",
            Name = "Lucky Horseshoe",
            Description = "+15% quest success rate",
            FlavorText = "From a horse that won the Kentucky Derby. Probably.",
            TinfoilCost = 250,
            Type = TinfoilUpgradeType.QuestSuccess,
            Value = 0.15,
            Icon = "Ω"
        },

        // Believer Multiplier (percentage bonus)
        new TinfoilUpgrade
        {
            Id = "charisma_training",
            Name = "Charisma Training",
            Description = "+20% believers from all sources",
            FlavorText = "Learn the art of persuasion.",
            TinfoilCost = 30,
            Type = TinfoilUpgradeType.BelieverBonus,
            Value = 1.20,
            Icon = "☺"
        },
        new TinfoilUpgrade
        {
            Id = "cult_leadership",
            Name = "Cult Leadership 101",
            Description = "+40% believers from all sources",
            FlavorText = "Chapter 1: Robes are optional but encouraged.",
            TinfoilCost = 150,
            Type = TinfoilUpgradeType.BelieverBonus,
            Value = 1.40,
            Icon = "☺☺"
        },
        new TinfoilUpgrade
        {
            Id = "mass_hypnosis",
            Name = "Mass Hypnosis",
            Description = "+100% believers from all sources",
            FlavorText = "You're getting very sleepy... and very convinced.",
            TinfoilCost = 600,
            Type = TinfoilUpgradeType.BelieverBonus,
            Value = 2.0,
            Icon = "◎◎◎"
        },

        // Auto Clicker
        new TinfoilUpgrade
        {
            Id = "clicking_intern",
            Name = "Clicking Intern",
            Description = "Auto-clicks 1x per second",
            FlavorText = "Unpaid, of course. For the exposure.",
            TinfoilCost = 50,
            Type = TinfoilUpgradeType.AutoClicker,
            Value = 1,
            Icon = "☝"
        },
        new TinfoilUpgrade
        {
            Id = "clicking_robot",
            Name = "Clicking Robot",
            Description = "Auto-clicks 3x per second",
            FlavorText = "Beep boop. Click click.",
            TinfoilCost = 200,
            Type = TinfoilUpgradeType.AutoClicker,
            Value = 3,
            Icon = "⚙"
        },
        new TinfoilUpgrade
        {
            Id = "quantum_clicker",
            Name = "Quantum Clicker",
            Description = "Auto-clicks 10x per second",
            FlavorText = "Clicks in multiple dimensions simultaneously.",
            TinfoilCost = 1000,
            Type = TinfoilUpgradeType.AutoClicker,
            Value = 10,
            Icon = "◇◇◇"
        },

        // Critical Chance
        new TinfoilUpgrade
        {
            Id = "lucky_guess",
            Name = "Lucky Guess",
            Description = "5% chance for critical clicks (5x)",
            FlavorText = "Sometimes you just get lucky.",
            TinfoilCost = 40,
            Type = TinfoilUpgradeType.CriticalChance,
            Value = 0.05,
            Icon = "★"
        },
        new TinfoilUpgrade
        {
            Id = "educated_guess",
            Name = "Educated Guess",
            Description = "+5% critical chance (10% total)",
            FlavorText = "Knowledge is power. Critical power.",
            TinfoilCost = 175,
            Type = TinfoilUpgradeType.CriticalChance,
            Value = 0.05,
            Icon = "★★"
        },
        new TinfoilUpgrade
        {
            Id = "prophetic_vision",
            Name = "Prophetic Vision",
            Description = "+10% critical chance (20% total)",
            FlavorText = "You can see the truth before it happens.",
            TinfoilCost = 750,
            Type = TinfoilUpgradeType.CriticalChance,
            Value = 0.10,
            Icon = "★★★"
        }
    };

    public static TinfoilUpgrade? GetById(string id) => AllUpgrades.FirstOrDefault(u => u.Id == id);
}
