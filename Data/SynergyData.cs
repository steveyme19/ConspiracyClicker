namespace ConspiracyClicker.Data;

/// <summary>
/// One generator boosting another: "every N of the source you own adds Bonus to the target's
/// production, up to Cap".
///
/// Buying used to be a solved problem - the newest generator you could afford was always the
/// right purchase, so the whole shop was a queue rather than a decision. Synergies give a few
/// early and midgame generators a second life: a stack of cheap ones you would otherwise stop
/// buying can be worth more than the next tier up, and the answer changes as prices climb.
/// </summary>
public class Synergy
{
    public required string SourceId { get; init; }   // Generator you own
    public required string TargetId { get; init; }   // Generator that gets faster
    public required int Per { get; init; }           // Source units per step
    public required double Bonus { get; init; }      // Production added per step (0.02 = +2%)
    public required double Cap { get; init; }        // Maximum added production (1.0 = +100%)
    public required string Description { get; init; }
}

public static class SynergyData
{
    public static readonly List<Synergy> AllSynergies = new()
    {
        new Synergy
        {
            SourceId = "red_string", TargetId = "basement_researcher",
            Per = 10, Bonus = 0.04, Cap = 2.0,
            Description = "Every 10 Red String boards make Basement Researchers +4% faster (max +200%)"
        },
        new Synergy
        {
            SourceId = "suspicious_neighbor", TargetId = "discord_server",
            Per = 10, Bonus = 0.05, Cap = 2.5,
            Description = "Every 10 Suspicious Neighbours make Discord Servers +5% faster (max +250%)"
        },
        new Synergy
        {
            SourceId = "blogspot_blog", TargetId = "youtube_channel",
            Per = 10, Bonus = 0.05, Cap = 3.0,
            Description = "Every 10 Blogspot Blogs make YouTube Channels +5% faster (max +300%)"
        },
        new Synergy
        {
            SourceId = "am_radio", TargetId = "podcast",
            Per = 10, Bonus = 0.06, Cap = 3.0,
            Description = "Every 10 AM Radio stations make Podcasts +6% faster (max +300%)"
        },
        new Synergy
        {
            SourceId = "podcast", TargetId = "netflix_doc",
            Per = 10, Bonus = 0.06, Cap = 3.0,
            Description = "Every 10 Podcasts make Netflix Documentaries +6% faster (max +300%)"
        },
        new Synergy
        {
            SourceId = "truth_conference", TargetId = "shadow_government",
            Per = 10, Bonus = 0.05, Cap = 2.5,
            Description = "Every 10 Truth Conferences make Shadow Governments +5% faster (max +250%)"
        },
        new Synergy
        {
            SourceId = "spy_satellite", TargetId = "weather_machine",
            Per = 10, Bonus = 0.06, Cap = 3.0,
            Description = "Every 10 Spy Satellites make Weather Machines +6% faster (max +300%)"
        },
        new Synergy
        {
            SourceId = "mind_control_tower", TargetId = "clone_facility",
            Per = 10, Bonus = 0.06, Cap = 3.0,
            Description = "Every 10 Mind Control Towers make Clone Facilities +6% faster (max +300%)"
        },
        new Synergy
        {
            SourceId = "clone_facility", TargetId = "time_machine",
            Per = 10, Bonus = 0.07, Cap = 3.5,
            Description = "Every 10 Clone Facilities make Time Machines +7% faster (max +350%)"
        },
        new Synergy
        {
            SourceId = "hollow_earth_base", TargetId = "moon_base",
            Per = 10, Bonus = 0.07, Cap = 3.5,
            Description = "Every 10 Hollow Earth Bases make Moon Bases +7% faster (max +350%)"
        },
        new Synergy
        {
            SourceId = "stargate_array", TargetId = "alien_alliance",
            Per = 10, Bonus = 0.08, Cap = 4.0,
            Description = "Every 10 Stargate Arrays make Alien Alliances +8% faster (max +400%)"
        },
        new Synergy
        {
            SourceId = "dimension_portal", TargetId = "simulation_admin",
            Per = 10, Bonus = 0.08, Cap = 4.0,
            Description = "Every 10 Dimension Portals make Simulation Admins +8% faster (max +400%)"
        },
        new Synergy
        {
            SourceId = "quantum_entangler", TargetId = "multiverse_network",
            Per = 10, Bonus = 0.08, Cap = 4.0,
            Description = "Every 10 Quantum Entanglers make Multiverse Networks +8% faster (max +400%)"
        },
        new Synergy
        {
            SourceId = "paradox_engine", TargetId = "omniscience_engine",
            Per = 10, Bonus = 0.10, Cap = 5.0,
            Description = "Every 10 Paradox Engines make Omniscience Engines +10% faster (max +500%)"
        },
        new Synergy
        {
            SourceId = "universe_creator", TargetId = "existence_core",
            Per = 10, Bonus = 0.10, Cap = 5.0,
            Description = "Every 10 Universe Creators make Existence Cores +10% faster (max +500%)"
        }
    };

    private static readonly Dictionary<string, List<Synergy>> ByTarget =
        AllSynergies.GroupBy(s => s.TargetId)
                    .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.Ordinal);

    private static readonly Dictionary<string, List<Synergy>> BySource =
        AllSynergies.GroupBy(s => s.SourceId)
                    .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.Ordinal);

    private static readonly List<Synergy> None = new();

    /// <summary>Synergies that boost this generator. Shared list - do not mutate.</summary>
    public static List<Synergy> GetForTarget(string generatorId) =>
        ByTarget.TryGetValue(generatorId, out var list) ? list : None;

    /// <summary>Synergies this generator feeds. Shared list - do not mutate.</summary>
    public static List<Synergy> GetForSource(string generatorId) =>
        BySource.TryGetValue(generatorId, out var list) ? list : None;

    public static bool HasAny(string generatorId) =>
        ByTarget.ContainsKey(generatorId) || BySource.ContainsKey(generatorId);
}
