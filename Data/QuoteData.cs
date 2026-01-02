namespace ConspiracyClicker.Data;

public static class QuoteData
{
    private static readonly Random _random = new();
    private static int _lastIndex = -1;

    public static readonly List<string> ConspiracyQuotes = new()
    {
        "\"The truth is out there. Mostly in this game.\" - Ancient Proverb",
        "\"They don't want you to know. But you're clicking anyway.\" - Definitely Real Quote",
        "\"Wake up, sheeple. Or at least click.\" - Internet Wisdom",
        "\"Question everything. Except this game.\" - Sound Advice",
        "\"The deeper you dig, the more evidence you find.\" - Basic Logic",
        "\"They laughed at flat earthers. They'll stop laughing... eventually.\" - Hopeful Theorist",
        "\"Every click brings you closer to the truth.\" - Motivational Poster",
        "\"Coincidence? I THINK NOT.\" - Everyone, Eventually",
        "\"Follow the money. And the evidence.\" - Financial Advice",
        "\"Birds aren't real. Neither are your doubts.\" - Bird Truther",
        "\"The government doesn't want you to be this good at clicking.\" - Probably True",
        "\"Knowledge is power. Evidence is more power.\" - Power Rankings",
        "\"Stay vigilant. Stay clicking.\" - Daily Reminder",
        "\"They're watching. But so are we.\" - Mutual Surveillance",
        "\"Every conspiracy starts with a question. Yours started with a click.\" - Origin Story",
        "\"Trust no one. Except your auto-clicker.\" - Practical Advice",
        "\"The Illuminati fears the dedicated clicker.\" - Encouraging Words",
        "\"One click for truth, a giant leap for conspiracy-kind.\" - Historic Moment",
        "\"In clicks we trust.\" - National Motto",
        "\"Clicking is believing.\" - New Proverb",
        "\"Behind every great conspiracy is a dedicated researcher.\" - Self Affirmation",
        "\"They can take our freedom, but they can't take our click rate.\" - Brave Words",
        "\"The revolution will not be televised. It will be clicked.\" - Modern Revolution",
        "\"Some say it's just a game. Those people are suspicious.\" - Valid Point",
        "\"I click, therefore I am... onto something.\" - Philosophy",
    };

    public static readonly List<string> IdleQuotes = new()
    {
        "Your evidence is accumulating... suspiciously fast.",
        "The believers are spreading the word.",
        "Somewhere, a government official is nervous.",
        "Your research continues in the shadows.",
        "The truth never sleeps. Neither should you. But also get sleep.",
        "Evidence flows like water. Or like conspiracy theories.",
        "Your network grows stronger.",
        "The mainstream media hates this one weird trick.",
        "Progress is being made. The establishment trembles.",
        "Another day, another cover-up to uncover."
    };

    public static string GetRandomConspiracyQuote()
    {
        int index;
        do
        {
            index = _random.Next(ConspiracyQuotes.Count);
        } while (index == _lastIndex && ConspiracyQuotes.Count > 1);

        _lastIndex = index;
        return ConspiracyQuotes[index];
    }

    public static string GetRandomIdleQuote()
    {
        return IdleQuotes[_random.Next(IdleQuotes.Count)];
    }
}
