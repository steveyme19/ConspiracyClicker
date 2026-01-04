using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ConspiracyClicker.Utils;

/// <summary>
/// Loads and extracts individual icons from the second tinfoil shop sprite sheet (ultra tier).
/// Sprite sheet is 2048x2048 pixels, 4 columns x 4 rows = 512x512 pixels per icon.
/// </summary>
public static class TinfoilShopSpriteSheetLoader2
{
    private static BitmapSource? _spriteSheet;
    private static readonly Dictionary<string, ImageSource> _iconCache = new();

    private const int Columns = 4;
    private const int Rows = 4;
    private const int IconWidth = 512;
    private const int IconHeight = 512;
    private const int IconPadding = 8;

    // Map ultra-tier shop item IDs to sprite positions (row, column) - 0-indexed
    private static readonly Dictionary<string, (int row, int col)> ShopSpritePositions = new()
    {
        // Row 0 - Cosmic Clicking Powers
        ["cosmic_click_aura"] = (0, 0),
        ["multiverse_clicker"] = (0, 1),
        ["click_singularity"] = (0, 2),
        ["infinite_clicker_array"] = (0, 3),

        // Row 1 - Reality Manipulation
        ["reality_architect"] = (1, 0),
        ["truth_fabricator"] = (1, 1),
        ["destiny_manipulator"] = (1, 2),
        ["reality_distortion_aura"] = (1, 3),

        // Row 2 - Mind & Consciousness
        ["consciousness_amplifier"] = (2, 0),
        ["hivemind_omega"] = (2, 1),
        ["omniscient_critical"] = (2, 2),
        ["omniscient_clicking"] = (2, 2),
        ["perfect_critical"] = (2, 3),

        // Row 3 - Ultimate Powers
        ["omega_evidence_engine"] = (3, 0),
        ["cosmic_click_engine"] = (3, 1),
        ["final_believer_ascension"] = (3, 2),
        ["transcendent_clicker"] = (3, 3),

        // Aliases for items that can use these icons
        ["reality_clicker"] = (0, 0),
        ["reality_clicker_supreme"] = (0, 1),
        ["illuminati_chairman"] = (3, 2),
        ["hivemind_beacon"] = (2, 1),
    };

    private static void EnsureSpriteSheetLoaded()
    {
        if (_spriteSheet != null) return;

        try
        {
            var uri = new Uri("pack://application:,,,/Resources/TinfoilShopSpriteSheet2.png", UriKind.Absolute);
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
            System.Diagnostics.Debug.WriteLine($"Failed to load tinfoil shop sprite sheet 2: {ex.Message}");
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
            int x = position.col * IconWidth + IconPadding;
            int y = position.row * IconHeight + IconPadding;
            int cropWidth = IconWidth - (IconPadding * 2);
            int cropHeight = IconHeight - (IconPadding * 2);

            int width = Math.Min(cropWidth, _spriteSheet.PixelWidth - x);
            int height = Math.Min(cropHeight, _spriteSheet.PixelHeight - y);

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
