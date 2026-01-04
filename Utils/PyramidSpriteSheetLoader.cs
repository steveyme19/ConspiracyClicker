using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ConspiracyClicker.Utils;

/// <summary>
/// Loads and extracts individual icons from the pyramid evolution sprite sheet.
/// Sprite sheet is 2272x1888 pixels, 6 columns x 5 rows.
/// Contains 24 pyramid levels (0-23), 1 ultimate pyramid (24-25), and 4 eye tiers.
/// </summary>
public static class PyramidSpriteSheetLoader
{
    private static BitmapSource? _spriteSheet;
    private static readonly Dictionary<string, ImageSource> _iconCache = new();

    // Pyramid sprite sheet: 2272x1888, 6 columns x 5 rows
    private const int PyramidIconWidth = 378;
    private const int PyramidIconHeight = 377;

    // Map pyramid level and eye IDs to sprite positions (row, column) - 0-indexed
    private static readonly Dictionary<string, (int row, int col)> SpritePositions = new()
    {
        // Row 0 - Levels 0-5 (Basic to Awakening)
        ["pyramid_level_0"] = (0, 0),
        ["pyramid_level_1"] = (0, 1),
        ["pyramid_level_2"] = (0, 2),
        ["pyramid_level_3"] = (0, 3),
        ["pyramid_level_4"] = (0, 4),
        ["pyramid_level_5"] = (0, 5),

        // Row 1 - Levels 6-11 (Enlightenment)
        ["pyramid_level_6"] = (1, 0),
        ["pyramid_level_7"] = (1, 1),
        ["pyramid_level_8"] = (1, 2),
        ["pyramid_level_9"] = (1, 3),
        ["pyramid_level_10"] = (1, 4),
        ["pyramid_level_11"] = (1, 5),

        // Row 2 - Levels 12-17 (Transcendence)
        ["pyramid_level_12"] = (2, 0),
        ["pyramid_level_13"] = (2, 1),
        ["pyramid_level_14"] = (2, 2),
        ["pyramid_level_15"] = (2, 3),
        ["pyramid_level_16"] = (2, 4),
        ["pyramid_level_17"] = (2, 5),

        // Row 3 - Levels 18-23 (Ascension)
        ["pyramid_level_18"] = (3, 0),
        ["pyramid_level_19"] = (3, 1),
        ["pyramid_level_20"] = (3, 2),
        ["pyramid_level_21"] = (3, 3),
        ["pyramid_level_22"] = (3, 4),
        ["pyramid_level_23"] = (3, 5),

        // Row 4 - Ultimate pyramids + 4 eye tiers
        ["pyramid_level_24"] = (4, 0),
        ["pyramid_level_25"] = (4, 1),

        // 4 eye tiers matching pyramid progression (last 4 cells)
        ["eye_basic"] = (4, 2),      // Green eye - for levels 0-5
        ["eye_golden"] = (4, 3),     // Golden eye - for levels 6-11 and golden power-up
        ["eye_cosmic"] = (4, 4),     // Purple cosmic eye - for levels 12-17
        ["eye_omega"] = (4, 5),      // Ultimate eye - for levels 18+
    };

    private static void EnsureSpriteSheetLoaded()
    {
        if (_spriteSheet != null) return;

        try
        {
            var uri = new Uri("pack://application:,,,/Resources/PyramidSpriteSheet.png", UriKind.Absolute);
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
            System.Diagnostics.Debug.WriteLine($"Failed to load pyramid sprite sheet: {ex.Message}");
        }
    }

    public static ImageSource? GetPyramidIcon(int level)
    {
        level = Math.Clamp(level, 0, 25);
        return GetIcon($"pyramid_level_{level}");
    }

    /// <summary>
    /// Gets the appropriate eye icon based on pyramid level.
    /// Eye tiers match the glow color transitions.
    /// </summary>
    /// <param name="pyramidLevel">Current pyramid level (0-25)</param>
    /// <param name="forceGolden">If true, returns golden eye for power-up regardless of level</param>
    public static ImageSource? GetEyeIcon(int pyramidLevel = 0, bool forceGolden = false)
    {
        // During golden eye power-up, always use golden eye
        if (forceGolden)
            return GetIcon("eye_golden");

        // Select eye tier based on pyramid level - matches glow color thresholds
        string eyeKey = pyramidLevel switch
        {
            <= 11 => "eye_basic",   // Green eye matches green glow (0-11)
            <= 17 => "eye_golden",  // Golden eye matches yellow-green/cyan-green glow (12-17)
            <= 23 => "eye_cosmic",  // Purple cosmic eye matches cyan-blue/purple glow (18-23)
            _ => "eye_omega",       // Ultimate eye matches light purple glow (24+)
        };

        return GetIcon(eyeKey);
    }

    /// <summary>
    /// Legacy overload for compatibility - gets eye based on golden flag only.
    /// </summary>
    public static ImageSource? GetEyeIcon(bool golden)
    {
        return golden ? GetIcon("eye_golden") : GetIcon("eye_basic");
    }

    public static ImageSource? GetIcon(string iconId)
    {
        if (_iconCache.TryGetValue(iconId, out var cached))
            return cached;

        if (!SpritePositions.TryGetValue(iconId, out var position))
            return null;

        EnsureSpriteSheetLoaded();
        if (_spriteSheet == null) return null;

        try
        {
            int x = position.col * PyramidIconWidth;
            int y = position.row * PyramidIconHeight;

            int width = Math.Min(PyramidIconWidth, _spriteSheet.PixelWidth - x);
            int height = Math.Min(PyramidIconHeight, _spriteSheet.PixelHeight - y);

            if (width <= 0 || height <= 0) return null;

            var croppedBitmap = new CroppedBitmap(_spriteSheet, new Int32Rect(x, y, width, height));
            croppedBitmap.Freeze();

            _iconCache[iconId] = croppedBitmap;
            return croppedBitmap;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to extract pyramid icon for {iconId}: {ex.Message}");
            return null;
        }
    }

    public static bool HasSprite(string iconId) => SpritePositions.ContainsKey(iconId);
}
