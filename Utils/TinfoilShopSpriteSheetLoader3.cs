using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ConspiracyClicker.Utils;

/// <summary>
/// Loads and extracts individual icons from the third tinfoil shop sprite sheet (transcendent/omega tier).
/// Sprite sheet is 3 columns x 5 rows for ultra-powerful items.
/// </summary>
public static class TinfoilShopSpriteSheetLoader3
{
    private static BitmapSource? _spriteSheet;
    private static readonly Dictionary<string, ImageSource> _iconCache = new();

    private const int Columns = 3;
    private const int Rows = 5;
    private const int IconPadding = 10;

    // Map ultra-tier shop item IDs to sprite positions (row, column) - 0-indexed
    private static readonly Dictionary<string, (int row, int col)> ShopSpritePositions = new()
    {
        // Row 0 - Cosmic Click Powers
        ["cosmic_click_aura"] = (0, 0),
        ["multiverse_clicker"] = (0, 1),
        ["click_singularity"] = (0, 2),

        // Row 1 - Reality Powers
        ["infinite_clicker_array"] = (1, 0),
        ["reality_architect"] = (1, 1),
        ["truth_fabricator"] = (1, 2),

        // Row 2 - Destiny & Probability
        ["destiny_manipulator"] = (2, 0),
        ["probability_engine"] = (2, 1),
        ["consciousness_amplifier"] = (2, 2),

        // Row 3 - Mind Powers
        ["hivemind_omega"] = (3, 0),
        ["omniscient_critical"] = (3, 1),
        ["perfect_critical"] = (3, 2),

        // Row 4 - Ultimate Powers
        ["omega_evidence_engine"] = (4, 0),
        ["cosmic_click_engine"] = (4, 1),
        ["final_believer_ascension"] = (4, 2),
    };

    private static void EnsureSpriteSheetLoaded()
    {
        if (_spriteSheet != null) return;

        try
        {
            var uri = new Uri("pack://application:,,,/Resources/TinfoilShopSpriteSheet3.png", UriKind.Absolute);
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
            System.Diagnostics.Debug.WriteLine($"Failed to load tinfoil shop sprite sheet 3: {ex.Message}");
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
            // Calculate actual icon dimensions from sprite sheet
            int actualIconWidth = _spriteSheet.PixelWidth / Columns;
            int actualIconHeight = _spriteSheet.PixelHeight / Rows;

            int x = position.col * actualIconWidth + IconPadding;
            int y = position.row * actualIconHeight + IconPadding;
            int cropWidth = actualIconWidth - (IconPadding * 2);
            int cropHeight = actualIconHeight - (IconPadding * 2);

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
            System.Diagnostics.Debug.WriteLine($"Failed to extract shop icon for {itemId}: {ex.Message}");
            return null;
        }
    }

    public static bool HasSprite(string itemId) => ShopSpritePositions.ContainsKey(itemId);
    public static IEnumerable<string> GetAvailableItemIds() => ShopSpritePositions.Keys;
}
