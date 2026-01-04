using ConspiracyClicker.Models;

namespace ConspiracyClicker.Data;

public static class ConspiracyData
{
    public static readonly List<Conspiracy> AllConspiracies = new()
    {
        // === TIER 1: MUNDANE & SILLY (Grounded stuff people joke about) ===
        // WALL DESIGN: Conspiracy 7 (Lizard People) is the HARD WALL
        // Without Illuminati multipliers, reaching 100B+ is extremely slow
        // First 4 conspiracies: accessible (~1-2h each without ascension)
        // Conspiracies 5-7: slow grind (~3-5h each) - player should ascend here
        // Conspiracies 8+: require Illuminati multipliers to be practical
        new Conspiracy
        {
            Id = "birds_arent_real",
            Name = "Birds Aren't Real",
            Description = "Prove that birds are government surveillance drones",
            FlavorText = "That explains why they sit on power lines. Recharging.",
            EvidenceCost = 500000, // First conspiracy - achievable in ~2-3h
            ClickBonus = 1,
            TinfoilReward = 1,
            Icon = "🐦"
        },
        new Conspiracy
        {
            Id = "mattress_laundering",
            Name = "Mattress Store Money Laundering",
            Description = "Expose the mattress store conspiracy",
            FlavorText = "Nobody buys that many mattresses. NOBODY.",
            EvidenceCost = 8000000, // 16x - first wall starting
            ClickBonus = 2,
            TinfoilReward = 2,
            Icon = "🛏️"
        },
        new Conspiracy
        {
            Id = "moon_landing",
            Name = "Moon Landing Faked",
            Description = "Prove Kubrick directed the moon landing",
            FlavorText = "He insisted on 127 takes of the flag planting.",
            EvidenceCost = 150000000, // 18x - walls getting harder
            ClickBonus = 5,
            TinfoilReward = 3,
            Icon = "🎬"
        },
        new Conspiracy
        {
            Id = "flat_earth",
            Name = "Flat Earth",
            Description = "Prove the Earth is actually flat",
            FlavorText = "Like a pancake. A delicious, controversial pancake.",
            EvidenceCost = 3500000000, // 23x - steeper wall
            ClickBonus = 10,
            TinfoilReward = 5,
            Icon = "🥞"
        },
        new Conspiracy
        {
            Id = "australia_fake",
            Name = "Australia Doesn't Exist",
            Description = "Prove Australia is a hologram",
            FlavorText = "That's why everything there 'kills you'. It's a warning.",
            EvidenceCost = 100000000000, // 100B - ASCENSION THRESHOLD
            ClickBonus = 20,
            TinfoilReward = 8,
            Icon = "🦘"
        },
        new Conspiracy
        {
            Id = "finland_myth",
            Name = "Finland is a Myth",
            Description = "Prove Finland was invented by Japan and Russia",
            FlavorText = "For fish-related tax purposes. Obviously.",
            EvidenceCost = 3500000000000, // 3.5T - HARD WALL without Illuminati
            ClickBonus = 40,
            TinfoilReward = 12,
            Icon = "🐟"
        },
        new Conspiracy
        {
            Id = "lizard_people",
            Name = "Lizard People",
            Description = "Expose the reptilian elite",
            FlavorText = "Explains Congress's cold-blooded voting patterns.",
            EvidenceCost = 120000000000000, // 120T - nearly impossible without ascension
            ClickBonus = 80,
            TinfoilReward = 20,
            Icon = "🦎"
        },
        new Conspiracy
        {
            Id = "denver_airport",
            Name = "Denver Airport Underground",
            Description = "Expose the secret underground bunker city",
            FlavorText = "Those murals aren't 'art'. They're blueprints.",
            EvidenceCost = 5000000000000000, // 5 quadrillion
            ClickBonus = 150,
            TinfoilReward = 35,
            Icon = "✈️"
        },
        new Conspiracy
        {
            Id = "antarctica_treaty",
            Name = "Antarctica Treaty Secret",
            Description = "Prove what they're really hiding in Antarctica",
            FlavorText = "All countries agree on ONE thing? Come on.",
            EvidenceCost = 200000000000000000, // 200 quadrillion
            ClickBonus = 300,
            TinfoilReward = 50,
            Icon = "🧊"
        },
        new Conspiracy
        {
            Id = "you_are_conspiracy",
            Name = "You ARE the Conspiracy",
            Description = "The ultimate truth",
            FlavorText = "The call was coming from inside the house. You are the deep state.",
            EvidenceCost = 10000000000000000000.0, // 10 quintillion
            ClickBonus = 0,
            MultiplierBonus = 2.0,
            TinfoilReward = 100,
            Icon = "👁️"
        },

        // === TIER 2: HIDDEN HISTORY & STRANGE (Getting weirder) ===
        // Post-ascension content - requires Illuminati multipliers (25x+ boost)
        new Conspiracy
        {
            Id = "tartaria",
            Name = "Tartarian Empire Cover-Up",
            Description = "Prove the hidden mega-civilization",
            FlavorText = "Those 'old buildings' are only 100 years old. Think about it.",
            EvidenceCost = 600000000000000000000.0, // 600 quintillion
            ClickBonus = 500,
            TinfoilReward = 150,
            Icon = "🏛️"
        },
        new Conspiracy
        {
            Id = "hollow_moon",
            Name = "The Moon is Hollow",
            Description = "Expose the artificial moon structure",
            FlavorText = "Why does it ring like a bell? EXACTLY.",
            EvidenceCost = 40000000000000000000000.0, // 40 sextillion
            ClickBonus = 800,
            MultiplierBonus = 1.5,
            TinfoilReward = 200,
            Icon = "🌑"
        },
        new Conspiracy
        {
            Id = "mandela_effect",
            Name = "Mandela Effect is Real",
            Description = "Prove timeline alterations are occurring",
            FlavorText = "It WAS Berenstain. In YOUR timeline.",
            EvidenceCost = 3000000000000000000000000.0, // 3 septillion
            ClickBonus = 1200,
            TinfoilReward = 300,
            Icon = "🔀"
        },
        new Conspiracy
        {
            Id = "time_invention",
            Name = "Time is a Government Invention",
            Description = "Prove time was invented for control",
            FlavorText = "That's why Mondays feel so long. Wake up.",
            EvidenceCost = 250000000000000000000000000.0, // 250 septillion
            ClickBonus = 2000,
            MultiplierBonus = 2.0,
            TinfoilReward = 500,
            Icon = "⏰"
        },
        new Conspiracy
        {
            Id = "breakaway_civilization",
            Name = "Breakaway Civilization",
            Description = "Expose the secret space society",
            FlavorText = "They left Earth in the 50s. We're the ones left behind.",
            EvidenceCost = 20000000000000000000000000000.0, // 20 octillion
            ClickBonus = 3500,
            MultiplierBonus = 2.5,
            TinfoilReward = 750,
            Icon = "🚀"
        },

        // === TIER 3: REALITY BREAKING (Cosmic/Metaphysical) ===
        new Conspiracy
        {
            Id = "cern_portal",
            Name = "CERN Opened a Portal",
            Description = "Prove CERN is punching holes in reality",
            FlavorText = "They said it was 'particle physics'. Sure, Jan.",
            EvidenceCost = 1500000000000000000000000000000.0, // 1.5 nonillion
            ClickBonus = 5000,
            TinfoilReward = 1000,
            Icon = "⚛️"
        },
        new Conspiracy
        {
            Id = "simulation",
            Name = "Reality is a Simulation",
            Description = "Prove we live in a simulation",
            FlavorText = "This game is proof. You're clicking in a simulation about clicking.",
            EvidenceCost = 120000000000000000000000000000000.0, // 120 nonillion
            ClickBonus = 8000,
            MultiplierBonus = 3.0,
            TinfoilReward = 1500,
            Icon = "🖥️"
        },
        new Conspiracy
        {
            Id = "matrix_source_code",
            Name = "Matrix Source Code",
            Description = "Access the simulation's underlying code",
            FlavorText = "Easter eggs everywhere. The devs got lazy.",
            EvidenceCost = 10000000000000000000000000000000000.0, // 10 decillion
            ClickBonus = 12000,
            TinfoilReward = 2500,
            Icon = "💻"
        },
        new Conspiracy
        {
            Id = "akashic_records",
            Name = "Akashic Records Access",
            Description = "Unlock the universe's memory banks",
            FlavorText = "Ctrl+F for 'who shot JFK' returns 47 results.",
            EvidenceCost = 800000000000000000000000000000000000.0, // 800 decillion
            ClickBonus = 20000,
            MultiplierBonus = 4.0,
            TinfoilReward = 4000,
            Icon = "📚"
        },
        new Conspiracy
        {
            Id = "god_committee",
            Name = "The God Committee",
            Description = "Expose who really runs reality",
            FlavorText = "Turns out it's a subcommittee. With terrible minutes.",
            EvidenceCost = 65000000000000000000000000000000000000.0, // 65 undecillion
            ClickBonus = 35000,
            TinfoilReward = 6000,
            Icon = "⚖️"
        },

        // === TIER 4: COSMIC TRUTH (Ultimate revelations) ===
        new Conspiracy
        {
            Id = "universe_experiment",
            Name = "Universe is an Experiment",
            Description = "Prove our universe is a science project",
            FlavorText = "We got a B-. The black holes are 'stylistic choices'.",
            EvidenceCost = 5000000000000000000000000000000000000000.0, // 5 duodecillion
            ClickBonus = 60000,
            MultiplierBonus = 5.0,
            TinfoilReward = 10000,
            Icon = "🔬"
        },
        new Conspiracy
        {
            Id = "multiverse_conspiracy",
            Name = "Multiverse Conspiracy",
            Description = "Prove infinite cover-ups across infinite realities",
            FlavorText = "In every timeline, they're lying. Consistency!",
            EvidenceCost = 400000000000000000000000000000000000000000.0, // 400 duodecillion
            ClickBonus = 100000,
            TinfoilReward = 15000,
            Icon = "🌌"
        },
        new Conspiracy
        {
            Id = "consciousness_prison",
            Name = "Consciousness Prison",
            Description = "Prove reality is a soul trap",
            FlavorText = "Reincarnation is just respawning. The grind never ends.",
            EvidenceCost = 30000000000000000000000000000000000000000000.0, // 30 tredecillion
            ClickBonus = 175000,
            MultiplierBonus = 7.5,
            TinfoilReward = 25000,
            Icon = "🔗"
        },
        new Conspiracy
        {
            Id = "truth_singularity",
            Name = "Truth Singularity",
            Description = "Achieve infinite conspiracy awareness",
            FlavorText = "You've seen too much. You've seen EVERYTHING.",
            EvidenceCost = 2500000000000000000000000000000000000000000000.0, // 2.5 quattuordecillion
            ClickBonus = 300000,
            TinfoilReward = 40000,
            Icon = "✨"
        },
        new Conspiracy
        {
            Id = "final_revelation",
            Name = "The Final Revelation",
            Description = "Become one with the conspiracy",
            FlavorText = "Congratulations. You are now a conspiracy theory about yourself.",
            EvidenceCost = 200000000000000000000000000000000000000000000000.0, // 200 quattuordecillion
            ClickBonus = 500000,
            MultiplierBonus = 10.0,
            TinfoilReward = 100000,
            Icon = "∞"
        }
    };

    public static Conspiracy? GetById(string id)
    {
        return AllConspiracies.FirstOrDefault(c => c.Id == id);
    }

    public static IEnumerable<Conspiracy> GetAvailable(double evidence, HashSet<string> proven)
    {
        // Show all unproven conspiracies at all times
        return AllConspiracies.Where(c => !proven.Contains(c.Id));
    }
}
