using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ConspiracyClicker.Utils;

/// <summary>
/// Loads and extracts individual icons from the upgrade sprite sheet.
/// Sprite sheet is 1696x2528 pixels, 4 columns x 6 rows = 424x421 pixels per icon.
/// </summary>
public static class UpgradeSpriteSheetLoader
{
    private static BitmapSource? _spriteSheet;
    private static readonly Dictionary<string, ImageSource> _iconCache = new();

    private const int Columns = 4;
    private const int Rows = 6;
    private const int IconWidth = 424;
    private const int IconHeight = 421;

    // Map upgrade IDs to sprite positions (row, column) - 0-indexed
    private static readonly Dictionary<string, (int row, int col)> UpgradeSpritePositions = new()
    {
        // Row 0 - Click Power Items
        ["reinforced_tinfoil"] = (0, 0),
        ["magnifying_glass"] = (0, 1),
        ["red_marker"] = (0, 2),
        ["cork_board"] = (0, 3),

        // Row 1 - More Click Power
        ["night_vision"] = (1, 0),
        ["mechanical_keyboard"] = (1, 1),
        ["third_eye_drops"] = (1, 2),
        ["caffeine_iv"] = (1, 3),

        // Row 2 - Multipliers
        ["quantum_fingers"] = (2, 0),
        ["momentum_theory"] = (2, 1),
        ["synergy_doctrine"] = (2, 2),
        ["unified_field"] = (2, 3),

        // Row 3 - Production Boosts
        ["infinite_recursion"] = (3, 0),
        ["viral_momentum"] = (3, 1),
        ["mass_awakening"] = (3, 2),
        ["truth_singularity"] = (3, 3),

        // Row 4 - Generator Specific
        ["red_string_master"] = (4, 0),
        ["neighbor_network"] = (4, 1),
        ["research_grant"] = (4, 2),
        ["blog_empire"] = (4, 3),

        // Row 5 - Advanced Boosts
        ["podcast_network"] = (5, 0),
        ["documentary_deal"] = (5, 1),
        ["satellite_array"] = (5, 2),
        ["shadow_council"] = (5, 3),
    };

    /// <summary>
    /// Ensures the sprite sheet is loaded.
    /// </summary>
    private static void EnsureSpriteSheetLoaded()
    {
        if (_spriteSheet != null) return;

        try
        {
            var uri = new Uri("pack://application:,,,/Resources/UpgradeSpriteSheet.png", UriKind.Absolute);
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
            System.Diagnostics.Debug.WriteLine($"Failed to load upgrade sprite sheet: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets an upgrade icon from the sprite sheet.
    /// </summary>
    /// <param name="upgradeId">The upgrade ID</param>
    /// <returns>The icon ImageSource, or null if not found</returns>
    public static ImageSource? GetUpgradeIcon(string upgradeId)
    {
        // Check cache first
        if (_iconCache.TryGetValue(upgradeId, out var cached))
            return cached;

        // Check if we have a sprite position for this upgrade
        if (!UpgradeSpritePositions.TryGetValue(upgradeId, out var position))
            return null;

        EnsureSpriteSheetLoaded();
        if (_spriteSheet == null) return null;

        try
        {
            // Calculate pixel coordinates
            int x = position.col * IconWidth;
            int y = position.row * IconHeight;

            // Ensure we don't go out of bounds
            int width = Math.Min(IconWidth, _spriteSheet.PixelWidth - x);
            int height = Math.Min(IconHeight, _spriteSheet.PixelHeight - y);

            if (width <= 0 || height <= 0) return null;

            // Create a cropped bitmap
            var croppedBitmap = new CroppedBitmap(_spriteSheet, new Int32Rect(x, y, width, height));
            croppedBitmap.Freeze();

            // Cache and return
            _iconCache[upgradeId] = croppedBitmap;
            return croppedBitmap;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to extract icon for {upgradeId}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Checks if a sprite exists for the given upgrade ID.
    /// </summary>
    public static bool HasSprite(string upgradeId)
    {
        return UpgradeSpritePositions.ContainsKey(upgradeId);
    }

    /// <summary>
    /// Gets all upgrade IDs that have sprites.
    /// </summary>
    public static IEnumerable<string> GetAvailableUpgradeIds()
    {
        return UpgradeSpritePositions.Keys;
    }

    /// <summary>
    /// Preloads all upgrade icons into cache for better performance.
    /// </summary>
    public static void PreloadAllIcons()
    {
        foreach (var upgradeId in UpgradeSpritePositions.Keys)
        {
            GetUpgradeIcon(upgradeId);
        }
    }
}
