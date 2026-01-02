using ConspiracyClicker.Models;

namespace ConspiracyClicker.Data;

public static class QuestData
{
    public static readonly List<Quest> AllQuests = new()
    {
        // === LOW RISK (Safe, small rewards) ===
        new Quest
        {
            Id = "recon_mission",
            Name = "Reconnaissance Mission",
            Description = "Survey suspicious locations and collect intel",
            FlavorText = "Eyes everywhere. Trust no one. Document everything.",
            Risk = QuestRisk.Low,
            BelieversRequired = 20,
            DurationSeconds = 120,
            SuccessChance = 0.90,
            EvidenceMultiplier = 100,
            TinfoilReward = 2,
            Icon = "🔍"
        },
        new Quest
        {
            Id = "document_recovery",
            Name = "Document Recovery",
            Description = "Recover and piece together shredded documents",
            FlavorText = "One man's trash is another man's truth bomb.",
            Risk = QuestRisk.Low,
            BelieversRequired = 100,
            DurationSeconds = 240,
            SuccessChance = 0.85,
            EvidenceMultiplier = 200,
            TinfoilReward = 6,
            Icon = "📄"
        },

        // === MEDIUM RISK (Moderate rewards, no believers lost) ===
        new Quest
        {
            Id = "signal_intercept",
            Name = "Signal Intercept Operation",
            Description = "Intercept and decode suspicious transmissions",
            FlavorText = "Those aren't just static. They're orders.",
            Risk = QuestRisk.Medium,
            BelieversRequired = 400,
            DurationSeconds = 480,
            SuccessChance = 0.65,
            EvidenceMultiplier = 600,
            TinfoilReward = 20,
            Icon = "📡"
        },
        new Quest
        {
            Id = "facility_infiltration",
            Name = "Facility Infiltration",
            Description = "Pose as workers to access restricted areas",
            FlavorText = "Clipboard and hard hat = access all areas.",
            Risk = QuestRisk.Medium,
            BelieversRequired = 1500,
            DurationSeconds = 720,
            SuccessChance = 0.55,
            EvidenceMultiplier = 1500,
            TinfoilReward = 40,
            Icon = "🏭"
        },

        // === HIGH RISK (Big rewards, can lose believers) ===
        new Quest
        {
            Id = "black_site_raid",
            Name = "Black Site Raid",
            Description = "Infiltrate a hidden government installation",
            FlavorText = "Maps say nothing's there. That's how you know something is.",
            Risk = QuestRisk.High,
            BelieversRequired = 5000,
            DurationSeconds = 1200,
            SuccessChance = 0.38,
            EvidenceMultiplier = 8000,
            TinfoilReward = 150,
            Icon = "🏴"
        },
        new Quest
        {
            Id = "shadow_council",
            Name = "Shadow Council Infiltration",
            Description = "Crash an elite secret society gathering",
            FlavorText = "Dress code: Robes, owl masks, and unshakable conviction.",
            Risk = QuestRisk.High,
            BelieversRequired = 50000,
            DurationSeconds = 1500,
            SuccessChance = 0.18,
            EvidenceMultiplier = 100000,
            TinfoilReward = 1000,
            Icon = "🦉"
        },

        // === EXTREME RISK (Massive rewards, high believer requirement) ===
        new Quest
        {
            Id = "underground_network",
            Name = "Underground Network Takeover",
            Description = "Seize control of a secret tunnel network",
            FlavorText = "There's a reason they call them 'underground' movements.",
            Risk = QuestRisk.High,
            BelieversRequired = 150000,
            DurationSeconds = 1500,
            SuccessChance = 0.15,
            EvidenceMultiplier = 500000,
            TinfoilReward = 3000,
            Icon = "🚇"
        },
        new Quest
        {
            Id = "satellite_hijack",
            Name = "Satellite Hijacking",
            Description = "Take control of a 'decommissioned' spy satellite",
            FlavorText = "It's not stealing if they said it doesn't exist.",
            Risk = QuestRisk.High,
            BelieversRequired = 500000,
            DurationSeconds = 1500,
            SuccessChance = 0.12,
            EvidenceMultiplier = 2000000,
            TinfoilReward = 8000,
            Icon = "🛸"
        },
        new Quest
        {
            Id = "deep_state_mole",
            Name = "Deep State Mole Operation",
            Description = "Plant believers inside the shadow government itself",
            FlavorText = "Become the very thing you swore to expose. For research.",
            Risk = QuestRisk.High,
            BelieversRequired = 1500000,
            DurationSeconds = 1500,
            SuccessChance = 0.08,
            EvidenceMultiplier = 10000000,
            TinfoilReward = 25000,
            Icon = "🕴️"
        },
        new Quest
        {
            Id = "reality_breach",
            Name = "Reality Breach Expedition",
            Description = "Send believers through a dimensional rift",
            FlavorText = "They'll either return with proof or become proof themselves.",
            Risk = QuestRisk.High,
            BelieversRequired = 5000000,
            DurationSeconds = 1500,
            SuccessChance = 0.05,
            EvidenceMultiplier = 50000000,
            TinfoilReward = 100000,
            Icon = "🌀"
        }
    };

    public static Quest? GetById(string id) => AllQuests.FirstOrDefault(q => q.Id == id);

    public static IEnumerable<Quest> GetAvailable(double believers)
    {
        return AllQuests.Where(q => believers >= q.BelieversRequired);
    }
}
