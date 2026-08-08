namespace ConspiracyClicker.Data;

/// <summary>
/// A doctrine is a rule change for one run, drafted at the moment of ascension.
///
/// Every other progression system in this game pushes the same lever - multiply a number - so
/// the eight-or-so runs it takes to finish were mechanically identical to each other, just
/// faster. A doctrine instead bends the shape of a run: it makes something much stronger and
/// something else much weaker, so the optimal play for *this* run differs from the last one.
///
/// Every field is a plain multiplier or additive bonus the engine already had a hook for, which
/// keeps the whole system data-driven - a new doctrine is one entry in the list below.
/// </summary>
public class Doctrine
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required string Upside { get; init; }
    public required string Downside { get; init; }
    public string Icon { get; init; } = "◆";

    public double ClickPowerMultiplier { get; init; } = 1.0;
    public double EpsMultiplier { get; init; } = 1.0;
    public double BelieverMultiplier { get; init; } = 1.0;
    public double GeneratorCostMultiplier { get; init; } = 1.0;
    public double AutoClickMultiplier { get; init; } = 1.0;
    public double QuestSpeedMultiplier { get; init; } = 1.0;
    public double QuestRewardMultiplier { get; init; } = 1.0;
    public double CritChanceBonus { get; init; } = 0.0;
    public double CritDamageMultiplier { get; init; } = 1.0;
    public double ComboDecayMultiplier { get; init; } = 1.0;
    public double ComboFillMultiplier { get; init; } = 1.0;
    public double ConspiracyCostMultiplier { get; init; } = 1.0;
    public double TinfoilMultiplier { get; init; } = 1.0;
    public double FrenzyPowerBonus { get; init; } = 0.0;   // added to every frenzy step and the cap
}

public static class DoctrineData
{
    /// <summary>How many doctrines the player chooses between at each ascension.</summary>
    public const int DraftSize = 3;

    public static readonly List<Doctrine> AllDoctrines = new()
    {
        new Doctrine
        {
            Id = "hands_on",
            Name = "Hands-On Research",
            Description = "You trust nothing you did not dig up yourself.",
            Upside = "Click power x25",
            Downside = "Generators produce half as much",
            ClickPowerMultiplier = 25.0,
            EpsMultiplier = 0.5,
            Icon = "👆"
        },
        new Doctrine
        {
            Id = "full_automation",
            Name = "Total Automation",
            Description = "The machines file the paperwork now.",
            Upside = "Auto-clickers run 4x faster, generators produce 50% more",
            Downside = "Your own clicks are worth a fifth",
            AutoClickMultiplier = 4.0,
            EpsMultiplier = 1.5,
            ClickPowerMultiplier = 0.2,
            Icon = "⚙"
        },
        new Doctrine
        {
            Id = "zealotry",
            Name = "Zealotry",
            Description = "Converts first. Proof can wait.",
            Upside = "Believers x8, quests resolve 3x faster",
            Downside = "Evidence production x0.6",
            BelieverMultiplier = 8.0,
            QuestSpeedMultiplier = 0.33,
            EpsMultiplier = 0.6,
            Icon = "👥"
        },
        new Doctrine
        {
            Id = "frugality",
            Name = "Shoestring Budget",
            Description = "Everything second-hand, everything cheap.",
            Upside = "Generators cost 85% less",
            Downside = "Evidence production x0.65",
            GeneratorCostMultiplier = 0.15,
            EpsMultiplier = 0.65,
            Icon = "◇"
        },
        new Doctrine
        {
            Id = "brinkmanship",
            Name = "Brinkmanship",
            Description = "Big swings. You will feel every miss.",
            Upside = "+30% crit chance, crits hit 3x harder",
            Downside = "The combo meter drains four times as fast",
            CritChanceBonus = 0.30,
            CritDamageMultiplier = 3.0,
            ComboDecayMultiplier = 4.0,
            Icon = "🎯"
        },
        new Doctrine
        {
            Id = "deep_cover",
            Name = "Deep Cover",
            Description = "Everyone is in the field. Nobody is at the desk.",
            Upside = "Quests resolve 5x faster and pay 3x",
            Downside = "Evidence production x0.7, believers x0.5",
            QuestSpeedMultiplier = 0.2,
            QuestRewardMultiplier = 3.0,
            EpsMultiplier = 0.7,
            BelieverMultiplier = 0.5,
            Icon = "🕵"
        },
        new Doctrine
        {
            Id = "flow_state",
            Name = "Flow State",
            Description = "The chain is the point.",
            Upside = "Combo fills 3x faster, Frenzy runs 3x stronger",
            Downside = "Generators produce 40% less",
            ComboFillMultiplier = 3.0,
            FrenzyPowerBonus = 3.0,
            EpsMultiplier = 0.6,
            Icon = "⚡"
        },
        new Doctrine
        {
            Id = "open_secret",
            Name = "Open Secret",
            Description = "Tell everyone. Nobody believes it anyway.",
            Upside = "Tinfoil income x6, conspiracies cost 60% less to prove",
            Downside = "Click power x0.4, believers x0.6",
            TinfoilMultiplier = 6.0,
            ConspiracyCostMultiplier = 0.4,
            ClickPowerMultiplier = 0.4,
            BelieverMultiplier = 0.6,
            Icon = "📰"
        },
        new Doctrine
        {
            Id = "burn_the_files",
            Name = "Burn The Files",
            Description = "Prove it fast, before anyone can bury it.",
            Upside = "Evidence production x3",
            Downside = "Conspiracies cost 4x more to prove",
            EpsMultiplier = 3.0,
            ConspiracyCostMultiplier = 4.0,
            Icon = "🔥"
        },
        new Doctrine
        {
            Id = "true_believer",
            Name = "True Believer",
            Description = "No hedging. No shortcuts. No automation.",
            Upside = "Everything you earn by hand is worth 8x",
            Downside = "Auto-clickers do not work at all",
            ClickPowerMultiplier = 8.0,
            FrenzyPowerBonus = 1.0,
            AutoClickMultiplier = 0.0,
            Icon = "∞"
        }
    };

    private static readonly Dictionary<string, Doctrine> ById = DataIndex.Build(AllDoctrines, d => d.Id);

    public static Doctrine? GetById(string id) => ById.TryGetValue(id, out var doctrine) ? doctrine : null;

    /// <summary>
    /// Picks the doctrines on offer for one ascension. Seeded by ascension number so the draft
    /// survives a save/reload - a player cannot reroll it by restarting the game.
    /// Anything already taken this playthrough is excluded until the pool runs dry.
    /// </summary>
    public static List<Doctrine> GetDraft(int ascensionNumber, IEnumerable<string> alreadyTaken)
    {
        var taken = new HashSet<string>(alreadyTaken, StringComparer.Ordinal);

        var pool = AllDoctrines.Where(d => !taken.Contains(d.Id)).ToList();
        if (pool.Count < DraftSize) pool = new List<Doctrine>(AllDoctrines);

        var random = new Random(ascensionNumber * 7919 + 13);
        var draft = new List<Doctrine>();
        while (draft.Count < DraftSize && pool.Count > 0)
        {
            int index = random.Next(pool.Count);
            draft.Add(pool[index]);
            pool.RemoveAt(index);
        }
        return draft;
    }
}
