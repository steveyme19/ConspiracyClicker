using ConspiracyClicker.Models;

namespace ConspiracyClicker.Data;

public static class AchievementData
{
    public static readonly List<Achievement> AllAchievements = new()
    {
        // === EVIDENCE MILESTONES ===
        new Achievement
        {
            Id = "evidence_100",
            Name = "Suspicious Beginning",
            Description = "Gather 100 evidence",
            Type = AchievementType.TotalEvidence,
            Threshold = 100,
            TinfoilReward = 1
        },
        new Achievement
        {
            Id = "evidence_10k",
            Name = "Down the Rabbit Hole",
            Description = "Gather 10,000 evidence",
            Type = AchievementType.TotalEvidence,
            Threshold = 10000,
            TinfoilReward = 2
        },
        new Achievement
        {
            Id = "evidence_1m",
            Name = "Professional Paranoia",
            Description = "Gather 1,000,000 evidence",
            Type = AchievementType.TotalEvidence,
            Threshold = 1000000,
            TinfoilReward = 5
        },
        new Achievement
        {
            Id = "evidence_1b",
            Name = "They DEFINITELY Know About You",
            Description = "Gather 1,000,000,000 evidence",
            Type = AchievementType.TotalEvidence,
            Threshold = 1000000000,
            TinfoilReward = 10
        },
        new Achievement
        {
            Id = "evidence_1t",
            Name = "Evidence Singularity",
            Description = "Gather 1 trillion evidence",
            Type = AchievementType.TotalEvidence,
            Threshold = 1000000000000,
            TinfoilReward = 25
        },

        // === CLICK ACHIEVEMENTS ===
        new Achievement
        {
            Id = "clicks_100",
            Name = "Carpal Tunnel Incoming",
            Description = "Click 100 times",
            Type = AchievementType.TotalClicks,
            Threshold = 100,
            TinfoilReward = 1
        },
        new Achievement
        {
            Id = "clicks_1000",
            Name = "The Clickening",
            Description = "Click 1,000 times",
            Type = AchievementType.TotalClicks,
            Threshold = 1000,
            TinfoilReward = 2
        },
        new Achievement
        {
            Id = "clicks_10000",
            Name = "Actually Concerning",
            Description = "Click 10,000 times",
            Type = AchievementType.TotalClicks,
            Threshold = 10000,
            TinfoilReward = 5
        },
        new Achievement
        {
            Id = "clicks_100000",
            Name = "Seek Help (But Not From Them)",
            Description = "Click 100,000 times",
            Type = AchievementType.TotalClicks,
            Threshold = 100000,
            TinfoilReward = 10
        },
        new Achievement
        {
            Id = "clicks_1m",
            Name = "Finger of Truth",
            Description = "Click 1,000,000 times",
            Type = AchievementType.TotalClicks,
            Threshold = 1000000,
            TinfoilReward = 25
        },

        // === GENERATOR ACHIEVEMENTS ===
        new Achievement
        {
            Id = "strings_100",
            Name = "String Theory",
            Description = "Own 100 Red Strings",
            Type = AchievementType.GeneratorOwned,
            Threshold = 100,
            TargetId = "red_string",
            TinfoilReward = 3
        },
        new Achievement
        {
            Id = "neighbors_50",
            Name = "Army of Garys",
            Description = "Own 50 Suspicious Neighbors",
            Type = AchievementType.GeneratorOwned,
            Threshold = 50,
            TargetId = "suspicious_neighbor",
            TinfoilReward = 3
        },
        new Achievement
        {
            Id = "researchers_25",
            Name = "Basement Dwellers United",
            Description = "Own 25 Basement Researchers",
            Type = AchievementType.GeneratorOwned,
            Threshold = 25,
            TargetId = "basement_researcher",
            TinfoilReward = 3
        },
        new Achievement
        {
            Id = "youtube_10",
            Name = "Monetization Pending",
            Description = "Own 10 YouTube Channels",
            Type = AchievementType.GeneratorOwned,
            Threshold = 10,
            TargetId = "youtube_channel",
            TinfoilReward = 5
        },
        new Achievement
        {
            Id = "podcast_5",
            Name = "Podcast Industrial Complex",
            Description = "Own 5 4-Hour Podcasts",
            Type = AchievementType.GeneratorOwned,
            Threshold = 5,
            TargetId = "podcast",
            TinfoilReward = 8
        },

        // === CONSPIRACY ACHIEVEMENTS ===
        new Achievement
        {
            Id = "conspiracy_1",
            Name = "Truther",
            Description = "Prove your first conspiracy",
            Type = AchievementType.ConspiraciesProven,
            Threshold = 1,
            TinfoilReward = 2
        },
        new Achievement
        {
            Id = "conspiracy_5",
            Name = "Red-Pilled",
            Description = "Prove 5 conspiracies",
            Type = AchievementType.ConspiraciesProven,
            Threshold = 5,
            TinfoilReward = 5
        },
        new Achievement
        {
            Id = "conspiracy_all",
            Name = "Omniscient",
            Description = "Prove all conspiracies",
            Type = AchievementType.ConspiraciesProven,
            Threshold = 10,
            TinfoilReward = 25
        },

        // === PLAYTIME ===
        new Achievement
        {
            Id = "playtime_1h",
            Name = "Just Getting Started",
            Description = "Play for 1 hour",
            Type = AchievementType.PlayTime,
            Threshold = 3600,
            TinfoilReward = 2
        },
        new Achievement
        {
            Id = "playtime_10h",
            Name = "Dedicated Researcher",
            Description = "Play for 10 hours",
            Type = AchievementType.PlayTime,
            Threshold = 36000,
            TinfoilReward = 10
        },
        new Achievement
        {
            Id = "playtime_100h",
            Name = "This IS Your Job Now",
            Description = "Play for 100 hours",
            Type = AchievementType.PlayTime,
            Threshold = 360000,
            TinfoilReward = 50
        },

        // === FOURTH WALL BREAK ===
        new Achievement
        {
            Id = "meta_reading",
            Name = "Reading the Code",
            Description = "You're reading this achievement description",
            FlavorText = "We know you're looking at this.",
            Type = AchievementType.TotalEvidence,
            Threshold = 500,
            TinfoilReward = 1
        },
        new Achievement
        {
            Id = "meta_cookie",
            Name = "Not Cookie Clicker",
            Description = "Definitely not inspired by anything",
            FlavorText = "Legal disclaimer: We are NOT clicking cookies.",
            Type = AchievementType.TotalClicks,
            Threshold = 500,
            TinfoilReward = 1
        },
        new Achievement
        {
            Id = "meta_idle",
            Name = "Actually Idle",
            Description = "You stepped away, didn't you?",
            FlavorText = "We see you 'idle gaming'.",
            Type = AchievementType.TotalEvidence,
            Threshold = 50000,
            TinfoilReward = 2
        },
        new Achievement
        {
            Id = "meta_conspiracy",
            Name = "The Real Conspiracy",
            Description = "Maybe the conspiracy was the clicks we made along the way",
            FlavorText = "Deep, right?",
            Type = AchievementType.TotalClicks,
            Threshold = 5000,
            TinfoilReward = 3
        },
        new Achievement
        {
            Id = "meta_developer",
            Name = "Hello, Developer",
            Description = "We know you're testing this",
            FlavorText = "Hope the build succeeded!",
            Type = AchievementType.TotalEvidence,
            Threshold = 1,
            TinfoilReward = 0
        },
        new Achievement
        {
            Id = "meta_addiction",
            Name = "Just One More Click",
            Description = "Said 10,000 clicks ago",
            FlavorText = "We're not judging. Much.",
            Type = AchievementType.TotalClicks,
            Threshold = 50000,
            TinfoilReward = 5
        },
        new Achievement
        {
            Id = "meta_time",
            Name = "Time is an Illusion",
            Description = "Especially when clicking",
            FlavorText = "Has it really been that long?",
            Type = AchievementType.PlayTime,
            Threshold = 7200, // 2 hours
            TinfoilReward = 3
        },
        new Achievement
        {
            Id = "meta_prestige",
            Name = "It's a Trap!",
            Description = "The Illuminati was the endgame all along",
            FlavorText = "You fell right into their pyramid scheme.",
            Type = AchievementType.TotalEvidence,
            Threshold = 100000000,
            TinfoilReward = 5
        },

        // === PRESTIGE ACHIEVEMENTS ===
        new Achievement
        {
            Id = "ascend_1",
            Name = "Illuminated",
            Description = "Perform your first Illuminati Ascension",
            FlavorText = "Welcome to the pyramid, initiate.",
            Type = AchievementType.TimesAscended,
            Threshold = 1,
            TinfoilReward = 10
        },
        new Achievement
        {
            Id = "ascend_5",
            Name = "Inner Circle",
            Description = "Ascend 5 times",
            FlavorText = "You're moving up the ranks.",
            Type = AchievementType.TimesAscended,
            Threshold = 5,
            TinfoilReward = 25
        },
        new Achievement
        {
            Id = "ascend_10",
            Name = "Grand Master",
            Description = "Ascend 10 times",
            FlavorText = "The all-seeing eye blinks in approval.",
            Type = AchievementType.TimesAscended,
            Threshold = 10,
            TinfoilReward = 50
        },
        new Achievement
        {
            Id = "ascend_25",
            Name = "Illuminati Prime",
            Description = "Ascend 25 times",
            FlavorText = "You've transcended the pyramid itself.",
            Type = AchievementType.TimesAscended,
            Threshold = 25,
            TinfoilReward = 100
        },

        // === MATRIX ACHIEVEMENTS ===
        new Achievement
        {
            Id = "matrix_1",
            Name = "Red Pill",
            Description = "Break the Matrix for the first time",
            FlavorText = "Welcome to the real real world.",
            Type = AchievementType.TimesMatrixBroken,
            Threshold = 1,
            TinfoilReward = 50
        },
        new Achievement
        {
            Id = "matrix_3",
            Name = "The Architect's Concern",
            Description = "Break the Matrix 3 times",
            FlavorText = "You're causing stability issues.",
            Type = AchievementType.TimesMatrixBroken,
            Threshold = 3,
            TinfoilReward = 100
        },
        new Achievement
        {
            Id = "matrix_5",
            Name = "Anomaly",
            Description = "Break the Matrix 5 times",
            FlavorText = "The equations no longer balance.",
            Type = AchievementType.TimesMatrixBroken,
            Threshold = 5,
            TinfoilReward = 200
        },

        // === QUEST ACHIEVEMENTS ===
        new Achievement
        {
            Id = "quest_1",
            Name = "Field Agent",
            Description = "Complete your first quest",
            FlavorText = "Your first mission. Many more to come.",
            Type = AchievementType.QuestsCompleted,
            Threshold = 1,
            TinfoilReward = 2
        },
        new Achievement
        {
            Id = "quest_10",
            Name = "Seasoned Operative",
            Description = "Complete 10 quests",
            FlavorText = "You've got experience now.",
            Type = AchievementType.QuestsCompleted,
            Threshold = 10,
            TinfoilReward = 5
        },
        new Achievement
        {
            Id = "quest_50",
            Name = "Mission Control",
            Description = "Complete 50 quests",
            FlavorText = "You run a tight operation.",
            Type = AchievementType.QuestsCompleted,
            Threshold = 50,
            TinfoilReward = 15
        },
        new Achievement
        {
            Id = "quest_100",
            Name = "Shadow Commander",
            Description = "Complete 100 quests",
            FlavorText = "Your believers follow you into any darkness.",
            Type = AchievementType.QuestsCompleted,
            Threshold = 100,
            TinfoilReward = 30
        },
        new Achievement
        {
            Id = "quest_500",
            Name = "Conspiracy Central",
            Description = "Complete 500 quests",
            FlavorText = "You've built an empire of truth-seekers.",
            Type = AchievementType.QuestsCompleted,
            Threshold = 500,
            TinfoilReward = 100
        },

        // === CRITICAL HIT ACHIEVEMENTS ===
        new Achievement
        {
            Id = "crit_10",
            Name = "Lucky Strike",
            Description = "Land 10 critical clicks",
            FlavorText = "The universe agrees with you.",
            Type = AchievementType.CriticalClicks,
            Threshold = 10,
            TinfoilReward = 2
        },
        new Achievement
        {
            Id = "crit_100",
            Name = "Precision Truther",
            Description = "Land 100 critical clicks",
            FlavorText = "Every click counts double... sometimes more.",
            Type = AchievementType.CriticalClicks,
            Threshold = 100,
            TinfoilReward = 5
        },
        new Achievement
        {
            Id = "crit_1000",
            Name = "Critical Mass",
            Description = "Land 1,000 critical clicks",
            FlavorText = "Your finger has achieved enlightenment.",
            Type = AchievementType.CriticalClicks,
            Threshold = 1000,
            TinfoilReward = 15
        },
        new Achievement
        {
            Id = "crit_10000",
            Name = "One With The Click",
            Description = "Land 10,000 critical clicks",
            FlavorText = "You don't find critical hits. They find you.",
            Type = AchievementType.CriticalClicks,
            Threshold = 10000,
            TinfoilReward = 50
        },

        // === ENDGAME EVIDENCE ===
        new Achievement
        {
            Id = "evidence_1q",
            Name = "Quadrillionaire Truther",
            Description = "Gather 1 quadrillion evidence",
            Type = AchievementType.TotalEvidence,
            Threshold = 1000000000000000,
            TinfoilReward = 50,
            MultiplierReward = 1.1
        },
        new Achievement
        {
            Id = "evidence_1qi",
            Name = "Evidence Transcendence",
            Description = "Gather 1 quintillion evidence",
            Type = AchievementType.TotalEvidence,
            Threshold = 1000000000000000000,
            TinfoilReward = 100,
            MultiplierReward = 1.2
        },

        // === LATE GAME GENERATORS ===
        new Achievement
        {
            Id = "spy_satellite_1",
            Name = "Eye in the Sky",
            Description = "Own your first Spy Satellite",
            Type = AchievementType.GeneratorOwned,
            Threshold = 1,
            TargetId = "spy_satellite",
            TinfoilReward = 10
        },
        new Achievement
        {
            Id = "shadow_gov_1",
            Name = "Power Behind the Throne",
            Description = "Own your first Shadow Government",
            Type = AchievementType.GeneratorOwned,
            Threshold = 1,
            TargetId = "shadow_government",
            TinfoilReward = 15
        },
        new Achievement
        {
            Id = "time_machine_1",
            Name = "Temporal Agent",
            Description = "Own your first Time Machine",
            Type = AchievementType.GeneratorOwned,
            Threshold = 1,
            TargetId = "time_machine",
            TinfoilReward = 25
        },
        new Achievement
        {
            Id = "reality_editor_1",
            Name = "Reality Hacker",
            Description = "Own your first Reality Editor",
            Type = AchievementType.GeneratorOwned,
            Threshold = 1,
            TargetId = "reality_editor",
            TinfoilReward = 50
        },
        new Achievement
        {
            Id = "universe_creator_1",
            Name = "God Complex (Achieved)",
            Description = "Own your first Universe Creator",
            FlavorText = "Congratulations. You've won. Or have you?",
            Type = AchievementType.GeneratorOwned,
            Threshold = 1,
            TargetId = "universe_creator",
            TinfoilReward = 100,
            MultiplierReward = 1.5
        }
    };

    public static Achievement? GetById(string id)
    {
        return AllAchievements.FirstOrDefault(a => a.Id == id);
    }
}
