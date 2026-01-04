using ConspiracyClicker.Utils;

namespace ConspiracyClicker.Data;

public enum TinfoilUpgradeType
{
    ClickPower,
    EpsMultiplier,
    QuestSuccess,
    BelieverBonus,
    AutoClicker,
    CriticalChance,
    AutoQuest
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
            Icon = IconHelper.TinfoilShop.HatBasic
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
            Icon = IconHelper.TinfoilShop.HatReinforced
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
            Icon = IconHelper.TinfoilShop.Crown
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
            Icon = IconHelper.TinfoilShop.DeepStateContact
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
            Icon = IconHelper.TinfoilShop.DeepStateInsider
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
            Icon = IconHelper.TinfoilShop.DeepStateOperative
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
            Icon = IconHelper.TinfoilShop.RabbitFoot
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
            Icon = IconHelper.TinfoilShop.FourLeafClover
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
            Icon = IconHelper.TinfoilShop.Horseshoe
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
            Icon = IconHelper.TinfoilShop.Charisma
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
            Icon = IconHelper.TinfoilShop.CultLeadership
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
            Icon = IconHelper.TinfoilShop.Hypnosis
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
            Icon = IconHelper.TinfoilShop.ClickingIntern
        },
        new TinfoilUpgrade
        {
            Id = "clicking_robot",
            Name = "Clicking Robot",
            Description = "Auto-clicks 2x per second",
            FlavorText = "Beep boop. Click click.",
            TinfoilCost = 200,
            Type = TinfoilUpgradeType.AutoClicker,
            Value = 2,
            Icon = IconHelper.TinfoilShop.ClickingRobot
        },
        new TinfoilUpgrade
        {
            Id = "quantum_clicker",
            Name = "Quantum Clicker",
            Description = "Auto-clicks 5x per second",
            FlavorText = "Clicks in multiple dimensions simultaneously.",
            TinfoilCost = 1000,
            Type = TinfoilUpgradeType.AutoClicker,
            Value = 5,
            Icon = IconHelper.TinfoilShop.QuantumClicker
        },

        // Auto Quest (unlocks when you have 8+ auto CPS)
        new TinfoilUpgrade
        {
            Id = "quest_autopilot",
            Name = "Quest Autopilot",
            Description = "Automatically starts available quests",
            FlavorText = "Why click when the machine can conspire for you?",
            TinfoilCost = 100,
            Type = TinfoilUpgradeType.AutoQuest,
            Value = 1,
            Icon = IconHelper.TinfoilShop.QuantumClicker
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
            Icon = IconHelper.TinfoilShop.LuckyStar
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
            Icon = IconHelper.TinfoilShop.LuckyStar
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
            Icon = IconHelper.TinfoilShop.PropheticVision
        },

        // === ADVANCED TIER ===
        new TinfoilUpgrade
        {
            Id = "tinfoil_bodysuit",
            Name = "Tinfoil Bodysuit",
            Description = "+200% click power permanently",
            FlavorText = "Full-body protection. Very crinkly.",
            TinfoilCost = 800,
            Type = TinfoilUpgradeType.ClickPower,
            Value = 3.0,
            Icon = IconHelper.TinfoilShop.Bodysuit
        },
        new TinfoilUpgrade
        {
            Id = "tinfoil_bunker",
            Name = "Tinfoil-Lined Bunker",
            Description = "+400% click power permanently",
            FlavorText = "The ultimate safe room.",
            TinfoilCost = 5000,
            Type = TinfoilUpgradeType.ClickPower,
            Value = 5.0,
            Icon = IconHelper.TinfoilShop.Bunker
        },
        new TinfoilUpgrade
        {
            Id = "deep_state_director",
            Name = "Deep State Director",
            Description = "x2 evidence per second",
            FlavorText = "You run the meetings now.",
            TinfoilCost = 2500,
            Type = TinfoilUpgradeType.EpsMultiplier,
            Value = 2.0,
            Icon = IconHelper.TinfoilShop.DeepStateOperative
        },
        new TinfoilUpgrade
        {
            Id = "shadow_council_seat",
            Name = "Shadow Council Seat",
            Description = "x3 evidence per second",
            FlavorText = "Your vote shapes reality.",
            TinfoilCost = 15000,
            Type = TinfoilUpgradeType.EpsMultiplier,
            Value = 3.0,
            Icon = IconHelper.TinfoilShop.DeepStateOperative
        },
        new TinfoilUpgrade
        {
            Id = "reality_anchor",
            Name = "Reality Anchor",
            Description = "+20% quest success rate",
            FlavorText = "Some things just go your way.",
            TinfoilCost = 1500,
            Type = TinfoilUpgradeType.QuestSuccess,
            Value = 0.20,
            Icon = IconHelper.TinfoilShop.Horseshoe
        },
        new TinfoilUpgrade
        {
            Id = "probability_manipulator",
            Name = "Probability Manipulator",
            Description = "+25% quest success rate",
            FlavorText = "The dice are loaded. In your favor.",
            TinfoilCost = 8000,
            Type = TinfoilUpgradeType.QuestSuccess,
            Value = 0.25,
            Icon = IconHelper.TinfoilShop.FateWeaver
        },
        new TinfoilUpgrade
        {
            Id = "mind_control_crown",
            Name = "Mind Control Crown",
            Description = "x3 believers from all sources",
            FlavorText = "They follow because they must.",
            TinfoilCost = 3000,
            Type = TinfoilUpgradeType.BelieverBonus,
            Value = 3.0,
            Icon = IconHelper.TinfoilShop.MindControlCrown
        },
        new TinfoilUpgrade
        {
            Id = "hivemind_beacon",
            Name = "Hivemind Beacon",
            Description = "x5 believers from all sources",
            FlavorText = "One mind. Billions of bodies.",
            TinfoilCost = 20000,
            Type = TinfoilUpgradeType.BelieverBonus,
            Value = 5.0,
            Icon = IconHelper.TinfoilShop.MindControlCrown
        },
        new TinfoilUpgrade
        {
            Id = "neural_auto_clicker",
            Name = "Neural Interface Clicker",
            Description = "Auto-clicks 10x per second",
            FlavorText = "Think clicks. Get clicks.",
            TinfoilCost = 5000,
            Type = TinfoilUpgradeType.AutoClicker,
            Value = 10,
            Icon = IconHelper.TinfoilShop.NeuralClicker
        },
        new TinfoilUpgrade
        {
            Id = "reality_clicker",
            Name = "Reality Bending Clicker",
            Description = "Auto-clicks 15x per second",
            FlavorText = "Clicks from every possible timeline.",
            TinfoilCost = 25000,
            Type = TinfoilUpgradeType.AutoClicker,
            Value = 15,
            Icon = IconHelper.TinfoilShop.QuantumClicker
        },
        new TinfoilUpgrade
        {
            Id = "third_eye_open",
            Name = "Third Eye Fully Open",
            Description = "+15% critical chance",
            FlavorText = "See the matrix. Exploit the matrix.",
            TinfoilCost = 4000,
            Type = TinfoilUpgradeType.CriticalChance,
            Value = 0.15,
            Icon = IconHelper.TinfoilShop.ThirdEyeOpen
        },
        new TinfoilUpgrade
        {
            Id = "omniscient_clicking",
            Name = "Omniscient Clicking",
            Description = "+20% critical chance (50%+ total)",
            FlavorText = "Every click is the right click.",
            TinfoilCost = 30000,
            Type = TinfoilUpgradeType.CriticalChance,
            Value = 0.20,
            Icon = IconHelper.TinfoilShop.ThirdEyeOpen
        },

        // === ELITE TIER ===
        new TinfoilUpgrade
        {
            Id = "reality_distortion_aura",
            Name = "Reality Distortion Aura",
            Description = "x10 click power permanently",
            FlavorText = "Truth bends around you.",
            TinfoilCost = 50000,
            Type = TinfoilUpgradeType.ClickPower,
            Value = 10.0,
            Icon = IconHelper.TinfoilShop.RealityDistortion
        },
        new TinfoilUpgrade
        {
            Id = "illuminati_chairman",
            Name = "Illuminati Chairman",
            Description = "x5 evidence per second",
            FlavorText = "The all-seeing eye sees you... as its leader.",
            TinfoilCost = 100000,
            Type = TinfoilUpgradeType.EpsMultiplier,
            Value = 5.0,
            Icon = IconHelper.TinfoilShop.DeepStateOperative
        },
        new TinfoilUpgrade
        {
            Id = "fate_weaver",
            Name = "Fate Weaver",
            Description = "+50% quest success rate",
            FlavorText = "You don't follow destiny. Destiny follows you.",
            TinfoilCost = 75000,
            Type = TinfoilUpgradeType.QuestSuccess,
            Value = 0.5,
            Icon = IconHelper.TinfoilShop.FateWeaver
        },
        new TinfoilUpgrade
        {
            Id = "reality_clicker_supreme",
            Name = "Infinite Click Engine",
            Description = "Auto-clicks 17x per second",
            FlavorText = "Clicking transcends time and space.",
            TinfoilCost = 150000,
            Type = TinfoilUpgradeType.AutoClicker,
            Value = 17,
            Icon = IconHelper.TinfoilShop.QuantumClicker
        },

        // === TRANSCENDENT TIER ===
        new TinfoilUpgrade
        {
            Id = "cosmic_click_aura",
            Name = "Cosmic Click Aura",
            Description = "x25 click power permanently",
            FlavorText = "Your finger contains multitudes.",
            TinfoilCost = 250000,
            Type = TinfoilUpgradeType.ClickPower,
            Value = 25.0,
            Icon = "cosmic_click_aura"
        },
        new TinfoilUpgrade
        {
            Id = "multiverse_clicker",
            Name = "Multiverse Clicker Array",
            Description = "Auto-clicks 50x per second",
            FlavorText = "Every version of you is clicking.",
            TinfoilCost = 500000,
            Type = TinfoilUpgradeType.AutoClicker,
            Value = 50,
            Icon = "multiverse_clicker"
        },
        new TinfoilUpgrade
        {
            Id = "reality_architect",
            Name = "Reality Architect Status",
            Description = "x10 evidence per second",
            FlavorText = "You don't find truth. You build it.",
            TinfoilCost = 400000,
            Type = TinfoilUpgradeType.EpsMultiplier,
            Value = 10.0,
            Icon = "reality_architect"
        },
        new TinfoilUpgrade
        {
            Id = "probability_engine",
            Name = "Probability Engine",
            Description = "+75% quest success rate (cap 100%)",
            FlavorText = "Failure is no longer an option.",
            TinfoilCost = 350000,
            Type = TinfoilUpgradeType.QuestSuccess,
            Value = 0.75,
            Icon = "probability_engine"
        },
        new TinfoilUpgrade
        {
            Id = "consciousness_amplifier",
            Name = "Consciousness Amplifier",
            Description = "x10 believers from all sources",
            FlavorText = "Wake up. Literally everyone.",
            TinfoilCost = 300000,
            Type = TinfoilUpgradeType.BelieverBonus,
            Value = 10.0,
            Icon = "consciousness_amplifier"
        },
        new TinfoilUpgrade
        {
            Id = "omniscient_critical",
            Name = "Omniscient Critical System",
            Description = "+35% critical chance (85%+ total)",
            FlavorText = "You see ALL the weaknesses.",
            TinfoilCost = 200000,
            Type = TinfoilUpgradeType.CriticalChance,
            Value = 0.35,
            Icon = "omniscient_critical"
        },

        // === OMEGA TIER ===
        new TinfoilUpgrade
        {
            Id = "click_singularity",
            Name = "Click Singularity",
            Description = "x100 click power permanently",
            FlavorText = "One click to rule them all.",
            TinfoilCost = 1000000,
            Type = TinfoilUpgradeType.ClickPower,
            Value = 100.0,
            Icon = "click_singularity"
        },
        new TinfoilUpgrade
        {
            Id = "infinite_clicker_array",
            Name = "Infinite Clicker Array",
            Description = "Auto-clicks 100x per second",
            FlavorText = "Infinite clicks. Finite patience.",
            TinfoilCost = 2000000,
            Type = TinfoilUpgradeType.AutoClicker,
            Value = 100,
            Icon = "infinite_clicker_array"
        },
        new TinfoilUpgrade
        {
            Id = "truth_fabricator",
            Name = "Truth Fabricator",
            Description = "x25 evidence per second",
            FlavorText = "Why discover truth when you can manufacture it?",
            TinfoilCost = 1500000,
            Type = TinfoilUpgradeType.EpsMultiplier,
            Value = 25.0,
            Icon = "truth_fabricator"
        },
        new TinfoilUpgrade
        {
            Id = "destiny_manipulator",
            Name = "Destiny Manipulator",
            Description = "Quests complete instantly, always succeed",
            FlavorText = "The future does what you tell it.",
            TinfoilCost = 2500000,
            Type = TinfoilUpgradeType.QuestSuccess,
            Value = 1.0,
            Icon = "destiny_manipulator"
        },
        new TinfoilUpgrade
        {
            Id = "hivemind_omega",
            Name = "Omega Hivemind",
            Description = "x50 believers from all sources",
            FlavorText = "Every consciousness joins. Resistance is illogical.",
            TinfoilCost = 1800000,
            Type = TinfoilUpgradeType.BelieverBonus,
            Value = 50.0,
            Icon = "hivemind_omega"
        },
        new TinfoilUpgrade
        {
            Id = "perfect_critical",
            Name = "Perfect Critical Mastery",
            Description = "100% critical chance, crits deal 20x damage",
            FlavorText = "Every. Single. Click. Is. Critical.",
            TinfoilCost = 3000000,
            Type = TinfoilUpgradeType.CriticalChance,
            Value = 1.0,
            Icon = "perfect_critical"
        },

        // === FINAL TIER ===
        new TinfoilUpgrade
        {
            Id = "omega_evidence_engine",
            Name = "Omega Evidence Engine",
            Description = "x100 evidence per second permanently",
            FlavorText = "The truth machine runs at full power.",
            TinfoilCost = 10000000,
            Type = TinfoilUpgradeType.EpsMultiplier,
            Value = 100.0,
            Icon = "omega_evidence_engine"
        },
        new TinfoilUpgrade
        {
            Id = "cosmic_click_engine",
            Name = "Cosmic Click Engine",
            Description = "x500 click power, +200 auto-clicks/sec",
            FlavorText = "The universe clicks in harmony with you.",
            TinfoilCost = 15000000,
            Type = TinfoilUpgradeType.ClickPower,
            Value = 500.0,
            Icon = "cosmic_click_engine"
        },
        new TinfoilUpgrade
        {
            Id = "final_believer_ascension",
            Name = "Final Believer Ascension",
            Description = "x200 believers from all sources",
            FlavorText = "All minds. One truth. Yours.",
            TinfoilCost = 20000000,
            Type = TinfoilUpgradeType.BelieverBonus,
            Value = 200.0,
            Icon = "final_believer_ascension"
        }
    };

    public static TinfoilUpgrade? GetById(string id) => AllUpgrades.FirstOrDefault(u => u.Id == id);
}
