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
        }
    };

    public static Achievement? GetById(string id)
    {
        return AllAchievements.FirstOrDefault(a => a.Id == id);
    }
}
