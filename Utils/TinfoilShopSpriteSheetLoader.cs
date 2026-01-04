using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ConspiracyClicker.Utils;

/// <summary>
/// Loads and extracts individual icons from the tinfoil shop sprite sheet.
/// Sprite sheet is 1632x2624 pixels, 5 columns x 8 rows = 326x328 pixels per icon.
/// </summary>
public static class TinfoilShopSpriteSheetLoader
{
    private static BitmapSource? _spriteSheet;
    private static readonly Dictionary<string, ImageSource> _iconCache = new();

    private const int Columns = 5;
    private const int Rows = 8;
    private const int IconWidth = 326;
    private const int IconHeight = 328;

    // Map shop item IDs to sprite positions (row, column) - 0-indexed
    private static readonly Dictionary<string, (int row, int col)> ShopSpritePositions = new()
    {
        // Row 0 - Tinfoil Armor Progression
        ["tinfoil_hat_basic"] = (0, 0),
        ["tinfoil_hat_reinforced"] = (0, 1),
        ["tinfoil_hat_deluxe"] = (0, 2),
        ["tinfoil_crown"] = (0, 2),
        ["tinfoil_bodysuit"] = (0, 3),
        ["tinfoil_bunker"] = (0, 4),

        // Row 1 - Deep State Contacts
        ["deep_state_contact"] = (1, 0),
        ["deep_state_insider"] = (1, 1),
        ["deep_state_operative"] = (1, 2),
        ["deep_state_director"] = (1, 3),
        ["government_mole"] = (1, 3),
        ["shadow_council_seat"] = (1, 4),
        ["shadow_director"] = (1, 4),

        // Row 2 - Lucky Items
        ["rabbit_foot"] = (2, 0),
        ["lucky_rabbit_foot"] = (2, 0),
        ["four_leaf_clover"] = (2, 1),
        ["horseshoe"] = (2, 2),
        ["lucky_horseshoe"] = (2, 2),
        ["lucky_dice"] = (2, 3),
        ["lucky_guess"] = (2, 3),
        ["educated_guess"] = (2, 4),
        ["wishbone"] = (2, 4),

        // Row 3 - Charisma & Influence
        ["charisma"] = (3, 0),
        ["charisma_training"] = (3, 0),
        ["cult_leadership"] = (3, 1),
        ["hypnosis"] = (3, 2),
        ["mass_hypnosis"] = (3, 2),
        ["mind_control_crown"] = (3, 3),
        ["hivemind_beacon"] = (3, 4),
        ["hivemind_omega"] = (3, 4),
        ["mass_influence"] = (3, 4),

        // Row 4 - Auto-Clickers
        ["clicking_intern"] = (4, 0),
        ["clicking_robot"] = (4, 1),
        ["quantum_clicker"] = (4, 2),
        ["neural_clicker"] = (4, 3),
        ["neural_auto_clicker"] = (4, 3),
        ["hive_mind_clicker"] = (4, 4),
        ["reality_clicker"] = (4, 4),
        ["reality_clicker_supreme"] = (4, 4),

        // Row 5 - Critical Chance
        ["lucky_star"] = (5, 0),
        ["prophetic_vision"] = (5, 1),
        ["third_eye_open"] = (5, 2),
        ["omniscient_clicking"] = (5, 2),
        ["omniscient_critical"] = (5, 2),
        ["fate_dice"] = (5, 3),
        ["critical_mass"] = (5, 4),
        ["perfect_critical"] = (5, 4),

        // Row 6 - Elite Tier
        ["reality_distortion"] = (6, 0),
        ["reality_distortion_aura"] = (6, 0),
        ["reality_anchor"] = (6, 0),
        ["fate_weaver"] = (6, 1),
        ["destiny_manipulator"] = (6, 1),
        ["time_manipulator"] = (6, 2),
        ["probability_engine"] = (6, 3),
        ["probability_manipulator"] = (6, 3),
        ["omniscient_orb"] = (6, 4),
        ["consciousness_amplifier"] = (6, 4),

        // Row 7 - Special Items & Ultra Tier
        ["evidence_magnet"] = (7, 0),
        ["omega_evidence_engine"] = (7, 0),
        ["truth_serum"] = (7, 1),
        ["truth_fabricator"] = (7, 1),
        ["memory_wipe"] = (7, 2),
        ["quest_autopilot"] = (7, 2),
        ["backup_brain"] = (7, 3),
        ["reality_architect"] = (7, 3),
        ["infinite_storage"] = (7, 4),
        ["click_singularity"] = (7, 4),
        ["infinite_clicker_array"] = (7, 4),
        ["illuminati_chairman"] = (7, 4),
        ["cosmic_click_aura"] = (7, 4),
        ["cosmic_click_engine"] = (7, 4),
        ["multiverse_clicker"] = (7, 4),
        ["final_believer_ascension"] = (7, 4),
    };

    private static void EnsureSpriteSheetLoaded()
    {
        if (_spriteSheet != null) return;

        try
        {
            var uri = new Uri("pack://application:,,,/Resources/TinfoilShopSpriteSheet.png", UriKind.Absolute);
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = uri;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();
            _spriteSheet = bitmap;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load tinfoil shop sprite sheet: {ex.Message}");
        }
    }

    public static ImageSource? GetShopIcon(string itemId)
    {
        if (_iconCache.TryGetValue(itemId, out var cached))
            return cached;

        if (!ShopSpritePositions.TryGetValue(itemId, out var position))
            return null;

        EnsureSpriteSheetLoaded();
        if (_spriteSheet == null) return null;

        try
        {
            int x = position.col * IconWidth;
            int y = position.row * IconHeight;

            int width = Math.Min(IconWidth, _spriteSheet.PixelWidth - x);
            int height = Math.Min(IconHeight, _spriteSheet.PixelHeight - y);

            if (width <= 0 || height <= 0) return null;

            var croppedBitmap = new CroppedBitmap(_spriteSheet, new Int32Rect(x, y, width, height));
            croppedBitmap.Freeze();

            _iconCache[itemId] = croppedBitmap;
            return croppedBitmap;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to extract icon for {itemId}: {ex.Message}");
            return null;
        }
    }

    public static bool HasSprite(string itemId) => ShopSpritePositions.ContainsKey(itemId);
    public static IEnumerable<string> GetAvailableItemIds() => ShopSpritePositions.Keys;
}
