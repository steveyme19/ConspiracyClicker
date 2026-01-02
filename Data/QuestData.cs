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
        },

        // === POST-ASCENSION QUESTS ===
        new Quest
        {
            Id = "whistleblower_extraction",
            Name = "Whistleblower Extraction",
            Description = "Extract a key witness before they're silenced",
            FlavorText = "They know too much. We need them to know more.",
            Risk = QuestRisk.Medium,
            BelieversRequired = 250,
            DurationSeconds = 300,
            SuccessChance = 0.70,
            EvidenceMultiplier = 350,
            TinfoilReward = 12,
            Icon = "🏃"
        },
        new Quest
        {
            Id = "server_farm_hack",
            Name = "Server Farm Infiltration",
            Description = "Access classified data centers",
            FlavorText = "The cloud isn't in the sky. It's underground.",
            Risk = QuestRisk.Medium,
            BelieversRequired = 800,
            DurationSeconds = 600,
            SuccessChance = 0.60,
            EvidenceMultiplier = 900,
            TinfoilReward = 30,
            Icon = "💾"
        },
        new Quest
        {
            Id = "chemtrail_sample",
            Name = "Chemtrail Sample Collection",
            Description = "Collect air samples from suspicious flight paths",
            FlavorText = "Those aren't contrails. The proof is in the particles.",
            Risk = QuestRisk.Low,
            BelieversRequired = 50,
            DurationSeconds = 180,
            SuccessChance = 0.88,
            EvidenceMultiplier = 150,
            TinfoilReward = 4,
            Icon = "✈️"
        },
        new Quest
        {
            Id = "ancient_archive",
            Name = "Ancient Archive Discovery",
            Description = "Explore hidden chambers beneath historical sites",
            FlavorText = "History is written by winners. The truth is buried by losers.",
            Risk = QuestRisk.Medium,
            BelieversRequired = 2500,
            DurationSeconds = 900,
            SuccessChance = 0.50,
            EvidenceMultiplier = 2500,
            TinfoilReward = 60,
            Icon = "🏛️"
        },
        new Quest
        {
            Id = "celebrity_clone_hunt",
            Name = "Celebrity Clone Investigation",
            Description = "Document evidence of celebrity replacements",
            FlavorText = "The ear lobes never lie.",
            Risk = QuestRisk.Medium,
            BelieversRequired = 8000,
            DurationSeconds = 600,
            SuccessChance = 0.45,
            EvidenceMultiplier = 4000,
            TinfoilReward = 80,
            Icon = "🎭"
        },
        new Quest
        {
            Id = "haarp_investigation",
            Name = "HAARP Investigation",
            Description = "Monitor weather manipulation frequencies",
            FlavorText = "That hurricane wasn't natural. Neither was that sunny day.",
            Risk = QuestRisk.High,
            BelieversRequired = 25000,
            DurationSeconds = 1200,
            SuccessChance = 0.35,
            EvidenceMultiplier = 25000,
            TinfoilReward = 300,
            Icon = "🌩️"
        },
        new Quest
        {
            Id = "bigfoot_alliance",
            Name = "Cryptid Alliance Formation",
            Description = "Establish contact with hidden humanoid populations",
            FlavorText = "They've been hiding for a reason. Now they're ready to talk.",
            Risk = QuestRisk.Medium,
            BelieversRequired = 3000,
            DurationSeconds = 480,
            SuccessChance = 0.52,
            EvidenceMultiplier = 1800,
            TinfoilReward = 50,
            Icon = "🦶"
        },
        new Quest
        {
            Id = "time_anomaly",
            Name = "Time Anomaly Investigation",
            Description = "Document temporal inconsistencies in historical records",
            FlavorText = "The Mandela Effect isn't memory. It's evidence.",
            Risk = QuestRisk.High,
            BelieversRequired = 100000,
            DurationSeconds = 1500,
            SuccessChance = 0.22,
            EvidenceMultiplier = 200000,
            TinfoilReward = 2000,
            Icon = "⏳"
        },
        new Quest
        {
            Id = "hollow_moon",
            Name = "Hollow Moon Expedition",
            Description = "Analyze lunar ringing data for artificial structures",
            FlavorText = "When Apollo hit it, the moon rang like a bell. Bells are hollow.",
            Risk = QuestRisk.High,
            BelieversRequired = 300000,
            DurationSeconds = 1500,
            SuccessChance = 0.15,
            EvidenceMultiplier = 800000,
            TinfoilReward = 5000,
            Icon = "🌑"
        },
        new Quest
        {
            Id = "reptilian_gala",
            Name = "Reptilian Gala Infiltration",
            Description = "Attend an elite gathering in disguise",
            FlavorText = "Remember: act cold-blooded, avoid heat lamps.",
            Risk = QuestRisk.High,
            BelieversRequired = 80000,
            DurationSeconds = 1200,
            SuccessChance = 0.25,
            EvidenceMultiplier = 120000,
            TinfoilReward = 1500,
            Icon = "🦎"
        },
        new Quest
        {
            Id = "antarctic_expedition",
            Name = "Antarctic Expedition",
            Description = "Explore the restricted zones beyond the ice wall",
            FlavorText = "They guard it for a reason. Let's find out why.",
            Risk = QuestRisk.High,
            BelieversRequired = 10000000,
            DurationSeconds = 1500,
            SuccessChance = 0.08,
            EvidenceMultiplier = 100000000,
            TinfoilReward = 150000,
            Icon = "🧊"
        },
        new Quest
        {
            Id = "alien_embassy",
            Name = "Alien Embassy Contact",
            Description = "Establish diplomatic relations with extraterrestrial observers",
            FlavorText = "They've been watching. Time to start talking.",
            Risk = QuestRisk.High,
            BelieversRequired = 50000000,
            DurationSeconds = 1500,
            SuccessChance = 0.04,
            EvidenceMultiplier = 500000000,
            TinfoilReward = 500000,
            Icon = "👽"
        },
        new Quest
        {
            Id = "simulation_glitch",
            Name = "Simulation Glitch Exploitation",
            Description = "Find and document cracks in reality's code",
            FlavorText = "Deja vu isn't a bug. It's a feature we can exploit.",
            Risk = QuestRisk.High,
            BelieversRequired = 200000000,
            DurationSeconds = 1500,
            SuccessChance = 0.03,
            EvidenceMultiplier = 2000000000,
            TinfoilReward = 1000000,
            Icon = "🔧"
        }
    };

    public static Quest? GetById(string id) => AllQuests.FirstOrDefault(q => q.Id == id);

    public static IEnumerable<Quest> GetAvailable(double believers)
    {
        return AllQuests.Where(q => believers >= q.BelieversRequired);
    }
}
