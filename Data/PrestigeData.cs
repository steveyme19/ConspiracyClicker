namespace ConspiracyClicker.Data;

public class IlluminatiUpgrade
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required string FlavorText { get; init; }
    public required int TokenCost { get; init; }
    public string Icon { get; init; } = "△";
}

public static class PrestigeData
{
    public const double PRESTIGE_THRESHOLD = 1_000_000_000_000; // 1 trillion
    public const double TOKEN_SCALING = 100_000_000_000; // 100 billion base for steeper curve
    public const double TOKEN_POWER = 0.6; // Steeper curve for better first-prestige rewards

    public static readonly List<IlluminatiUpgrade> IlluminatiUpgrades = new()
    {
        new IlluminatiUpgrade
        {
            Id = "pyramid_scheme",
            Name = "Pyramid Scheme",
            Description = "+5% evidence per second permanently",
            FlavorText = "It's not a pyramid, it's a triangle of opportunity.",
            TokenCost = 1,
            Icon = "△"
        },
        new IlluminatiUpgrade
        {
            Id = "secret_handshake",
            Name = "Secret Handshake",
            Description = "+10% click power permanently",
            FlavorText = "Three shakes, two winks, one nod.",
            TokenCost = 1,
            Icon = "🤝"
        },
        new IlluminatiUpgrade
        {
            Id = "new_world_order_discount",
            Name = "New World Order Discount",
            Description = "-10% generator costs permanently",
            FlavorText = "Member benefits include: world domination discounts.",
            TokenCost = 2,
            Icon = "🌐"
        },
        new IlluminatiUpgrade
        {
            Id = "all_seeing_investment",
            Name = "All-Seeing Investment",
            Description = "+1% click power per Illuminati Token owned",
            FlavorText = "Your tokens are always watching... your profits grow.",
            TokenCost = 3,
            Icon = "👁"
        },
        new IlluminatiUpgrade
        {
            Id = "reptilian_dna",
            Name = "Reptilian DNA Injection",
            Description = "x2 evidence per second permanently",
            FlavorText = "Side effects may include: cold blood, forked tongue, ruling the world.",
            TokenCost = 5,
            Icon = "🦎"
        },
        new IlluminatiUpgrade
        {
            Id = "moon_base_alpha",
            Name = "Moon Base Alpha Access",
            Description = "+50% quest rewards permanently",
            FlavorText = "The dark side has excellent Wi-Fi.",
            TokenCost = 5,
            Icon = "🌙"
        },
        new IlluminatiUpgrade
        {
            Id = "time_manipulation",
            Name = "Time Manipulation Device",
            Description = "-25% quest duration permanently",
            FlavorText = "Wibbly wobbly, timey wimey.",
            TokenCost = 8,
            Icon = "⏰"
        },
        new IlluminatiUpgrade
        {
            Id = "starting_evidence",
            Name = "Shadow Government Stipend",
            Description = "Start with 1M evidence after prestige",
            FlavorText = "A small loan of one million evidence.",
            TokenCost = 10,
            Icon = "💰"
        },
        new IlluminatiUpgrade
        {
            Id = "believer_magnetism",
            Name = "Believer Magnetism",
            Description = "+25% believers from all sources",
            FlavorText = "They can't help but follow you.",
            TokenCost = 10,
            Icon = "🧲"
        },
        new IlluminatiUpgrade
        {
            Id = "infinite_tinfoil",
            Name = "Infinite Tinfoil Supply",
            Description = "+1 Tinfoil per minute passively",
            FlavorText = "The Illuminati has excellent suppliers.",
            TokenCost = 15,
            Icon = "◇"
        }
    };

    public static IlluminatiUpgrade? GetById(string id) => IlluminatiUpgrades.FirstOrDefault(u => u.Id == id);

    public static int CalculateTokensEarned(double totalEvidence)
    {
        if (totalEvidence < PRESTIGE_THRESHOLD) return 0;
        // Steeper curve: power of 0.55 instead of 0.5 (sqrt)
        // At 1T: ~3 tokens, 10T: ~12 tokens, 100T: ~45 tokens, 1Q: ~158 tokens
        return (int)Math.Floor(Math.Pow(totalEvidence / TOKEN_SCALING, TOKEN_POWER));
    }
}
