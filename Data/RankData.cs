namespace ConspiracyClicker.Data;

public class SecretSocietyRank
{
    public required string Id { get; init; }
    public required string Society { get; init; }
    public required string Title { get; init; }
    public required int ConspiraciesRequired { get; init; }
    public required string Icon { get; init; }
    public required string Color { get; init; } // Hex color for the rank
    public string? FlavorText { get; init; }
}

public static class RankData
{
    public static readonly List<SecretSocietyRank> AllRanks = new()
    {
        // === TIER 0: THE UNINITIATED ===
        new SecretSocietyRank
        {
            Id = "uninitiated",
            Society = "None",
            Title = "Uninitiated",
            ConspiraciesRequired = 0,
            Icon = "?",
            Color = "#808080",
            FlavorText = "You know nothing... yet."
        },

        // === TIER 1: TRUTH SEEKERS (1-3 conspiracies) ===
        new SecretSocietyRank
        {
            Id = "truth_curious",
            Society = "Truth Seekers",
            Title = "Curious Mind",
            ConspiraciesRequired = 1,
            Icon = "?",
            Color = "#4a9eff",
            FlavorText = "You've begun to question the narrative."
        },
        new SecretSocietyRank
        {
            Id = "truth_skeptic",
            Society = "Truth Seekers",
            Title = "Skeptic",
            ConspiraciesRequired = 2,
            Icon = "?",
            Color = "#4a9eff",
            FlavorText = "Trust nothing. Verify everything."
        },
        new SecretSocietyRank
        {
            Id = "truth_researcher",
            Society = "Truth Seekers",
            Title = "Researcher",
            ConspiraciesRequired = 3,
            Icon = "?",
            Color = "#4a9eff",
            FlavorText = "Down the rabbit hole you go."
        },

        // === TIER 2: THE FREEMASONS (4-6 conspiracies) ===
        new SecretSocietyRank
        {
            Id = "mason_entered",
            Society = "Freemasons",
            Title = "Entered Apprentice",
            ConspiraciesRequired = 4,
            Icon = "G",
            Color = "#3366cc",
            FlavorText = "The square and compass guide your path."
        },
        new SecretSocietyRank
        {
            Id = "mason_fellow",
            Society = "Freemasons",
            Title = "Fellow Craft",
            ConspiraciesRequired = 5,
            Icon = "G",
            Color = "#3366cc",
            FlavorText = "You've learned the secret handshake."
        },
        new SecretSocietyRank
        {
            Id = "mason_master",
            Society = "Freemasons",
            Title = "Master Mason",
            ConspiraciesRequired = 6,
            Icon = "G",
            Color = "#3366cc",
            FlavorText = "The third degree is complete."
        },

        // === TIER 3: ROSICRUCIANS (7-9 conspiracies) ===
        new SecretSocietyRank
        {
            Id = "rosy_neophyte",
            Society = "Rosicrucians",
            Title = "Neophyte",
            ConspiraciesRequired = 7,
            Icon = "R",
            Color = "#cc3366",
            FlavorText = "The rose blooms upon the cross."
        },
        new SecretSocietyRank
        {
            Id = "rosy_zelator",
            Society = "Rosicrucians",
            Title = "Zelator",
            ConspiraciesRequired = 8,
            Icon = "R",
            Color = "#cc3366",
            FlavorText = "Alchemical knowledge flows through you."
        },
        new SecretSocietyRank
        {
            Id = "rosy_adept",
            Society = "Rosicrucians",
            Title = "Adeptus Minor",
            ConspiraciesRequired = 9,
            Icon = "R",
            Color = "#cc3366",
            FlavorText = "The invisible college welcomes you."
        },

        // === TIER 4: THE ILLUMINATI (10-12 conspiracies) ===
        new SecretSocietyRank
        {
            Id = "illuminati_novice",
            Society = "Illuminati",
            Title = "Novice",
            ConspiraciesRequired = 10,
            Icon = "V",
            Color = "#00ff41",
            FlavorText = "Welcome to the pyramid."
        },
        new SecretSocietyRank
        {
            Id = "illuminati_minerval",
            Society = "Illuminati",
            Title = "Minerval",
            ConspiraciesRequired = 11,
            Icon = "V",
            Color = "#00ff41",
            FlavorText = "The owl of Minerva flies at dusk."
        },
        new SecretSocietyRank
        {
            Id = "illuminati_illuminatus",
            Society = "Illuminati",
            Title = "Illuminatus Minor",
            ConspiraciesRequired = 12,
            Icon = "V",
            Color = "#00ff41",
            FlavorText = "You see beyond the veil."
        },

        // === TIER 5: KNIGHTS TEMPLAR (13-15 conspiracies) ===
        new SecretSocietyRank
        {
            Id = "templar_squire",
            Society = "Knights Templar",
            Title = "Squire",
            ConspiraciesRequired = 13,
            Icon = "+",
            Color = "#cc0000",
            FlavorText = "The crusade for truth begins."
        },
        new SecretSocietyRank
        {
            Id = "templar_knight",
            Society = "Knights Templar",
            Title = "Knight",
            ConspiraciesRequired = 14,
            Icon = "+",
            Color = "#cc0000",
            FlavorText = "Your sword is truth. Your shield is evidence."
        },
        new SecretSocietyRank
        {
            Id = "templar_commander",
            Society = "Knights Templar",
            Title = "Commander",
            ConspiraciesRequired = 15,
            Icon = "+",
            Color = "#cc0000",
            FlavorText = "The Holy Grail of secrets awaits."
        },

        // === TIER 6: SKULL AND BONES (16-18 conspiracies) ===
        new SecretSocietyRank
        {
            Id = "bones_pledge",
            Society = "Skull and Bones",
            Title = "Pledge",
            ConspiraciesRequired = 16,
            Icon = "\u2620",
            Color = "#1a1a2e",
            FlavorText = "The Tomb opens its doors."
        },
        new SecretSocietyRank
        {
            Id = "bones_bonesman",
            Society = "Skull and Bones",
            Title = "Bonesman",
            ConspiraciesRequired = 17,
            Icon = "\u2620",
            Color = "#1a1a2e",
            FlavorText = "322. The number echoes in the crypt."
        },
        new SecretSocietyRank
        {
            Id = "bones_patriarch",
            Society = "Skull and Bones",
            Title = "Patriarch",
            ConspiraciesRequired = 18,
            Icon = "\u2620",
            Color = "#1a1a2e",
            FlavorText = "Presidents bow to your lineage."
        },

        // === TIER 7: THULE SOCIETY (19-21 conspiracies) ===
        new SecretSocietyRank
        {
            Id = "thule_initiate",
            Society = "Thule Society",
            Title = "Initiate",
            ConspiraciesRequired = 19,
            Icon = "\u2609",
            Color = "#4a0080",
            FlavorText = "Hyperborea calls to you."
        },
        new SecretSocietyRank
        {
            Id = "thule_mystic",
            Society = "Thule Society",
            Title = "Mystic",
            ConspiraciesRequired = 20,
            Icon = "\u2609",
            Color = "#4a0080",
            FlavorText = "The Vril energy flows through you."
        },
        new SecretSocietyRank
        {
            Id = "thule_archon",
            Society = "Thule Society",
            Title = "Archon",
            ConspiraciesRequired = 21,
            Icon = "\u2609",
            Color = "#4a0080",
            FlavorText = "Ancient knowledge is your domain."
        },

        // === TIER 8: THE COUNCIL (22-24 conspiracies) ===
        new SecretSocietyRank
        {
            Id = "council_observer",
            Society = "The Council of 13",
            Title = "Observer",
            ConspiraciesRequired = 22,
            Icon = "\u25B3",
            Color = "#ffd700",
            FlavorText = "You witness the world's true rulers."
        },
        new SecretSocietyRank
        {
            Id = "council_member",
            Society = "The Council of 13",
            Title = "Council Member",
            ConspiraciesRequired = 23,
            Icon = "\u25B3",
            Color = "#ffd700",
            FlavorText = "Your vote shapes reality itself."
        },
        new SecretSocietyRank
        {
            Id = "council_inner",
            Society = "The Council of 13",
            Title = "Inner Circle",
            ConspiraciesRequired = 24,
            Icon = "\u25B3",
            Color = "#ffd700",
            FlavorText = "The architects of existence bow to you."
        },

        // === TIER 9: THE ETERNAL (25 conspiracies) - FINAL RANK ===
        new SecretSocietyRank
        {
            Id = "eternal_one",
            Society = "The Eternal Conspiracy",
            Title = "The One Who Knows",
            ConspiraciesRequired = 25,
            Icon = "\u221E",
            Color = "#ff00ff",
            FlavorText = "You ARE the conspiracy. Always have been. Always will be."
        }
    };

    public static SecretSocietyRank GetRankForConspiracies(int conspiracyCount)
    {
        // Return the highest rank the player qualifies for
        SecretSocietyRank currentRank = AllRanks[0];
        foreach (var rank in AllRanks)
        {
            if (conspiracyCount >= rank.ConspiraciesRequired)
                currentRank = rank;
            else
                break;
        }
        return currentRank;
    }

    public static SecretSocietyRank? GetNextRank(int conspiracyCount)
    {
        foreach (var rank in AllRanks)
        {
            if (rank.ConspiraciesRequired > conspiracyCount)
                return rank;
        }
        return null; // Already at max rank
    }
}
