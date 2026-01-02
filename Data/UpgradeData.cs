using ConspiracyClicker.Models;

namespace ConspiracyClicker.Data;

public static class UpgradeData
{
    public static readonly List<Upgrade> AllUpgrades = new()
    {
        // === CLICK POWER UPGRADES (small values to stay at ~10% of EPS) ===
        new Upgrade
        {
            Id = "reinforced_tinfoil",
            Name = "Reinforced Tinfoil Hat",
            Description = "+0.2 click power",
            FlavorText = "Now with extra crinkles for style.",
            Type = UpgradeType.ClickPower,
            Value = 0.2,
            EvidenceCost = 50,
            RequiredEvidence = 25
        },
        new Upgrade
        {
            Id = "magnifying_glass",
            Name = "Magnifying Glass",
            Description = "+0.5 click power",
            FlavorText = "For examining those suspicious documents.",
            Type = UpgradeType.ClickPower,
            Value = 0.5,
            EvidenceCost = 200,
            RequiredEvidence = 100
        },
        new Upgrade
        {
            Id = "red_marker",
            Name = "Red Marker",
            Description = "+1 click power",
            FlavorText = "For circling faces in newspapers.",
            Type = UpgradeType.ClickPower,
            Value = 1,
            EvidenceCost = 800,
            RequiredEvidence = 400
        },
        new Upgrade
        {
            Id = "cork_board",
            Name = "Premium Cork Board",
            Description = "+2 click power",
            FlavorText = "Industrial grade. Holds 47% more string.",
            Type = UpgradeType.ClickPower,
            Value = 2,
            EvidenceCost = 3000,
            RequiredEvidence = 1500
        },
        new Upgrade
        {
            Id = "night_vision",
            Name = "Night Vision Goggles",
            Description = "+4 click power",
            FlavorText = "For those 3 AM stakeouts.",
            Type = UpgradeType.ClickPower,
            Value = 4,
            EvidenceCost = 15000,
            RequiredEvidence = 7500
        },
        new Upgrade
        {
            Id = "mechanical_keyboard",
            Name = "Mechanical Keyboard",
            Description = "+8 click power",
            FlavorText = "CLACK CLACK CLACK. The sound of truth.",
            Type = UpgradeType.ClickPower,
            Value = 8,
            EvidenceCost = 75000,
            RequiredEvidence = 35000
        },

        // === CLICK MULTIPLIERS (small multipliers) ===
        new Upgrade
        {
            Id = "third_eye_drops",
            Name = "Third Eye Drops",
            Description = "x1.1 click power",
            FlavorText = "Side effects may include seeing through walls.",
            Type = UpgradeType.ClickMultiplier,
            Value = 1.1,
            EvidenceCost = 100000,
            RequiredEvidence = 50000
        },
        new Upgrade
        {
            Id = "caffeine_iv",
            Name = "Caffeine IV Drip",
            Description = "x1.15 click power",
            FlavorText = "Sleep is for the uninformed.",
            Type = UpgradeType.ClickMultiplier,
            Value = 1.15,
            EvidenceCost = 1000000,
            RequiredEvidence = 500000
        },
        new Upgrade
        {
            Id = "quantum_fingers",
            Name = "Quantum Fingers",
            Description = "x1.2 click power",
            FlavorText = "Click in multiple dimensions simultaneously.",
            Type = UpgradeType.ClickMultiplier,
            Value = 1.2,
            EvidenceCost = 10000000,
            RequiredEvidence = 5000000
        },

        // === EPS TO CLICK (harder - more expensive and higher requirements) ===
        new Upgrade
        {
            Id = "finger_on_pulse",
            Name = "Finger on the Pulse",
            Description = "+1% of EPS per click",
            FlavorText = "Feel the truth flowing through you.",
            Type = UpgradeType.EpsToClick,
            Value = 0.01,
            EvidenceCost = 50000,
            RequiredEvidence = 25000
        },
        new Upgrade
        {
            Id = "active_investigation",
            Name = "Active Investigation",
            Description = "+2% of EPS per click",
            FlavorText = "The more you know, the more each discovery matters.",
            Type = UpgradeType.EpsToClick,
            Value = 0.02,
            EvidenceCost = 500000,
            RequiredEvidence = 250000
        },
        new Upgrade
        {
            Id = "momentum_theory",
            Name = "Momentum Theory",
            Description = "+3% of EPS per click",
            FlavorText = "Your passive income amplifies your active work.",
            Type = UpgradeType.EpsToClick,
            Value = 0.03,
            EvidenceCost = 5000000,
            RequiredEvidence = 2500000
        },
        new Upgrade
        {
            Id = "synergy_doctrine",
            Name = "Synergy Doctrine",
            Description = "+4% of EPS per click",
            FlavorText = "Everything is connected. Especially your income.",
            Type = UpgradeType.EpsToClick,
            Value = 0.04,
            EvidenceCost = 50000000,
            RequiredEvidence = 25000000
        },
        new Upgrade
        {
            Id = "unified_field",
            Name = "Unified Field Theory",
            Description = "+5% of EPS per click",
            FlavorText = "Tesla knew. Now you do too.",
            Type = UpgradeType.EpsToClick,
            Value = 0.05,
            EvidenceCost = 500000000,
            RequiredEvidence = 250000000
        },
        new Upgrade
        {
            Id = "clickchain_reaction",
            Name = "Click-Chain Reaction",
            Description = "+7% of EPS per click",
            FlavorText = "Each click triggers a cascade of truth.",
            Type = UpgradeType.EpsToClick,
            Value = 0.07,
            EvidenceCost = 5000000000,
            RequiredEvidence = 2500000000
        },
        new Upgrade
        {
            Id = "infinite_recursion",
            Name = "Infinite Recursion",
            Description = "+10% of EPS per click",
            FlavorText = "It's clicks all the way down.",
            Type = UpgradeType.EpsToClick,
            Value = 0.10,
            EvidenceCost = 50000000000,
            RequiredEvidence = 25000000000
        },

        // === GENERATOR BOOSTS ===
        new Upgrade
        {
            Id = "premium_string",
            Name = "Premium Red String",
            Description = "Red String production x2",
            FlavorText = "Egyptian cotton. Very connected.",
            Type = UpgradeType.GeneratorBoost,
            Value = 2.0,
            TargetGeneratorId = "red_string",
            EvidenceCost = 1000,
            RequiredGeneratorCount = 10,
            RequiredGeneratorId = "red_string"
        },
        new Upgrade
        {
            Id = "neighborhood_watch",
            Name = "Neighborhood Watch",
            Description = "Suspicious Neighbor production x2",
            FlavorText = "Now they're ALL watching.",
            Type = UpgradeType.GeneratorBoost,
            Value = 2.0,
            TargetGeneratorId = "suspicious_neighbor",
            EvidenceCost = 5000,
            RequiredGeneratorCount = 10,
            RequiredGeneratorId = "suspicious_neighbor"
        },
        new Upgrade
        {
            Id = "ergonomic_chair",
            Name = "Ergonomic Gaming Chair",
            Description = "Basement Researcher production x2",
            FlavorText = "For those 18-hour research sessions.",
            Type = UpgradeType.GeneratorBoost,
            Value = 2.0,
            TargetGeneratorId = "basement_researcher",
            EvidenceCost = 25000,
            RequiredGeneratorCount = 10,
            RequiredGeneratorId = "basement_researcher"
        },
        new Upgrade
        {
            Id = "seo_optimization",
            Name = "SEO Optimization",
            Description = "Blogspot Blog production x2",
            FlavorText = "Page 1 of Google for 'chemtrails real proof'.",
            Type = UpgradeType.GeneratorBoost,
            Value = 2.0,
            TargetGeneratorId = "blogspot_blog",
            EvidenceCost = 150000,
            RequiredGeneratorCount = 10,
            RequiredGeneratorId = "blogspot_blog"
        },
        new Upgrade
        {
            Id = "clickbait_thumbnails",
            Name = "Clickbait Thumbnails",
            Description = "YouTube Channel production x2",
            FlavorText = "Red arrows pointing at EVERYTHING.",
            Type = UpgradeType.GeneratorBoost,
            Value = 2.0,
            TargetGeneratorId = "youtube_channel",
            EvidenceCost = 1500000,
            RequiredGeneratorCount = 10,
            RequiredGeneratorId = "youtube_channel"
        },

        // === LATE GAME EARLY GENERATOR MULTIPLIERS (keep early gens relevant) ===
        new Upgrade
        {
            Id = "red_string_quantum",
            Name = "Quantum Entangled String",
            Description = "Red String production x5",
            FlavorText = "Connected across dimensions now.",
            Type = UpgradeType.GeneratorBoost,
            Value = 5.0,
            TargetGeneratorId = "red_string",
            EvidenceCost = 500000,
            RequiredGeneratorCount = 25,
            RequiredGeneratorId = "red_string"
        },
        new Upgrade
        {
            Id = "red_string_infinite",
            Name = "Infinite String Theory",
            Description = "Red String production x10",
            FlavorText = "Every string leads to every other string.",
            Type = UpgradeType.GeneratorBoost,
            Value = 10.0,
            TargetGeneratorId = "red_string",
            EvidenceCost = 50000000,
            RequiredGeneratorCount = 50,
            RequiredGeneratorId = "red_string"
        },
        new Upgrade
        {
            Id = "neighbor_network",
            Name = "Global Neighbor Network",
            Description = "Suspicious Neighbor production x5",
            FlavorText = "Neighbors watching neighbors watching neighbors.",
            Type = UpgradeType.GeneratorBoost,
            Value = 5.0,
            TargetGeneratorId = "suspicious_neighbor",
            EvidenceCost = 1000000,
            RequiredGeneratorCount = 25,
            RequiredGeneratorId = "suspicious_neighbor"
        },
        new Upgrade
        {
            Id = "neighbor_hivemind",
            Name = "Neighborhood Hivemind",
            Description = "Suspicious Neighbor production x10",
            FlavorText = "They share one brain now. Gary's brain.",
            Type = UpgradeType.GeneratorBoost,
            Value = 10.0,
            TargetGeneratorId = "suspicious_neighbor",
            EvidenceCost = 100000000,
            RequiredGeneratorCount = 50,
            RequiredGeneratorId = "suspicious_neighbor"
        },
        new Upgrade
        {
            Id = "researcher_ascension",
            Name = "Researcher Ascension",
            Description = "Basement Researcher production x5",
            FlavorText = "They've achieved a higher state of paranoia.",
            Type = UpgradeType.GeneratorBoost,
            Value = 5.0,
            TargetGeneratorId = "basement_researcher",
            EvidenceCost = 5000000,
            RequiredGeneratorCount = 25,
            RequiredGeneratorId = "basement_researcher"
        },
        new Upgrade
        {
            Id = "researcher_transcendence",
            Name = "Researcher Transcendence",
            Description = "Basement Researcher production x10",
            FlavorText = "No longer need eyes to see the truth.",
            Type = UpgradeType.GeneratorBoost,
            Value = 10.0,
            TargetGeneratorId = "basement_researcher",
            EvidenceCost = 500000000,
            RequiredGeneratorCount = 50,
            RequiredGeneratorId = "basement_researcher"
        },
        new Upgrade
        {
            Id = "blog_empire",
            Name = "Blogspot Empire",
            Description = "Blogspot Blog production x5",
            FlavorText = "A network of interconnected truth hubs.",
            Type = UpgradeType.GeneratorBoost,
            Value = 5.0,
            TargetGeneratorId = "blogspot_blog",
            EvidenceCost = 25000000,
            RequiredGeneratorCount = 25,
            RequiredGeneratorId = "blogspot_blog"
        },
        new Upgrade
        {
            Id = "blog_singularity",
            Name = "Blog Singularity",
            Description = "Blogspot Blog production x10",
            FlavorText = "All blogs have merged into one mega-truth.",
            Type = UpgradeType.GeneratorBoost,
            Value = 10.0,
            TargetGeneratorId = "blogspot_blog",
            EvidenceCost = 2500000000,
            RequiredGeneratorCount = 50,
            RequiredGeneratorId = "blogspot_blog"
        },
        new Upgrade
        {
            Id = "youtube_algorithm",
            Name = "Algorithm Manipulation",
            Description = "YouTube Channel production x5",
            FlavorText = "The algorithm works FOR you now.",
            Type = UpgradeType.GeneratorBoost,
            Value = 5.0,
            TargetGeneratorId = "youtube_channel",
            EvidenceCost = 100000000,
            RequiredGeneratorCount = 25,
            RequiredGeneratorId = "youtube_channel"
        },
        new Upgrade
        {
            Id = "youtube_monopoly",
            Name = "Truth Tube Monopoly",
            Description = "YouTube Channel production x10",
            FlavorText = "You ARE the recommended section.",
            Type = UpgradeType.GeneratorBoost,
            Value = 10.0,
            TargetGeneratorId = "youtube_channel",
            EvidenceCost = 10000000000,
            RequiredGeneratorCount = 50,
            RequiredGeneratorId = "youtube_channel"
        },

        // === GLOBAL BOOSTS ===
        new Upgrade
        {
            Id = "viral_momentum",
            Name = "Viral Momentum",
            Description = "All generators +10%",
            FlavorText = "The algorithm favors the bold.",
            Type = UpgradeType.GlobalBoost,
            Value = 1.10,
            EvidenceCost = 10000000,
            RequiredEvidence = 5000000
        },
        new Upgrade
        {
            Id = "mass_awakening",
            Name = "Mass Awakening",
            Description = "All generators +25%",
            FlavorText = "They're finally listening.",
            Type = UpgradeType.GlobalBoost,
            Value = 1.25,
            EvidenceCost = 100000000,
            RequiredEvidence = 50000000
        },
        new Upgrade
        {
            Id = "truth_singularity",
            Name = "Truth Singularity",
            Description = "All generators +50%",
            FlavorText = "Critical mass achieved.",
            Type = UpgradeType.GlobalBoost,
            Value = 1.50,
            EvidenceCost = 1000000000,
            RequiredEvidence = 500000000
        },

        // === MORE CLICK POWER UPGRADES (small values to stay at ~10% of EPS) ===
        new Upgrade
        {
            Id = "encrypted_usb",
            Name = "Encrypted USB Drive",
            Description = "+15 click power",
            FlavorText = "Found in a parking lot. Totally safe to plug in.",
            Type = UpgradeType.ClickPower,
            Value = 15,
            EvidenceCost = 250000,
            RequiredEvidence = 125000
        },
        new Upgrade
        {
            Id = "burner_phone",
            Name = "Burner Phone Collection",
            Description = "+30 click power",
            FlavorText = "One for each personality.",
            Type = UpgradeType.ClickPower,
            Value = 30,
            EvidenceCost = 1000000,
            RequiredEvidence = 500000
        },
        new Upgrade
        {
            Id = "satellite_dish",
            Name = "DIY Satellite Dish",
            Description = "+60 click power",
            FlavorText = "Made from a wok and coat hangers.",
            Type = UpgradeType.ClickPower,
            Value = 60,
            EvidenceCost = 5000000,
            RequiredEvidence = 2500000
        },
        new Upgrade
        {
            Id = "black_light",
            Name = "Industrial Black Light",
            Description = "+120 click power",
            FlavorText = "Reveals hidden messages. And unfortunate stains.",
            Type = UpgradeType.ClickPower,
            Value = 120,
            EvidenceCost = 25000000,
            RequiredEvidence = 12500000
        },
        new Upgrade
        {
            Id = "evidence_vault",
            Name = "Evidence Vault",
            Description = "+250 click power",
            FlavorText = "Fireproof, waterproof, government-proof.",
            Type = UpgradeType.ClickPower,
            Value = 250,
            EvidenceCost = 100000000,
            RequiredEvidence = 50000000
        },
        new Upgrade
        {
            Id = "neural_enhancer",
            Name = "Neural Pattern Enhancer",
            Description = "+500 click power",
            FlavorText = "See connections others can't. Literally.",
            Type = UpgradeType.ClickPower,
            Value = 500,
            EvidenceCost = 500000000,
            RequiredEvidence = 250000000
        },

        // === MORE CLICK MULTIPLIERS (small multipliers) ===
        new Upgrade
        {
            Id = "truth_serum",
            Name = "Truth Serum",
            Description = "x1.25 click power",
            FlavorText = "Works on you too, unfortunately.",
            Type = UpgradeType.ClickMultiplier,
            Value = 1.25,
            EvidenceCost = 50000000,
            RequiredEvidence = 25000000
        },
        new Upgrade
        {
            Id = "reality_distortion",
            Name = "Reality Distortion Field",
            Description = "x1.3 click power",
            FlavorText = "Bend truth to your will.",
            Type = UpgradeType.ClickMultiplier,
            Value = 1.3,
            EvidenceCost = 500000000,
            RequiredEvidence = 250000000
        },

        // === MORE GENERATOR BOOSTS (Tier 2) ===
        new Upgrade
        {
            Id = "premium_string_2",
            Name = "Quantum Entangled String",
            Description = "Red String production x3",
            FlavorText = "Connected across space and time.",
            Type = UpgradeType.GeneratorBoost,
            Value = 3.0,
            TargetGeneratorId = "red_string",
            EvidenceCost = 100000,
            RequiredGeneratorCount = 50,
            RequiredGeneratorId = "red_string"
        },
        new Upgrade
        {
            Id = "neighborhood_watch_2",
            Name = "Neighborhood Surveillance Network",
            Description = "Suspicious Neighbor production x3",
            FlavorText = "Ring doorbells. Ring doorbells everywhere.",
            Type = UpgradeType.GeneratorBoost,
            Value = 3.0,
            TargetGeneratorId = "suspicious_neighbor",
            EvidenceCost = 500000,
            RequiredGeneratorCount = 50,
            RequiredGeneratorId = "suspicious_neighbor"
        },
        new Upgrade
        {
            Id = "ergonomic_chair_2",
            Name = "Gamer Pod Station",
            Description = "Basement Researcher production x3",
            FlavorText = "Complete isolation. Just how they like it.",
            Type = UpgradeType.GeneratorBoost,
            Value = 3.0,
            TargetGeneratorId = "basement_researcher",
            EvidenceCost = 2500000,
            RequiredGeneratorCount = 50,
            RequiredGeneratorId = "basement_researcher"
        },
        new Upgrade
        {
            Id = "seo_optimization_2",
            Name = "Algorithm Gaming",
            Description = "Blogspot Blog production x3",
            FlavorText = "The bots work for YOU now.",
            Type = UpgradeType.GeneratorBoost,
            Value = 3.0,
            TargetGeneratorId = "blogspot_blog",
            EvidenceCost = 15000000,
            RequiredGeneratorCount = 50,
            RequiredGeneratorId = "blogspot_blog"
        },
        new Upgrade
        {
            Id = "clickbait_thumbnails_2",
            Name = "AI Thumbnail Generator",
            Description = "YouTube Channel production x3",
            FlavorText = "Every thumbnail is SHOCKED.",
            Type = UpgradeType.GeneratorBoost,
            Value = 3.0,
            TargetGeneratorId = "youtube_channel",
            EvidenceCost = 75000000,
            RequiredGeneratorCount = 50,
            RequiredGeneratorId = "youtube_channel"
        },

        // === DISCORD AND BEYOND GENERATOR BOOSTS ===
        new Upgrade
        {
            Id = "discord_bots",
            Name = "Discord Bot Army",
            Description = "Discord Server production x2",
            FlavorText = "24/7 automated truthing.",
            Type = UpgradeType.GeneratorBoost,
            Value = 2.0,
            TargetGeneratorId = "discord_server",
            EvidenceCost = 10000000,
            RequiredGeneratorCount = 10,
            RequiredGeneratorId = "discord_server"
        },
        new Upgrade
        {
            Id = "am_radio_tower",
            Name = "Boosted AM Tower",
            Description = "AM Radio Show production x2",
            FlavorText = "Now reaching 47 states!",
            Type = UpgradeType.GeneratorBoost,
            Value = 2.0,
            TargetGeneratorId = "am_radio",
            EvidenceCost = 50000000,
            RequiredGeneratorCount = 10,
            RequiredGeneratorId = "am_radio"
        },
        new Upgrade
        {
            Id = "podcast_sponsorships",
            Name = "Suspicious Sponsorships",
            Description = "4-Hour Podcast production x2",
            FlavorText = "This episode brought to you by...",
            Type = UpgradeType.GeneratorBoost,
            Value = 2.0,
            TargetGeneratorId = "podcast",
            EvidenceCost = 250000000,
            RequiredGeneratorCount = 10,
            RequiredGeneratorId = "podcast"
        },
        new Upgrade
        {
            Id = "conference_keynotes",
            Name = "Keynote Speaking Tour",
            Description = "Truth Conference production x2",
            FlavorText = "Standing ovations guaranteed.",
            Type = UpgradeType.GeneratorBoost,
            Value = 2.0,
            TargetGeneratorId = "truth_conference",
            EvidenceCost = 1500000000,
            RequiredGeneratorCount = 10,
            RequiredGeneratorId = "truth_conference"
        },
        new Upgrade
        {
            Id = "netflix_promotion",
            Name = "Netflix Algorithm Hack",
            Description = "Netflix Documentary production x2",
            FlavorText = "Trending #1 in 47 countries.",
            Type = UpgradeType.GeneratorBoost,
            Value = 2.0,
            TargetGeneratorId = "netflix_documentary",
            EvidenceCost = 10000000000,
            RequiredGeneratorCount = 5,
            RequiredGeneratorId = "netflix_documentary"
        },
        new Upgrade
        {
            Id = "satellite_network",
            Name = "Orbital Network",
            Description = "Spy Satellite production x2",
            FlavorText = "Complete global coverage.",
            Type = UpgradeType.GeneratorBoost,
            Value = 2.0,
            TargetGeneratorId = "spy_satellite",
            EvidenceCost = 75000000000,
            RequiredGeneratorCount = 5,
            RequiredGeneratorId = "spy_satellite"
        },
        new Upgrade
        {
            Id = "shadow_government_expansion",
            Name = "Shadow Government Expansion",
            Description = "Shadow Government production x2",
            FlavorText = "More shadows. More government.",
            Type = UpgradeType.GeneratorBoost,
            Value = 2.0,
            TargetGeneratorId = "shadow_government",
            EvidenceCost = 500000000000,
            RequiredGeneratorCount = 3,
            RequiredGeneratorId = "shadow_government"
        },

        // === MORE GLOBAL BOOSTS ===
        new Upgrade
        {
            Id = "collective_consciousness",
            Name = "Collective Consciousness",
            Description = "All generators +75%",
            FlavorText = "We are all connected.",
            Type = UpgradeType.GlobalBoost,
            Value = 1.75,
            EvidenceCost = 10000000000,
            RequiredEvidence = 5000000000
        },
        new Upgrade
        {
            Id = "great_revelation",
            Name = "The Great Revelation",
            Description = "All generators x2",
            FlavorText = "The truth shall set you free.",
            Type = UpgradeType.GlobalBoost,
            Value = 2.0,
            EvidenceCost = 100000000000,
            RequiredEvidence = 50000000000
        },
        new Upgrade
        {
            Id = "ascended_knowledge",
            Name = "Ascended Knowledge",
            Description = "All generators x3",
            FlavorText = "You have transcended.",
            Type = UpgradeType.GlobalBoost,
            Value = 3.0,
            EvidenceCost = 1000000000000,
            RequiredEvidence = 500000000000
        },

        // === POST-ASCENSION GENERATOR BOOSTS ===
        new Upgrade
        {
            Id = "mind_amplifier",
            Name = "Mind Amplifier",
            Description = "Mind Control Tower production x2",
            FlavorText = "Stronger signals. Weaker wills.",
            Type = UpgradeType.GeneratorBoost,
            Value = 2.0,
            TargetGeneratorId = "mind_control_tower",
            EvidenceCost = 500000000000000,
            RequiredGeneratorCount = 5,
            RequiredGeneratorId = "mind_control_tower"
        },
        new Upgrade
        {
            Id = "weather_dominance",
            Name = "Weather Dominance",
            Description = "Weather Machine production x2",
            FlavorText = "Sunny with a chance of conspiracy.",
            Type = UpgradeType.GeneratorBoost,
            Value = 2.0,
            TargetGeneratorId = "weather_machine",
            EvidenceCost = 15000000000000000,
            RequiredGeneratorCount = 5,
            RequiredGeneratorId = "weather_machine"
        },
        new Upgrade
        {
            Id = "clone_perfection",
            Name = "Clone Perfection",
            Description = "Clone Facility production x2",
            FlavorText = "Even their mothers can't tell the difference.",
            Type = UpgradeType.GeneratorBoost,
            Value = 2.0,
            TargetGeneratorId = "clone_facility",
            EvidenceCost = 500000000000000000,
            RequiredGeneratorCount = 3,
            RequiredGeneratorId = "clone_facility"
        },
        new Upgrade
        {
            Id = "temporal_mastery",
            Name = "Temporal Mastery",
            Description = "Time Machine production x2",
            FlavorText = "Yesterday's evidence delivered today.",
            Type = UpgradeType.GeneratorBoost,
            Value = 2.0,
            TargetGeneratorId = "time_machine",
            EvidenceCost = 20000000000000000000.0,
            RequiredGeneratorCount = 3,
            RequiredGeneratorId = "time_machine"
        },
        new Upgrade
        {
            Id = "inner_earth_network",
            Name = "Inner Earth Network",
            Description = "Hollow Earth Base production x2",
            FlavorText = "Tunnels to everywhere. And everywhen.",
            Type = UpgradeType.GeneratorBoost,
            Value = 2.0,
            TargetGeneratorId = "hollow_earth_base",
            EvidenceCost = 1000000000000000000000.0,
            RequiredGeneratorCount = 3,
            RequiredGeneratorId = "hollow_earth_base"
        },

        // === ULTRA LATE GAME GLOBAL BOOSTS ===
        new Upgrade
        {
            Id = "cosmic_awareness",
            Name = "Cosmic Awareness",
            Description = "All generators x5",
            FlavorText = "The universe reveals its secrets.",
            Type = UpgradeType.GlobalBoost,
            Value = 5.0,
            EvidenceCost = 100000000000000000000.0,
            RequiredEvidence = 50000000000000000000.0
        },
        new Upgrade
        {
            Id = "reality_manipulation",
            Name = "Reality Manipulation",
            Description = "All generators x10",
            FlavorText = "You don't discover truth. You create it.",
            Type = UpgradeType.GlobalBoost,
            Value = 10.0,
            EvidenceCost = 100000000000000000000000.0,
            RequiredEvidence = 50000000000000000000000.0
        },
        new Upgrade
        {
            Id = "omnipotent_understanding",
            Name = "Omnipotent Understanding",
            Description = "All generators x25",
            FlavorText = "All knowledge flows through you.",
            Type = UpgradeType.GlobalBoost,
            Value = 25.0,
            EvidenceCost = 100000000000000000000000000.0,
            RequiredEvidence = 50000000000000000000000000.0
        },
        new Upgrade
        {
            Id = "universe_mastery",
            Name = "Universe Mastery",
            Description = "All generators x100",
            FlavorText = "Congratulations. You ARE the conspiracy.",
            Type = UpgradeType.GlobalBoost,
            Value = 100.0,
            EvidenceCost = 100000000000000000000000000000.0,
            RequiredEvidence = 50000000000000000000000000000.0
        },

        // === EARLY GAME FILLERS (small values for ~10% EPS) ===
        new Upgrade
        {
            Id = "sticky_notes",
            Name = "Sticky Notes",
            Description = "+0.1 click power",
            FlavorText = "For important reminders like 'THEY'RE WATCHING'.",
            Type = UpgradeType.ClickPower,
            Value = 0.1,
            EvidenceCost = 15,
            RequiredEvidence = 5
        },
        new Upgrade
        {
            Id = "notebook",
            Name = "Spiral Notebook",
            Description = "+0.3 click power",
            FlavorText = "College ruled. Conspiracy filled.",
            Type = UpgradeType.ClickPower,
            Value = 0.3,
            EvidenceCost = 100,
            RequiredEvidence = 50
        },
        new Upgrade
        {
            Id = "flashlight",
            Name = "Heavy Duty Flashlight",
            Description = "+0.7 click power",
            FlavorText = "For illuminating dark corners. And faces.",
            Type = UpgradeType.ClickPower,
            Value = 0.7,
            EvidenceCost = 400,
            RequiredEvidence = 200
        },
        new Upgrade
        {
            Id = "binoculars",
            Name = "Surveillance Binoculars",
            Description = "+1.5 click power",
            FlavorText = "Bird watching. Definitely bird watching.",
            Type = UpgradeType.ClickPower,
            Value = 1.5,
            EvidenceCost = 1500,
            RequiredEvidence = 750
        },
        new Upgrade
        {
            Id = "scanner",
            Name = "Document Scanner",
            Description = "+3 click power",
            FlavorText = "Digitize ALL the evidence.",
            Type = UpgradeType.ClickPower,
            Value = 3,
            EvidenceCost = 6000,
            RequiredEvidence = 3000
        },
        new Upgrade
        {
            Id = "voice_recorder",
            Name = "Concealed Voice Recorder",
            Description = "+6 click power",
            FlavorText = "In pen form. Very subtle.",
            Type = UpgradeType.ClickPower,
            Value = 6,
            EvidenceCost = 30000,
            RequiredEvidence = 15000
        }
    };

    public static Upgrade? GetById(string id)
    {
        return AllUpgrades.FirstOrDefault(u => u.Id == id);
    }

    public static IEnumerable<Upgrade> GetAvailable(double evidence, Dictionary<string, int> generators, HashSet<string> purchased)
    {
        return AllUpgrades.Where(u =>
            !purchased.Contains(u.Id) &&
            evidence >= u.RequiredEvidence &&
            (u.RequiredGeneratorId == null ||
             (generators.TryGetValue(u.RequiredGeneratorId, out var count) && count >= u.RequiredGeneratorCount)));
    }
}
