namespace ConspiracyClicker.Data;

public enum SkillBranch
{
    Researcher,   // Evidence bonuses
    Influencer,   // Believer bonuses
    Infiltrator   // Click power bonuses
}

public class Skill
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required SkillBranch Branch { get; init; }
    public required int Tier { get; init; } // 1-5
    public required int SkillPointCost { get; init; }
    public string? RequiredSkillId { get; init; }
    public double Value { get; init; } = 1.0;
    public string Icon { get; init; } = "◆";
}

public static class SkillTreeData
{
    // Skill points earned: 1 per 10 achievements + 1 per prestige

    public static readonly List<Skill> AllSkills = new()
    {
        // === RESEARCHER BRANCH (Evidence/EPS) ===
        new Skill
        {
            Id = "research_basics",
            Name = "Research Basics",
            Description = "+10% evidence per second",
            Branch = SkillBranch.Researcher,
            Tier = 1,
            SkillPointCost = 1,
            Value = 1.10,
            Icon = "📚"
        },
        new Skill
        {
            Id = "speed_reading",
            Name = "Speed Reading",
            Description = "+15% evidence per second",
            Branch = SkillBranch.Researcher,
            Tier = 2,
            SkillPointCost = 2,
            RequiredSkillId = "research_basics",
            Value = 1.15,
            Icon = "📖"
        },
        new Skill
        {
            Id = "data_mining",
            Name = "Data Mining",
            Description = "+25% evidence per second",
            Branch = SkillBranch.Researcher,
            Tier = 3,
            SkillPointCost = 3,
            RequiredSkillId = "speed_reading",
            Value = 1.25,
            Icon = "💾"
        },
        new Skill
        {
            Id = "quantum_analysis",
            Name = "Quantum Analysis",
            Description = "+50% evidence per second",
            Branch = SkillBranch.Researcher,
            Tier = 4,
            SkillPointCost = 5,
            RequiredSkillId = "data_mining",
            Value = 1.50,
            Icon = "⚛"
        },
        new Skill
        {
            Id = "omniscience",
            Name = "Omniscience",
            Description = "x2 evidence per second",
            Branch = SkillBranch.Researcher,
            Tier = 5,
            SkillPointCost = 8,
            RequiredSkillId = "quantum_analysis",
            Value = 2.0,
            Icon = "🔮"
        },

        // === INFLUENCER BRANCH (Believers/Quests) ===
        new Skill
        {
            Id = "charisma",
            Name = "Natural Charisma",
            Description = "+10% believers from all sources",
            Branch = SkillBranch.Influencer,
            Tier = 1,
            SkillPointCost = 1,
            Value = 1.10,
            Icon = "😊"
        },
        new Skill
        {
            Id = "persuasion",
            Name = "Persuasion",
            Description = "+15% believers from all sources",
            Branch = SkillBranch.Influencer,
            Tier = 2,
            SkillPointCost = 2,
            RequiredSkillId = "charisma",
            Value = 1.15,
            Icon = "🗣"
        },
        new Skill
        {
            Id = "viral_marketing",
            Name = "Viral Marketing",
            Description = "+10% quest success chance",
            Branch = SkillBranch.Influencer,
            Tier = 3,
            SkillPointCost = 3,
            RequiredSkillId = "persuasion",
            Value = 0.10,
            Icon = "📱"
        },
        new Skill
        {
            Id = "cult_of_personality",
            Name = "Cult of Personality",
            Description = "+50% believers, +25% quest rewards",
            Branch = SkillBranch.Influencer,
            Tier = 4,
            SkillPointCost = 5,
            RequiredSkillId = "viral_marketing",
            Value = 1.50,
            Icon = "👑"
        },
        new Skill
        {
            Id = "mind_control",
            Name = "Mind Control",
            Description = "x2 believers, quests never fail",
            Branch = SkillBranch.Influencer,
            Tier = 5,
            SkillPointCost = 8,
            RequiredSkillId = "cult_of_personality",
            Value = 2.0,
            Icon = "🧠"
        },

        // === INFILTRATOR BRANCH (Clicks/Critical) ===
        new Skill
        {
            Id = "quick_fingers",
            Name = "Quick Fingers",
            Description = "+15% click power",
            Branch = SkillBranch.Infiltrator,
            Tier = 1,
            SkillPointCost = 1,
            Value = 1.15,
            Icon = "👆"
        },
        new Skill
        {
            Id = "precision_clicking",
            Name = "Precision Clicking",
            Description = "+5% critical hit chance",
            Branch = SkillBranch.Infiltrator,
            Tier = 2,
            SkillPointCost = 2,
            RequiredSkillId = "quick_fingers",
            Value = 0.05,
            Icon = "🎯"
        },
        new Skill
        {
            Id = "combo_master",
            Name = "Combo Master",
            Description = "Combo meter fills 25% faster",
            Branch = SkillBranch.Infiltrator,
            Tier = 3,
            SkillPointCost = 3,
            RequiredSkillId = "precision_clicking",
            Value = 1.25,
            Icon = "⚡"
        },
        new Skill
        {
            Id = "deadly_precision",
            Name = "Deadly Precision",
            Description = "+10% crit chance, crits deal 15x",
            Branch = SkillBranch.Infiltrator,
            Tier = 4,
            SkillPointCost = 5,
            RequiredSkillId = "combo_master",
            Value = 0.10,
            Icon = "💀"
        },
        new Skill
        {
            Id = "one_with_the_click",
            Name = "One With The Click",
            Description = "x2 click power, +1 auto-click/sec",
            Branch = SkillBranch.Infiltrator,
            Tier = 5,
            SkillPointCost = 8,
            RequiredSkillId = "deadly_precision",
            Value = 2.0,
            Icon = "∞"
        }
    };

    public static Skill? GetById(string id) => AllSkills.FirstOrDefault(s => s.Id == id);

    public static IEnumerable<Skill> GetByBranch(SkillBranch branch) => AllSkills.Where(s => s.Branch == branch);
}
