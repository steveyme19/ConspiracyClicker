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
        new Quest
        {
            Id = "underground_bunker_survey",
            Name = "Underground Bunker Survey",
            Description = "Map abandoned government shelters for hidden tech",
            FlavorText = "Built for nuclear war. Used for something else entirely.",
            Risk = QuestRisk.Medium,
            BelieversRequired = 350,
            DurationSeconds = 350,
            SuccessChance = 0.68,
            EvidenceMultiplier = 400,
            TinfoilReward = 14,
            Icon = "🏚️"
        },
        new Quest
        {
            Id = "corporate_mole_placement",
            Name = "Corporate Mole Placement",
            Description = "Insert believers into suspicious megacorp positions",
            FlavorText = "The real conspiracy is always in the quarterly reports.",
            Risk = QuestRisk.Medium,
            BelieversRequired = 600,
            DurationSeconds = 400,
            SuccessChance = 0.65,
            EvidenceMultiplier = 500,
            TinfoilReward = 16,
            Icon = "💼"
        },
        new Quest
        {
            Id = "crop_circle_decryption",
            Name = "Crop Circle Decryption",
            Description = "Decode messages left in agricultural formations",
            FlavorText = "Farmers blame pranksters. Pranksters blame aliens. We blame both.",
            Risk = QuestRisk.Medium,
            BelieversRequired = 1200,
            DurationSeconds = 600,
            SuccessChance = 0.58,
            EvidenceMultiplier = 1000,
            TinfoilReward = 28,
            Icon = "🌾"
        },
        new Quest
        {
            Id = "mind_control_frequency_jam",
            Name = "Mind Control Frequency Jam",
            Description = "Disrupt subliminal broadcast signals in populated areas",
            FlavorText = "That ringing in your ears? We're about to make it stop.",
            Risk = QuestRisk.Medium,
            BelieversRequired = 2000,
            DurationSeconds = 750,
            SuccessChance = 0.52,
            EvidenceMultiplier = 2000,
            TinfoilReward = 45,
            Icon = "📻"
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
        },

        // === TRANSCENDENT TIER QUESTS ===
        new Quest
        {
            Id = "multiverse_bridge",
            Name = "Multiverse Bridge Construction",
            Description = "Build a stable connection to parallel realities",
            FlavorText = "In another timeline, you already succeeded. Borrow their work.",
            Risk = QuestRisk.High,
            BelieversRequired = 500000000,
            DurationSeconds = 1200,
            SuccessChance = 0.04,
            EvidenceMultiplier = 5000000000,
            TinfoilReward = 2500000,
            Icon = "🌉"
        },
        new Quest
        {
            Id = "akashic_download",
            Name = "Akashic Records Download",
            Description = "Download the complete history of all conspiracies",
            FlavorText = "Unlimited cloud storage. Literally cosmic.",
            Risk = QuestRisk.High,
            BelieversRequired = 1000000000,
            DurationSeconds = 1500,
            SuccessChance = 0.03,
            EvidenceMultiplier = 10000000000,
            TinfoilReward = 5000000,
            Icon = "📖"
        },
        new Quest
        {
            Id = "god_committee_meeting",
            Name = "God Committee Attendance",
            Description = "Attend the meeting where reality decisions are made",
            FlavorText = "RSVP: Mandatory. Dress code: Omniscient casual.",
            Risk = QuestRisk.High,
            BelieversRequired = 5000000000,
            DurationSeconds = 1500,
            SuccessChance = 0.02,
            EvidenceMultiplier = 50000000000,
            TinfoilReward = 10000000,
            Icon = "⚖️"
        },
        new Quest
        {
            Id = "entropy_reversal",
            Name = "Entropy Reversal Experiment",
            Description = "Run time backwards to witness the original cover-up",
            FlavorText = "Spoiler: It was always a conspiracy.",
            Risk = QuestRisk.High,
            BelieversRequired = 10000000000,
            DurationSeconds = 1200,
            SuccessChance = 0.025,
            EvidenceMultiplier = 100000000000,
            TinfoilReward = 25000000,
            Icon = "⏪"
        },
        new Quest
        {
            Id = "universe_source_code",
            Name = "Universe Source Code Audit",
            Description = "Review the code that runs existence itself",
            FlavorText = "// TODO: Fix conspiracy bug - has been here since v1.0",
            Risk = QuestRisk.High,
            BelieversRequired = 50000000000,
            DurationSeconds = 1500,
            SuccessChance = 0.02,
            EvidenceMultiplier = 500000000000,
            TinfoilReward = 50000000,
            Icon = "💻"
        },
        new Quest
        {
            Id = "cosmic_consciousness_merge",
            Name = "Cosmic Consciousness Merge",
            Description = "Become one with the universal awareness",
            FlavorText = "You are the universe experiencing itself. Conspiratorially.",
            Risk = QuestRisk.High,
            BelieversRequired = 100000000000,
            DurationSeconds = 1500,
            SuccessChance = 0.015,
            EvidenceMultiplier = 1000000000000,
            TinfoilReward = 100000000,
            Icon = "🧘"
        },
        new Quest
        {
            Id = "omega_point_expedition",
            Name = "Omega Point Expedition",
            Description = "Journey to the end of time where all truth converges",
            FlavorText = "At the end of everything, the truth finally comes out.",
            Risk = QuestRisk.High,
            BelieversRequired = 500000000000,
            DurationSeconds = 1500,
            SuccessChance = 0.01,
            EvidenceMultiplier = 10000000000000,
            TinfoilReward = 500000000,
            Icon = "🔚"
        },
        new Quest
        {
            Id = "reality_rewrite",
            Name = "Reality Rewrite Operation",
            Description = "Edit the fundamental laws of existence",
            FlavorText = "If you can't find the truth, make it.",
            Risk = QuestRisk.High,
            BelieversRequired = 1000000000000,
            DurationSeconds = 1200,
            SuccessChance = 0.01,
            EvidenceMultiplier = 100000000000000,
            TinfoilReward = 1000000000,
            Icon = "✏️"
        }
    };

    public static Quest? GetById(string id) => AllQuests.FirstOrDefault(q => q.Id == id);

    public static IEnumerable<Quest> GetAvailable(double believers)
    {
        return AllQuests.Where(q => believers >= q.BelieversRequired);
    }
}
