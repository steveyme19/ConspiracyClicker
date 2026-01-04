using ConspiracyClicker.Models;

namespace ConspiracyClicker.Data;

public static class GeneratorData
{
    public static readonly List<Generator> AllGenerators = new()
    {
        // === TIER 1: EARLY GAME (Pre-Ascension accessible) ===
        // WALL DESIGN: Cost multiplier 1.18 = costs grow ~5.5x per 10 purchases
        // But production is LINEAR - creating the Clicker Heroes wall effect
        // Without Illuminati multipliers, you eventually hit a wall where
        // buying more generators becomes pointless (cost >> value)
        new Generator
        {
            Id = "red_string",
            Name = "Red String",
            FlavorText = "Connects random photos on your cork board. Sometimes to itself.",
            BaseCost = 15,
            CostMultiplier = 1.15,
            BaseProduction = 1,
            BelieverBonus = 0
        },
        new Generator
        {
            Id = "suspicious_neighbor",
            Name = "Suspicious Neighbor",
            FlavorText = "He's 'just gardening' at 3 AM. Sure, Gary.",
            BaseCost = 100,
            CostMultiplier = 1.15,
            BaseProduction = 5,
            BelieverBonus = 1
        },
        new Generator
        {
            Id = "basement_researcher",
            Name = "Basement Researcher",
            FlavorText = "Hasn't seen sunlight in 47 days. Living the dream.",
            BaseCost = 1100,
            CostMultiplier = 1.15,
            BaseProduction = 30,
            BelieverBonus = 3
        },
        new Generator
        {
            Id = "blogspot_blog",
            Name = "Blogspot Blog",
            FlavorText = "Est. 2004. Still using the same background.",
            BaseCost = 12000,
            CostMultiplier = 1.14,
            BaseProduction = 200,
            BelieverBonus = 10
        },
        new Generator
        {
            Id = "youtube_channel",
            Name = "YouTube Channel",
            FlavorText = "Please like, subscribe, and question everything.",
            BaseCost = 130000,
            CostMultiplier = 1.14,
            BaseProduction = 1400,
            BelieverBonus = 50
        },
        new Generator
        {
            Id = "discord_server",
            Name = "Discord Server",
            FlavorText = "2,000 members. 3 actually active. All named Kyle.",
            BaseCost = 1400000,
            CostMultiplier = 1.13,
            BaseProduction = 10000,
            BelieverBonus = 200
        },
        new Generator
        {
            Id = "am_radio",
            Name = "AM Radio Show",
            FlavorText = "Broadcasting truth between mattress ads.",
            BaseCost = 20000000,
            CostMultiplier = 1.13,
            BaseProduction = 75000,
            BelieverBonus = 400
        },
        new Generator
        {
            Id = "podcast",
            Name = "4-Hour Podcast",
            FlavorText = "Joe called. He wants his format back.",
            BaseCost = 330000000,
            CostMultiplier = 1.12,
            BaseProduction = 500000,
            BelieverBonus = 1500
        },
        new Generator
        {
            Id = "truth_conference",
            Name = "Truth Conference",
            FlavorText = "Holiday Inn Express. Complimentary tinfoil.",
            BaseCost = 5100000000,
            CostMultiplier = 1.12,
            BaseProduction = 3600000,
            BelieverBonus = 5000
        },
        new Generator
        {
            Id = "netflix_doc",
            Name = "Netflix Documentary",
            FlavorText = "Suspiciously professional. Almost TOO credible...",
            BaseCost = 75000000000,
            CostMultiplier = 1.11,
            BaseProduction = 26000000,
            BelieverBonus = 15000
        },
        new Generator
        {
            Id = "spy_satellite",
            Name = "Spy Satellite",
            FlavorText = "Definitely not stolen from eBay. No questions.",
            BaseCost = 1000000000000,
            CostMultiplier = 1.11,
            BaseProduction = 190000000,
            BelieverBonus = 40000
        },
        new Generator
        {
            Id = "shadow_government",
            Name = "Shadow Government",
            FlavorText = "You've become the deep state. Congrats?",
            BaseCost = 14000000000000,
            CostMultiplier = 1.10,
            BaseProduction = 1400000000,
            BelieverBonus = 100000
        },

        // === TIER 2: POST-ASCENSION (Requires Illuminati bonuses) ===
        new Generator
        {
            Id = "mind_control_tower",
            Name = "Mind Control Tower",
            FlavorText = "5G? Try 5000G. Straight to the brain.",
            BaseCost = 30000000000000000, // 3e16 - fixed cost order
            CostMultiplier = 1.13,
            BaseProduction = 1200000000,
            BelieverBonus = 500000
        },
        new Generator
        {
            Id = "weather_machine",
            Name = "Weather Machine",
            FlavorText = "HAARP was just the prototype.",
            BaseCost = 12000000000000000,
            CostMultiplier = 1.12,
            BaseProduction = 8000000000,
            BelieverBonus = 2000000
        },
        new Generator
        {
            Id = "clone_facility",
            Name = "Clone Facility",
            FlavorText = "Celebrity replacements made fresh daily.",
            BaseCost = 360000000000000000,
            CostMultiplier = 1.11,
            BaseProduction = 55000000000,
            BelieverBonus = 8000000
        },
        new Generator
        {
            Id = "time_machine",
            Name = "Time Machine",
            FlavorText = "Mandela Effect? More like Mandela THEFT.",
            BaseCost = 12000000000000000000.0,
            CostMultiplier = 1.10,
            BaseProduction = 400000000000,
            BelieverBonus = 30000000
        },
        new Generator
        {
            Id = "hollow_earth_base",
            Name = "Hollow Earth Base",
            FlavorText = "Admiral Byrd was right. It's cozy down here.",
            BaseCost = 480000000000000000000.0,
            CostMultiplier = 1.09,
            BaseProduction = 3000000000000.0,
            BelieverBonus = 120000000
        },

        // === TIER 3: DEEP CONSPIRACY (Multiple Ascensions) ===
        new Generator
        {
            Id = "moon_base",
            Name = "Secret Moon Base",
            FlavorText = "The dark side has GREAT real estate.",
            BaseCost = 24000000000000000000000.0,
            CostMultiplier = 1.08,
            BaseProduction = 25000000000000,
            BelieverBonus = 500000000
        },
        new Generator
        {
            Id = "void_whispers",
            Name = "Void Whisper Network",
            FlavorText = "The empty space between stars... isn't empty. They're talking.",
            BaseCost = 1.2e23,
            CostMultiplier = 1.075,
            BaseProduction = 8e13,
            BelieverBonus = 8e8,
            Icon = "🌑"
        },
        new Generator
        {
            Id = "stargate_array",
            Name = "Stargate Array",
            FlavorText = "The Egyptians knew. The Mayans knew. Now you know.",
            BaseCost = 4e23,
            CostMultiplier = 1.07,
            BaseProduction = 5e13,
            BelieverBonus = 1.2e9,
            Icon = "🌀"
        },
        new Generator
        {
            Id = "alien_alliance",
            Name = "Alien Alliance",
            FlavorText = "They're not little green men. They're tall grey accountants.",
            BaseCost = 1500000000000000000000000.0,
            CostMultiplier = 1.07,
            BaseProduction = 200000000000000,
            BelieverBonus = 2000000000
        },
        new Generator
        {
            Id = "dimension_portal",
            Name = "Dimension Portal",
            FlavorText = "CERN opened it. We just... borrowed access.",
            BaseCost = 120000000000000000000000000.0,
            CostMultiplier = 1.06,
            BaseProduction = 1800000000000000,
            BelieverBonus = 10000000000
        },
        new Generator
        {
            Id = "simulation_admin",
            Name = "Simulation Admin Access",
            FlavorText = "sudo truth --force",
            BaseCost = 12000000000000000000000000000.0,
            CostMultiplier = 1.05,
            BaseProduction = 18000000000000000,
            BelieverBonus = 50000000000
        },
        new Generator
        {
            Id = "reality_editor",
            Name = "Reality Editor",
            FlavorText = "Ctrl+Z the cover-ups.",
            BaseCost = 1500000000000000000000000000000.0,
            CostMultiplier = 1.05,
            BaseProduction = 200000000000000000,
            BelieverBonus = 250000000000
        },
        new Generator
        {
            Id = "quantum_entangler",
            Name = "Quantum Entanglement Grid",
            FlavorText = "Observe one truth, collapse another. Spooky action at a distance.",
            BaseCost = 1.5e31,
            CostMultiplier = 1.045,
            BaseProduction = 6e17,
            BelieverBonus = 6e11,
            Icon = "⚛️"
        },
        new Generator
        {
            Id = "akashic_terminal",
            Name = "Akashic Records Terminal",
            FlavorText = "All knowledge, ever. It's a lot of scrolling.",
            BaseCost = 1.5e32,
            CostMultiplier = 1.04,
            BaseProduction = 2e18,
            BelieverBonus = 1e12,
            Icon = "📜"
        },

        // === TIER 4: COSMIC TRUTH (Many Ascensions + Full Skill Tree) ===
        new Generator
        {
            Id = "multiverse_network",
            Name = "Multiverse Network",
            FlavorText = "In one timeline, this is all true. This is that timeline.",
            BaseCost = 240000000000000000000000000000000.0,
            CostMultiplier = 1.04,
            BaseProduction = 2500000000000000000,
            BelieverBonus = 1500000000000
        },
        new Generator
        {
            Id = "cosmic_consciousness",
            Name = "Cosmic Consciousness",
            FlavorText = "You ARE the conspiracy now.",
            BaseCost = 48000000000000000000000000000000000.0,
            CostMultiplier = 1.04,
            BaseProduction = 35000000000000000000.0,
            BelieverBonus = 10000000000000
        },
        new Generator
        {
            Id = "probability_weaver",
            Name = "Probability Weaver",
            FlavorText = "Every timeline where you succeed? They all become this one.",
            BaseCost = 5e35,
            CostMultiplier = 1.035,
            BaseProduction = 1.5e20,
            BelieverBonus = 5e13,
            Icon = "🎲"
        },
        new Generator
        {
            Id = "paradox_engine",
            Name = "Paradox Engine",
            FlavorText = "It exists because it doesn't. It proves itself by contradiction.",
            BaseCost = 5e36,
            CostMultiplier = 1.03,
            BaseProduction = 6e20,
            BelieverBonus = 3e14,
            Icon = "♾️"
        },
        new Generator
        {
            Id = "truth_singularity_gen",
            Name = "Truth Singularity",
            FlavorText = "All conspiracies converge into one. You're at the center.",
            BaseCost = 12000000000000000000000000000000000000.0,
            CostMultiplier = 1.03,
            BaseProduction = 600000000000000000000.0,
            BelieverBonus = 80000000000000
        },
        new Generator
        {
            Id = "omniscience_engine",
            Name = "Omniscience Engine",
            FlavorText = "You don't find the truth. The truth finds you.",
            BaseCost = 4800000000000000000000000000000000000000.0,
            CostMultiplier = 1.03,
            BaseProduction = 15000000000000000000000.0,
            BelieverBonus = 750000000000000
        },
        new Generator
        {
            Id = "information_nexus",
            Name = "Information Nexus",
            FlavorText = "Every secret, every lie, every truth. All roads lead here.",
            BaseCost = 1.5e40,
            CostMultiplier = 1.025,
            BaseProduction = 5e22,
            BelieverBonus = 3e15,
            Icon = "🕸️"
        },
        new Generator
        {
            Id = "timeline_harvester",
            Name = "Timeline Harvester",
            FlavorText = "Reaping evidence from timelines that never existed. Until now.",
            BaseCost = 5e41,
            CostMultiplier = 1.02,
            BaseProduction = 1.5e23,
            BelieverBonus = 1e16,
            Icon = "⌛"
        },
        new Generator
        {
            Id = "universe_creator",
            Name = "Universe Creator",
            FlavorText = "Why uncover conspiracies when you can CREATE realities?",
            BaseCost = 3000000000000000000000000000000000000000000.0,
            CostMultiplier = 1.02,
            BaseProduction = 500000000000000000000000.0,
            BelieverBonus = 10000000000000000
        },

        // === TIER 5: TRANSCENDENT (Bridges to final conspiracies) ===
        // Smoother cost progression: each ~20-30x more expensive than previous
        new Generator
        {
            Id = "entropy_reverser",
            Name = "Entropy Reverser",
            FlavorText = "Heat death? More like heat LIFE. Time flows backwards here.",
            BaseCost = 8e43,  // ~25x Universe Creator
            CostMultiplier = 1.02,
            BaseProduction = 1.2e25,
            BelieverBonus = 1.5e17
        },
        new Generator
        {
            Id = "probability_matrix",
            Name = "Probability Matrix",
            FlavorText = "Every dice roll is 6. Every coin flip is heads. Every conspiracy is true.",
            BaseCost = 2e45,  // ~25x Entropy Reverser
            CostMultiplier = 1.02,
            BaseProduction = 3e26,
            BelieverBonus = 2.5e18
        },
        new Generator
        {
            Id = "existence_compiler",
            Name = "Existence Compiler",
            FlavorText = "Reality.exe has been rebuilt from source. No more bugs. Only features.",
            BaseCost = 5e46,  // ~25x Probability Matrix
            CostMultiplier = 1.015,
            BaseProduction = 8e27,
            BelieverBonus = 4e19
        },
        new Generator
        {
            Id = "infinite_recursion",
            Name = "Infinite Recursion Engine",
            FlavorText = "A conspiracy about conspiracies about conspiracies about... stack overflow.",
            BaseCost = 1.2e48,  // ~25x Existence Compiler
            CostMultiplier = 1.015,
            BaseProduction = 2e29,
            BelieverBonus = 6e20
        },
        new Generator
        {
            Id = "causality_loop",
            Name = "Causality Loop Generator",
            FlavorText = "The evidence proves itself. It always has. It always will. It already did.",
            BaseCost = 3e49,  // ~25x Infinite Recursion
            CostMultiplier = 1.01,
            BaseProduction = 5e30,
            BelieverBonus = 1e22
        },
        new Generator
        {
            Id = "absolute_truth",
            Name = "Absolute Truth Beacon",
            FlavorText = "Not A truth. THE truth. All of it. Forever. You're welcome.",
            BaseCost = 8e50,  // ~25x Causality Loop
            CostMultiplier = 1.01,
            BaseProduction = 1.2e32,
            BelieverBonus = 2e23
        },
        new Generator
        {
            Id = "omega_point",
            Name = "Omega Point Collective",
            FlavorText = "The final evolution of consciousness. Everyone knows everything. Nobody's surprised.",
            BaseCost = 2e52,  // ~25x Absolute Truth
            CostMultiplier = 1.008,
            BaseProduction = 3e33,
            BelieverBonus = 5e24
        },

        // === TIER 6: TRANSCENDENCE (Beyond the Omega Point) ===
        new Generator
        {
            Id = "void_architect",
            Name = "Void Architect",
            FlavorText = "Building realities from the spaces between realities.",
            BaseCost = 5e53,
            CostMultiplier = 1.007,
            BaseProduction = 8e34,
            BelieverBonus = 1.5e25,
            Icon = "🕳️"
        },
        new Generator
        {
            Id = "cosmic_forge",
            Name = "Cosmic Forge",
            FlavorText = "Stars are just raw materials. Truth is what you make of them.",
            BaseCost = 1.2e55,
            CostMultiplier = 1.006,
            BaseProduction = 2e36,
            BelieverBonus = 4e26,
            Icon = "🔨"
        },
        new Generator
        {
            Id = "dimension_weaver",
            Name = "Dimension Weaver",
            FlavorText = "Eleven dimensions? Try eleven thousand. Each one whispers truth.",
            BaseCost = 3e56,
            CostMultiplier = 1.006,
            BaseProduction = 5e37,
            BelieverBonus = 1e28,
            Icon = "🧵"
        },
        new Generator
        {
            Id = "eternity_engine",
            Name = "Eternity Engine",
            FlavorText = "Time is a suggestion. You respectfully decline.",
            BaseCost = 8e57,
            CostMultiplier = 1.005,
            BaseProduction = 1.2e39,
            BelieverBonus = 2.5e29,
            Icon = "⏳"
        },
        new Generator
        {
            Id = "primordial_truth",
            Name = "Primordial Truth Excavator",
            FlavorText = "Digging up secrets from before the Big Bang. Spoiler: it was an inside job.",
            BaseCost = 2e59,
            CostMultiplier = 1.005,
            BaseProduction = 3e40,
            BelieverBonus = 6e30,
            Icon = "💎"
        },
        new Generator
        {
            Id = "consciousness_merger",
            Name = "Universal Consciousness Merger",
            FlavorText = "Every mind in existence, thinking as one. Imagine the Reddit karma.",
            BaseCost = 5e60,
            CostMultiplier = 1.004,
            BaseProduction = 8e41,
            BelieverBonus = 1.5e32,
            Icon = "🧠"
        },

        // === TIER 7: APOTHEOSIS (Final tier) ===
        new Generator
        {
            Id = "reality_seed",
            Name = "Reality Seed Planter",
            FlavorText = "Plant a universe, harvest a conspiracy. Sustainable truth farming.",
            BaseCost = 1.2e62,
            CostMultiplier = 1.004,
            BaseProduction = 2e43,
            BelieverBonus = 4e33,
            Icon = "🌱"
        },
        new Generator
        {
            Id = "omni_fabricator",
            Name = "Omni-Fabricator",
            FlavorText = "If it can exist, it does. If it can't, it still does. Logic is optional.",
            BaseCost = 3e63,
            CostMultiplier = 1.003,
            BaseProduction = 5e44,
            BelieverBonus = 1e35,
            Icon = "⚙️"
        },
        new Generator
        {
            Id = "infinity_conduit",
            Name = "Infinity Conduit",
            FlavorText = "Channeling evidence from an infinite number of universes. The math checks out.",
            BaseCost = 8e64,
            CostMultiplier = 1.003,
            BaseProduction = 1.2e46,
            BelieverBonus = 2.5e36,
            Icon = "♾️"
        },
        new Generator
        {
            Id = "existence_core",
            Name = "Existence Core",
            FlavorText = "The heart of all being. It beats in conspiracy.",
            BaseCost = 2e66,
            CostMultiplier = 1.002,
            BaseProduction = 3e47,
            BelieverBonus = 6e37,
            Icon = "❤️"
        },
        new Generator
        {
            Id = "final_revelation_gen",
            Name = "Final Revelation Device",
            FlavorText = "The last truth. The only truth. Your truth.",
            BaseCost = 5e67,
            CostMultiplier = 1.002,
            BaseProduction = 8e48,
            BelieverBonus = 1.5e39,
            Icon = "👁️"
        }
    };

    public static Generator? GetById(string id)
    {
        return AllGenerators.FirstOrDefault(g => g.Id == id);
    }
}
