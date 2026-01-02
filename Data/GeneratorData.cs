using ConspiracyClicker.Models;

namespace ConspiracyClicker.Data;

public static class GeneratorData
{
    public static readonly List<Generator> AllGenerators = new()
    {
        // === TIER 1: EARLY GAME (Pre-Ascension accessible) ===
        new Generator
        {
            Id = "red_string",
            Name = "Red String",
            FlavorText = "Connects random photos on your cork board. Sometimes to itself.",
            BaseCost = 15,
            BaseProduction = 0.2,
            BelieverBonus = 0
        },
        new Generator
        {
            Id = "suspicious_neighbor",
            Name = "Suspicious Neighbor",
            FlavorText = "He's 'just gardening' at 3 AM. Sure, Gary.",
            BaseCost = 100,
            BaseProduction = 1.5,
            BelieverBonus = 1
        },
        new Generator
        {
            Id = "basement_researcher",
            Name = "Basement Researcher",
            FlavorText = "Hasn't seen sunlight in 47 days. Living the dream.",
            BaseCost = 1000,
            BaseProduction = 10,
            BelieverBonus = 3
        },
        new Generator
        {
            Id = "blogspot_blog",
            Name = "Blogspot Blog",
            FlavorText = "Est. 2004. Still using the same background.",
            BaseCost = 10000,
            BaseProduction = 60,
            BelieverBonus = 10
        },
        new Generator
        {
            Id = "youtube_channel",
            Name = "YouTube Channel",
            FlavorText = "Please like, subscribe, and question everything.",
            BaseCost = 100000,
            BaseProduction = 350,
            BelieverBonus = 50
        },
        new Generator
        {
            Id = "discord_server",
            Name = "Discord Server",
            FlavorText = "2,000 members. 3 actually active. All named Kyle.",
            BaseCost = 500000,
            BaseProduction = 2000,
            BelieverBonus = 200
        },
        new Generator
        {
            Id = "am_radio",
            Name = "AM Radio Show",
            FlavorText = "Broadcasting truth between mattress ads.",
            BaseCost = 3000000,
            BaseProduction = 12000,
            BelieverBonus = 400
        },
        new Generator
        {
            Id = "podcast",
            Name = "4-Hour Podcast",
            FlavorText = "Joe called. He wants his format back.",
            BaseCost = 30000000,
            BaseProduction = 75000,
            BelieverBonus = 1500
        },
        new Generator
        {
            Id = "truth_conference",
            Name = "Truth Conference",
            FlavorText = "Holiday Inn Express. Complimentary tinfoil.",
            BaseCost = 400000000,
            BaseProduction = 500000,
            BelieverBonus = 5000
        },
        new Generator
        {
            Id = "netflix_doc",
            Name = "Netflix Documentary",
            FlavorText = "Suspiciously professional. Almost TOO credible...",
            BaseCost = 8000000000,
            BaseProduction = 3500000,
            BelieverBonus = 15000
        },
        new Generator
        {
            Id = "spy_satellite",
            Name = "Spy Satellite",
            FlavorText = "Definitely not stolen from eBay. No questions.",
            BaseCost = 150000000000,
            BaseProduction = 25000000,
            BelieverBonus = 40000
        },
        new Generator
        {
            Id = "shadow_government",
            Name = "Shadow Government",
            FlavorText = "You've become the deep state. Congrats?",
            BaseCost = 3000000000000,
            BaseProduction = 180000000,
            BelieverBonus = 100000
        },

        // === TIER 2: POST-ASCENSION (Requires Illuminati bonuses) ===
        new Generator
        {
            Id = "mind_control_tower",
            Name = "Mind Control Tower",
            FlavorText = "5G? Try 5000G. Straight to the brain.",
            BaseCost = 75000000000000,
            CostMultiplier = 1.13,
            BaseProduction = 1200000000,
            BelieverBonus = 500000
        },
        new Generator
        {
            Id = "weather_machine",
            Name = "Weather Machine",
            FlavorText = "HAARP was just the prototype.",
            BaseCost = 2000000000000000,
            CostMultiplier = 1.12,
            BaseProduction = 8000000000,
            BelieverBonus = 2000000
        },
        new Generator
        {
            Id = "clone_facility",
            Name = "Clone Facility",
            FlavorText = "Celebrity replacements made fresh daily.",
            BaseCost = 60000000000000000,
            CostMultiplier = 1.11,
            BaseProduction = 55000000000,
            BelieverBonus = 8000000
        },
        new Generator
        {
            Id = "time_machine",
            Name = "Time Machine",
            FlavorText = "Mandela Effect? More like Mandela THEFT.",
            BaseCost = 2000000000000000000,
            CostMultiplier = 1.10,
            BaseProduction = 400000000000,
            BelieverBonus = 30000000
        },
        new Generator
        {
            Id = "hollow_earth_base",
            Name = "Hollow Earth Base",
            FlavorText = "Admiral Byrd was right. It's cozy down here.",
            BaseCost = 80000000000000000000.0,
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
            BaseCost = 4000000000000000000000.0,
            CostMultiplier = 1.08,
            BaseProduction = 25000000000000,
            BelieverBonus = 500000000
        },
        new Generator
        {
            Id = "alien_alliance",
            Name = "Alien Alliance",
            FlavorText = "They're not little green men. They're tall grey accountants.",
            BaseCost = 250000000000000000000000.0,
            CostMultiplier = 1.07,
            BaseProduction = 200000000000000,
            BelieverBonus = 2000000000
        },
        new Generator
        {
            Id = "dimension_portal",
            Name = "Dimension Portal",
            FlavorText = "CERN opened it. We just... borrowed access.",
            BaseCost = 20000000000000000000000000.0,
            CostMultiplier = 1.06,
            BaseProduction = 1800000000000000,
            BelieverBonus = 10000000000
        },
        new Generator
        {
            Id = "simulation_admin",
            Name = "Simulation Admin Access",
            FlavorText = "sudo truth --force",
            BaseCost = 2000000000000000000000000000.0,
            CostMultiplier = 1.05,
            BaseProduction = 18000000000000000,
            BelieverBonus = 50000000000
        },
        new Generator
        {
            Id = "reality_editor",
            Name = "Reality Editor",
            FlavorText = "Ctrl+Z the cover-ups.",
            BaseCost = 250000000000000000000000000000.0,
            CostMultiplier = 1.05,
            BaseProduction = 200000000000000000,
            BelieverBonus = 250000000000
        },

        // === TIER 4: COSMIC TRUTH (Many Ascensions + Full Skill Tree) ===
        new Generator
        {
            Id = "multiverse_network",
            Name = "Multiverse Network",
            FlavorText = "In one timeline, this is all true. This is that timeline.",
            BaseCost = 40000000000000000000000000000000.0,
            CostMultiplier = 1.04,
            BaseProduction = 2500000000000000000,
            BelieverBonus = 1500000000000
        },
        new Generator
        {
            Id = "cosmic_consciousness",
            Name = "Cosmic Consciousness",
            FlavorText = "You ARE the conspiracy now.",
            BaseCost = 8000000000000000000000000000000000.0,
            CostMultiplier = 1.04,
            BaseProduction = 35000000000000000000.0,
            BelieverBonus = 10000000000000
        },
        new Generator
        {
            Id = "truth_singularity_gen",
            Name = "Truth Singularity",
            FlavorText = "All conspiracies converge into one. You're at the center.",
            BaseCost = 2000000000000000000000000000000000000.0,
            CostMultiplier = 1.03,
            BaseProduction = 600000000000000000000.0,
            BelieverBonus = 80000000000000
        },
        new Generator
        {
            Id = "omniscience_engine",
            Name = "Omniscience Engine",
            FlavorText = "You don't find the truth. The truth finds you.",
            BaseCost = 800000000000000000000000000000000000000.0,
            CostMultiplier = 1.03,
            BaseProduction = 15000000000000000000000.0,
            BelieverBonus = 750000000000000
        },
        new Generator
        {
            Id = "universe_creator",
            Name = "Universe Creator",
            FlavorText = "Why uncover conspiracies when you can CREATE realities?",
            BaseCost = 500000000000000000000000000000000000000000.0,
            CostMultiplier = 1.02,
            BaseProduction = 500000000000000000000000.0,
            BelieverBonus = 10000000000000000
        }
    };

    public static Generator? GetById(string id)
    {
        return AllGenerators.FirstOrDefault(g => g.Id == id);
    }
}
