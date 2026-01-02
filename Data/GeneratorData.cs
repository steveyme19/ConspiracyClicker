using ConspiracyClicker.Models;

namespace ConspiracyClicker.Data;

public static class GeneratorData
{
    public static readonly List<Generator> AllGenerators = new()
    {
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
            BaseCost = 1000000,
            BaseProduction = 2000,
            BelieverBonus = 200
        },
        new Generator
        {
            Id = "am_radio",
            Name = "AM Radio Show",
            FlavorText = "Broadcasting truth between mattress ads.",
            BaseCost = 15000000,
            BaseProduction = 12000,
            BelieverBonus = 400
        },
        new Generator
        {
            Id = "podcast",
            Name = "4-Hour Podcast",
            FlavorText = "Joe called. He wants his format back.",
            BaseCost = 250000000,
            BaseProduction = 75000,
            BelieverBonus = 1500
        },
        new Generator
        {
            Id = "truth_conference",
            Name = "Truth Conference",
            FlavorText = "Holiday Inn Express. Complimentary tinfoil.",
            BaseCost = 5000000000,
            BaseProduction = 500000,
            BelieverBonus = 5000
        },
        new Generator
        {
            Id = "netflix_doc",
            Name = "Netflix Documentary",
            FlavorText = "Suspiciously professional. Almost TOO credible...",
            BaseCost = 100000000000,
            BaseProduction = 3500000,
            BelieverBonus = 15000
        },
        new Generator
        {
            Id = "spy_satellite",
            Name = "Spy Satellite",
            FlavorText = "Definitely not stolen from eBay. No questions.",
            BaseCost = 2000000000000,
            BaseProduction = 25000000,
            BelieverBonus = 40000
        },
        new Generator
        {
            Id = "shadow_government",
            Name = "Shadow Government",
            FlavorText = "You've become the deep state. Congrats?",
            BaseCost = 50000000000000,
            BaseProduction = 180000000,
            BelieverBonus = 100000
        }
    };

    public static Generator? GetById(string id)
    {
        return AllGenerators.FirstOrDefault(g => g.Id == id);
    }
}
