using ConspiracyClicker.Models;

namespace ConspiracyClicker.Data;

public static class ConspiracyData
{
    public static readonly List<Conspiracy> AllConspiracies = new()
    {
        new Conspiracy
        {
            Id = "birds_arent_real",
            Name = "Birds Aren't Real",
            Description = "Prove that birds are government surveillance drones",
            FlavorText = "That explains why they sit on power lines. Recharging.",
            EvidenceCost = 500,
            ClickBonus = 1,
            TinfoilReward = 1
        },
        new Conspiracy
        {
            Id = "flat_earth",
            Name = "Flat Earth",
            Description = "Prove the Earth is actually flat",
            FlavorText = "Like a pancake. A delicious, controversial pancake.",
            EvidenceCost = 5000,
            ClickBonus = 2,
            TinfoilReward = 2
        },
        new Conspiracy
        {
            Id = "moon_landing",
            Name = "Moon Landing Faked",
            Description = "Prove Kubrick directed the moon landing",
            FlavorText = "He insisted on 127 takes of the flag planting.",
            EvidenceCost = 50000,
            ClickBonus = 5,
            TinfoilReward = 3
        },
        new Conspiracy
        {
            Id = "lizard_people",
            Name = "Lizard People",
            Description = "Expose the reptilian elite",
            FlavorText = "Explains Congress's cold-blooded voting patterns.",
            EvidenceCost = 500000,
            ClickBonus = 10,
            TinfoilReward = 5
        },
        new Conspiracy
        {
            Id = "australia_fake",
            Name = "Australia Doesn't Exist",
            Description = "Prove Australia is a hologram",
            FlavorText = "That's why everything there 'kills you'. It's a warning.",
            EvidenceCost = 5000000,
            ClickBonus = 20,
            TinfoilReward = 8
        },
        new Conspiracy
        {
            Id = "finland_myth",
            Name = "Finland is a Myth",
            Description = "Prove Finland was invented by Japan and Russia",
            FlavorText = "For fish-related tax purposes. Obviously.",
            EvidenceCost = 50000000,
            ClickBonus = 40,
            TinfoilReward = 12
        },
        new Conspiracy
        {
            Id = "mattress_laundering",
            Name = "Mattress Store Money Laundering",
            Description = "Expose the mattress store conspiracy",
            FlavorText = "Nobody buys that many mattresses. NOBODY.",
            EvidenceCost = 500000000,
            ClickBonus = 80,
            TinfoilReward = 20
        },
        new Conspiracy
        {
            Id = "time_invention",
            Name = "Time is a Government Invention",
            Description = "Prove time was invented for control",
            FlavorText = "That's why Mondays feel so long. Wake up.",
            EvidenceCost = 5000000000,
            ClickBonus = 150,
            TinfoilReward = 35
        },
        new Conspiracy
        {
            Id = "simulation",
            Name = "Reality is a Simulation",
            Description = "Prove we live in a simulation",
            FlavorText = "This game is proof. You're clicking in a simulation about clicking.",
            EvidenceCost = 50000000000,
            ClickBonus = 300,
            TinfoilReward = 50
        },
        new Conspiracy
        {
            Id = "you_are_conspiracy",
            Name = "You ARE the Conspiracy",
            Description = "The ultimate truth",
            FlavorText = "The call was coming from inside the house. You are the deep state.",
            EvidenceCost = 500000000000,
            ClickBonus = 0,
            MultiplierBonus = 2.0,
            TinfoilReward = 100
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
