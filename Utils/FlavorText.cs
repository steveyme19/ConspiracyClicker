namespace ConspiracyClicker.Utils;

public static class FlavorText
{
    private static readonly Random _random = new();

    public static readonly string[] ClickMessages =
    {
        "Coincidence? I think NOT!",
        "They don't want you to know this!",
        "Wake up, sheeple!",
        "Open your third eye!",
        "The truth is out there...",
        "That's what THEY want you to think!",
        "Do your own research!",
        "I saw it on YouTube, so it's true.",
        "Connect. The. Dots.",
        "Follow the money!",
        "It's all connected, man.",
        "The mainstream media won't tell you this!",
        "Think about it!",
        "Suspicious, isn't it?",
        "Another piece of the puzzle...",
    };

    public static readonly string[] IdleMessages =
    {
        "Your computer is watching you watch it.",
        "Did you know: 73% of statistics are made up.",
        "Remember: Just because you're paranoid doesn't mean they're not after you.",
        "Tip: The government hates this one weird trick!",
        "Fun fact: Birds recharge on power lines. Think about it.",
        "Alert: Your tinfoil hat needs adjustment.",
        "Breaking: Local man connects two unrelated events.",
        "Reminder: Trust no one. Especially Gary.",
        "Notice: The simulation is running normally.",
    };

    public static string GetRandomClickMessage()
    {
        return ClickMessages[_random.Next(ClickMessages.Length)];
    }

    public static string GetRandomIdleMessage()
    {
        return IdleMessages[_random.Next(IdleMessages.Length)];
    }
}
