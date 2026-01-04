namespace ConspiracyClicker.Data;

public class IlluminatiUpgrade
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required string FlavorText { get; init; }
    public required int TokenCost { get; init; }
    public string Icon { get; init; } = "△";
}

public static class PrestigeData
{
    // LOGARITHMIC TOKEN SYSTEM - inspired by Clicker Heroes
    // First ascension at ~2h with 50K evidence = 1 token
    // Each ~3x more evidence = +1 token (diminishing returns)
    // Pushing 10x further only gives ~2 more tokens, encouraging frequent ascensions
    public const double PRESTIGE_THRESHOLD = 50_000; // 50K - achievable in ~1.5-2h of play
    public const double TOKEN_LOG_BASE = 3.0; // Every 3x evidence = roughly +1 token
    public const double TOKEN_MULTIPLIER = 1.0; // Scaling factor

    public static readonly List<IlluminatiUpgrade> IlluminatiUpgrades = new()
    {
        // === TIER 1: FIRST ASCENSION (1-3 tokens) ===
        // Even 1 token should feel AMAZING - massive boost to encourage ascending
        // First ascension with 1-2 tokens = 100x+ boost, completely changes the game
        new IlluminatiUpgrade
        {
            Id = "pyramid_scheme",
            Name = "Pyramid Scheme",
            Description = "x100 evidence per second permanently",
            FlavorText = "It's not a pyramid, it's a triangle of opportunity.",
            TokenCost = 1,
            Icon = "△"
        },
        new IlluminatiUpgrade
        {
            Id = "reptilian_dna",
            Name = "Reptilian DNA Injection",
            Description = "x100 evidence per second permanently",
            FlavorText = "Side effects may include: cold blood, forked tongue, ruling the world.",
            TokenCost = 2,
            Icon = "🦎"
        },
        new IlluminatiUpgrade
        {
            Id = "secret_handshake",
            Name = "Secret Handshake",
            Description = "x50 click power permanently",
            FlavorText = "Three shakes, two winks, one nod.",
            TokenCost = 2,
            Icon = "🤝"
        },
        new IlluminatiUpgrade
        {
            Id = "new_world_order_discount",
            Name = "New World Order Discount",
            Description = "-90% generator costs permanently",
            FlavorText = "Member benefits include: world domination discounts.",
            TokenCost = 3,
            Icon = "🌐"
        },
        new IlluminatiUpgrade
        {
            Id = "deep_state_connections",
            Name = "Deep State Connections",
            Description = "x50 evidence per second permanently",
            FlavorText = "It's not what you know, it's who you know in the shadow government.",
            TokenCost = 3,
            Icon = "🕴️"
        },
        // === TIER 2: SECOND ASCENSION (4-10 tokens) ===
        // 2nd ascension with 4-8 tokens = another 100-500x on top of first
        new IlluminatiUpgrade
        {
            Id = "ancient_knowledge",
            Name = "Ancient Knowledge",
            Description = "x100 evidence per second permanently",
            FlavorText = "Secrets from the Library of Alexandria.",
            TokenCost = 4,
            Icon = "📜"
        },
        new IlluminatiUpgrade
        {
            Id = "auto_clicker",
            Name = "Automated Truth Dispenser",
            Description = "+20 automatic clicks per second",
            FlavorText = "Why click yourself when the machines can do it?",
            TokenCost = 4,
            Icon = "🤖"
        },
        new IlluminatiUpgrade
        {
            Id = "moon_base_alpha",
            Name = "Moon Base Alpha Access",
            Description = "+500% quest rewards permanently",
            FlavorText = "The dark side has excellent Wi-Fi.",
            TokenCost = 5,
            Icon = "🌙"
        },
        new IlluminatiUpgrade
        {
            Id = "time_manipulation",
            Name = "Time Manipulation Device",
            Description = "-90% quest duration permanently",
            FlavorText = "Wibbly wobbly, timey wimey.",
            TokenCost = 5,
            Icon = "⏰"
        },
        new IlluminatiUpgrade
        {
            Id = "golden_eye_magnetism",
            Name = "Golden Eye Magnetism",
            Description = "Golden Eyes appear 5x more often and give 10x rewards",
            FlavorText = "The all-seeing eye sees you... and likes what it sees.",
            TokenCost = 6,
            Icon = "👁️‍🗨️"
        },
        new IlluminatiUpgrade
        {
            Id = "believer_magnetism",
            Name = "Believer Magnetism",
            Description = "+500% believers from all sources",
            FlavorText = "They can't help but follow you.",
            TokenCost = 6,
            Icon = "🧲"
        },
        new IlluminatiUpgrade
        {
            Id = "mind_control_mastery",
            Name = "Mind Control Mastery",
            Description = "Believers work 5x faster on quests",
            FlavorText = "They don't just believe. They OBEY.",
            TokenCost = 7,
            Icon = "🧠"
        },
        new IlluminatiUpgrade
        {
            Id = "all_seeing_investment",
            Name = "All-Seeing Investment",
            Description = "+25% evidence per Illuminati Token owned",
            FlavorText = "Your tokens are always watching... your profits grow.",
            TokenCost = 8,
            Icon = "👁"
        },
        new IlluminatiUpgrade
        {
            Id = "infinite_tinfoil",
            Name = "Infinite Tinfoil Supply",
            Description = "+200 Tinfoil per minute passively",
            FlavorText = "The Illuminati has excellent suppliers.",
            TokenCost = 10,
            Icon = "◇"
        },
        // === TIER 3: MULTIPLE ASCENSIONS (12-30 tokens) ===
        // By now player has ascended 3-5 times, total multiplier ~100,000x+
        new IlluminatiUpgrade
        {
            Id = "third_eye_awakening",
            Name = "Third Eye Awakening",
            Description = "+100% critical hit chance, crits deal 10x more",
            FlavorText = "See beyond the veil of lies.",
            TokenCost = 12,
            Icon = "👁️"
        },
        new IlluminatiUpgrade
        {
            Id = "instant_indoctrination",
            Name = "Instant Indoctrination",
            Description = "50% of quests complete instantly",
            FlavorText = "Who needs time when you have mind control?",
            TokenCost = 14,
            Icon = "⚡"
        },
        new IlluminatiUpgrade
        {
            Id = "shadow_network",
            Name = "Shadow Network",
            Description = "-95% all generator costs permanently",
            FlavorText = "Our agents are everywhere. Including wholesale.",
            TokenCost = 15,
            Icon = "🕸️"
        },
        new IlluminatiUpgrade
        {
            Id = "parallel_universe_access",
            Name = "Parallel Universe Access",
            Description = "x10 all generator production permanently",
            FlavorText = "In another timeline, you already won. Borrow their success.",
            TokenCost = 18,
            Icon = "🌌"
        },
        new IlluminatiUpgrade
        {
            Id = "reality_distortion",
            Name = "Reality Distortion Field",
            Description = "x100 click power permanently",
            FlavorText = "Bend reality to your will.",
            TokenCost = 20,
            Icon = "🌀"
        },
        new IlluminatiUpgrade
        {
            Id = "cosmic_alignment",
            Name = "Cosmic Alignment",
            Description = "x200 evidence per second permanently",
            FlavorText = "The stars finally agree with you.",
            TokenCost = 25,
            Icon = "✨"
        },
        new IlluminatiUpgrade
        {
            Id = "conspiracy_cascade",
            Name = "Conspiracy Cascade",
            Description = "Proving a conspiracy gives 10 minutes of 50x EPS",
            FlavorText = "One truth leads to another... exponentially.",
            TokenCost = 28,
            Icon = "🎭"
        },
        new IlluminatiUpgrade
        {
            Id = "global_awakening",
            Name = "Global Awakening",
            Description = "+2000% believers from all sources",
            FlavorText = "The world is ready for the truth.",
            TokenCost = 30,
            Icon = "🌍"
        },
        // === TIER 4: ENDGAME (40+ tokens) ===
        // Final tier - total EPS multiplier reaches billions
        new IlluminatiUpgrade
        {
            Id = "temporal_fold",
            Name = "Temporal Fold",
            Description = "-98% quest duration permanently",
            FlavorText = "Time is an illusion. Deadlines doubly so.",
            TokenCost = 40,
            Icon = "⌛"
        },
        new IlluminatiUpgrade
        {
            Id = "whistle_blower_network",
            Name = "Whistle-blower Network",
            Description = "Whistle-blowers give 500x evidence and spawn 10x more",
            FlavorText = "Leaks? We prefer 'strategic information releases'.",
            TokenCost = 50,
            Icon = "📢"
        },
        new IlluminatiUpgrade
        {
            Id = "illuminati_council_seat",
            Name = "Illuminati Council Seat",
            Description = "x500 evidence per second permanently",
            FlavorText = "Welcome to the inner circle.",
            TokenCost = 60,
            Icon = "👑"
        },
        new IlluminatiUpgrade
        {
            Id = "time_dilation_field",
            Name = "Time Dilation Field",
            Description = "All game timers run 5x faster",
            FlavorText = "When you control time, everything else is easy.",
            TokenCost = 75,
            Icon = "⏱️"
        },
        new IlluminatiUpgrade
        {
            Id = "omniscient_vision",
            Name = "Omniscient Vision",
            Description = "Quests always succeed and complete 80% faster",
            FlavorText = "You see all possible outcomes.",
            TokenCost = 100,
            Icon = "🔮"
        },
        new IlluminatiUpgrade
        {
            Id = "eternal_conspiracy",
            Name = "Eternal Conspiracy",
            Description = "x1000 evidence per second permanently",
            FlavorText = "Your conspiracy spans all of time.",
            TokenCost = 125,
            Icon = "∞"
        },
        new IlluminatiUpgrade
        {
            Id = "reality_overwrite",
            Name = "Reality Overwrite",
            Description = "x25 ALL production and click power",
            FlavorText = "You don't uncover the truth. You BECOME the truth.",
            TokenCost = 150,
            Icon = "💫"
        },

        // === TIER 5: TRANSCENDENT (200+ tokens) ===
        new IlluminatiUpgrade
        {
            Id = "entropy_mastery",
            Name = "Entropy Mastery",
            Description = "x2000 evidence per second permanently",
            FlavorText = "Disorder serves order. Your order.",
            TokenCost = 200,
            Icon = "🌡️"
        },
        new IlluminatiUpgrade
        {
            Id = "probability_control",
            Name = "Probability Control",
            Description = "Quests always succeed, complete 90% faster",
            FlavorText = "The odds are whatever you say they are.",
            TokenCost = 225,
            Icon = "🎲"
        },
        new IlluminatiUpgrade
        {
            Id = "tinfoil_transmutation",
            Name = "Tinfoil Transmutation",
            Description = "+2000 Tinfoil per minute passively",
            FlavorText = "Turn thoughts into tinfoil. Literally.",
            TokenCost = 250,
            Icon = "⚗️"
        },
        new IlluminatiUpgrade
        {
            Id = "believer_singularity",
            Name = "Believer Singularity",
            Description = "+25000% believers from all sources",
            FlavorText = "Every mind is your mind.",
            TokenCost = 275,
            Icon = "🧬"
        },
        new IlluminatiUpgrade
        {
            Id = "click_transcendence",
            Name = "Click Transcendence",
            Description = "x500 click power, +200 auto-clicks/sec",
            FlavorText = "Your clicks echo through reality.",
            TokenCost = 300,
            Icon = "👆"
        },

        // === TIER 6: OMEGA (350+ tokens) ===
        new IlluminatiUpgrade
        {
            Id = "evidence_singularity",
            Name = "Evidence Singularity",
            Description = "x5000 evidence per second permanently",
            FlavorText = "All truth collapses into one point. You.",
            TokenCost = 350,
            Icon = "⚫"
        },
        new IlluminatiUpgrade
        {
            Id = "temporal_loop",
            Name = "Temporal Loop",
            Description = "All timers run 10x faster",
            FlavorText = "Time is a circle. A very fast circle.",
            TokenCost = 400,
            Icon = "🔄"
        },
        new IlluminatiUpgrade
        {
            Id = "omnipresent_network",
            Name = "Omnipresent Network",
            Description = "x50 ALL production permanently",
            FlavorText = "You are everywhere. You know everything.",
            TokenCost = 450,
            Icon = "🌐"
        },
        new IlluminatiUpgrade
        {
            Id = "cosmic_tinfoil",
            Name = "Cosmic Tinfoil Forge",
            Description = "+10000 Tinfoil per minute, x10 all tinfoil gains",
            FlavorText = "The universe itself is your foil supply.",
            TokenCost = 500,
            Icon = "🌟"
        },
        new IlluminatiUpgrade
        {
            Id = "final_truth",
            Name = "The Final Truth",
            Description = "x200 ALL production and click power",
            FlavorText = "There is only one truth left: YOU.",
            TokenCost = 600,
            Icon = "👁️‍🗨️"
        }
    };

    public static IlluminatiUpgrade? GetById(string id) => IlluminatiUpgrades.FirstOrDefault(u => u.Id == id);

    public static int CalculateTokensEarned(double totalEvidence)
    {
        if (totalEvidence < PRESTIGE_THRESHOLD) return 0;

        // LOGARITHMIC FORMULA: tokens = floor(log_base(evidence/threshold) * multiplier) + 1
        // This creates diminishing returns - pushing 3x further = +1 token
        // Token chart:
        //   10M (threshold): 1 token     | 30M: 2 tokens
        //   90M: 3 tokens                | 270M: 4 tokens
        //   810M: 5 tokens               | 2.4B: 6 tokens
        //   7.3B: 7 tokens               | 22B: 8 tokens
        //   66B: 9 tokens                | 200B: 10 tokens
        // Going from 10M to 200B (20,000x) only gives 10 tokens - must ascend frequently!

        double ratio = totalEvidence / PRESTIGE_THRESHOLD;
        double logValue = Math.Log(ratio) / Math.Log(TOKEN_LOG_BASE);
        return (int)Math.Floor(logValue * TOKEN_MULTIPLIER) + 1;
    }
}
