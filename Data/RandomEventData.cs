namespace ConspiracyClicker.Data;

public enum RandomEventType
{
    GoldenEye,        // 10x clicks for 10 seconds
    WhistleBlower,    // Click for bonus
    MysteryBoost,     // Random EPS boost
    TinfoilRain,      // Free tinfoil
    DoubleEvidence,   // 2x EPS for duration
    SpeedClicking,    // Auto-clicks for duration
    BelieverSurge,    // Temporary believer boost
    ConspiracyLeak    // Discount on next conspiracy
}

public class RandomEvent
{
    public required RandomEventType Type { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required string FlavorText { get; init; }
    public double Duration { get; init; } = 10; // seconds
    public double Value { get; init; } = 1.0; // multiplier or amount
    public string Icon { get; init; } = "!";
}

public static class RandomEventData
{
    public static readonly List<RandomEvent> AllEvents = new()
    {
        new RandomEvent
        {
            Type = RandomEventType.GoldenEye,
            Name = "Golden Eye",
            Description = "10x click power for 10 seconds!",
            FlavorText = "The all-seeing eye turns to gold!",
            Duration = 10,
            Value = 10,
            Icon = "👁"
        },
        new RandomEvent
        {
            Type = RandomEventType.WhistleBlower,
            Name = "Whistle-Blower",
            Description = "An insider appears! Click them quickly!",
            FlavorText = "They know too much to stay silent.",
            Duration = 5,
            Value = 120, // seconds of EPS + 5 tinfoil
            Icon = "!"
        },
        new RandomEvent
        {
            Type = RandomEventType.MysteryBoost,
            Name = "Mystery Documents",
            Description = "A mysterious package arrives with evidence!",
            FlavorText = "No return address. Just 'A Friend'.",
            Duration = 0,
            Value = 300, // 5 minutes of EPS
            Icon = "📦"
        },
        new RandomEvent
        {
            Type = RandomEventType.TinfoilRain,
            Name = "Tinfoil Rain",
            Description = "It's raining tinfoil!",
            FlavorText = "The chemtrails finally gave us something useful.",
            Duration = 0,
            Value = 3, // 1-5 tinfoil
            Icon = "🔻"
        },
        new RandomEvent
        {
            Type = RandomEventType.DoubleEvidence,
            Name = "Deep State Leak",
            Description = "Double evidence generation!",
            FlavorText = "Someone left the classified server unlocked.",
            Duration = 20,
            Value = 2,
            Icon = "📁"
        },
        new RandomEvent
        {
            Type = RandomEventType.SpeedClicking,
            Name = "Automated Analysis",
            Description = "Your research assistant takes over!",
            FlavorText = "AI doing the conspiracy work. How meta.",
            Duration = 15,
            Value = 5, // clicks per second
            Icon = "🤖"
        },
        new RandomEvent
        {
            Type = RandomEventType.BelieverSurge,
            Name = "Viral Moment",
            Description = "Your content is going viral!",
            FlavorText = "#ConspiracyConfirmed is trending.",
            Duration = 30,
            Value = 2, // 2x believers
            Icon = "📱"
        },
        new RandomEvent
        {
            Type = RandomEventType.ConspiracyLeak,
            Name = "Insider Information",
            Description = "Next conspiracy costs 50% less!",
            FlavorText = "A shadowy figure slides you an envelope.",
            Duration = 60,
            Value = 0.5,
            Icon = "🕵"
        }
    };

    private static readonly Random _random = new();

    public static RandomEvent GetRandomEvent()
    {
        // Weighted selection - some events are rarer
        var weights = new Dictionary<RandomEventType, int>
        {
            { RandomEventType.GoldenEye, 15 },
            { RandomEventType.WhistleBlower, 15 },
            { RandomEventType.MysteryBoost, 20 },
            { RandomEventType.TinfoilRain, 20 },
            { RandomEventType.DoubleEvidence, 10 },
            { RandomEventType.SpeedClicking, 8 },
            { RandomEventType.BelieverSurge, 7 },
            { RandomEventType.ConspiracyLeak, 5 }
        };

        int totalWeight = weights.Values.Sum();
        int roll = _random.Next(totalWeight);
        int cumulative = 0;

        foreach (var (type, weight) in weights)
        {
            cumulative += weight;
            if (roll < cumulative)
                return AllEvents.First(e => e.Type == type);
        }

        return AllEvents[0];
    }
}
